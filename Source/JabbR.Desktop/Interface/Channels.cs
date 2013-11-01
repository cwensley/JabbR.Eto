using System;
using Eto.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JabbR.Desktop.Model;
using System.Collections.Specialized;
using System.Threading;
using Eto.Drawing;

namespace JabbR.Desktop.Interface
{
    public class Channels : Panel
    {
        TreeView channelList;
        TreeItemCollection servers = new TreeItemCollection();
        Dictionary<Tuple<Server,string>, Control> sectionCache = new Dictionary<Tuple<Server,string>, Control>();
        
        public Configuration Config { get; private set; }
        
        public event EventHandler<EventArgs> ChannelChanged;
        
        public Channel SelectedChannel
        {
            get
            {
                return channelList.SelectedItem as Channel;
            }
        }
        
        public Server SelectedServer
        { 
            get
            {
                if (SelectedChannel != null)
                    return SelectedChannel.Server;
                else
                    return channelList.SelectedItem as Server;
            }
        }
        
        protected virtual void OnChannelChanged(EventArgs e)
        {
            if (ChannelChanged != null)
                ChannelChanged(this, e);
            var selected = SelectedChannel;
            if (selected != null)
                selected.ResetUnreadCount();
            SetUnreadCount();
        }
        
        public Channels(Configuration config)
        {
            this.Config = config;
            channelList = new TreeView { Style = "channelList" };
            channelList.DataStore = servers;
            channelList.SelectionChanged += (sender, e) => {
                OnChannelChanged(e);
            };
            channelList.Activated += HandleActivated;
            
            config.ServerAdded += HandleServerAdded;
            config.ServerRemoved += HandleServerRemoved;
            
            this.AddDockedControl(channelList);
        }
        
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.ParentWindow.GotFocus += (sender, ee) => {
                var selected = SelectedChannel;
                if (selected != null)
                {
                    selected.ResetUnreadCount();
                    SetUnreadCount();
                    Update();
                }
            };
            this.ParentWindow.LostFocus += (sender, ee) => {
                var section = GetCurrentSection();
                if (section != null)
                    section.SetMarker();
            };
        }
        
        void HandleActivated(object sender, TreeViewItemEventArgs e)
        {
            var server = e.Item as Server;
            if (server != null)
            {
                var action = new Actions.EditServer(this);
                action.Activate();
            }
        }
        
        public new void Initialize()
        {
            servers.Clear();
            servers.AddRange(Config.Servers);
            foreach (var server in Config.Servers)
            {
                Register(server);
            }
            Update(true);
            channelList.SelectedItem = servers.FirstOrDefault();
        }
        
        void Register(Server server)
        {
            server.Connected += HandleConnected;
            server.ConnectError += HandleConnectError;
            server.Disconnected += HandleDisconnected;
            server.Disconnecting += HandleDisconnecting;
            server.OpenChannel += HandleOpenChannel;
            server.CloseChannel += HandleCloseChannel;
            server.ChannelInfoChanged += HandleChannelInfoChanged;
            CreateSection(server, server);
        }
        
        void UnRegister(Server server)
        {
            server.Disconnecting -= HandleDisconnecting;
            server.Disconnected -= HandleDisconnected;
            server.Connected -= HandleConnected;
            server.ConnectError -= HandleConnectError;
            server.OpenChannel -= HandleOpenChannel;
            server.CloseChannel -= HandleCloseChannel;
            server.ChannelInfoChanged -= HandleChannelInfoChanged;
        }
        
        void HandleConnectError(object sender, ConnectionErrorEventArgs e)
        {
            GetServerSection(e.Server);
            if (e.Exception is NotAuthenticatedException)
            {
                if (e.Server.Authenticate(this))
                {
                    JabbRApplication.Instance.SaveConfiguration();
                    e.Server.Connect();
                }
            }
            else
            {
                MessageBox.Show(this, e.Exception.GetBaseException().Message, string.Format("Could not connect to server {0}. {1}", e.Server.Name));
            }
        }
        
        void HandleDisconnecting(object sender, EventArgs e)
        {
            var server = sender as Server;
            Application.Instance.Invoke(delegate
            {
                var selectedChannel = channelList.SelectedItem as Channel;
                if (selectedChannel != null && selectedChannel.Server == server)
                {
                    channelList.SelectedItem = server;
                }

                Update(false);
            });
        }

        void HandleDisconnected(object sender, EventArgs e)
        {
            var server = sender as Server;
            Application.Instance.AsyncInvoke(delegate
            {
                Update(false);
                if (channelList.SelectedItem == null)
                    channelList.SelectedItem = server;
                RemoveSections(server);
            });
        }
        
        void HandleCloseChannel(object sender, ChannelEventArgs e)
        {
            Application.Instance.Invoke(delegate
            {
                var isSelected = channelList.SelectedItem == e.Channel;
                RemoveSection(e.Channel.Server, e.Channel);
                
                Update();
                if (isSelected)
                    channelList.SelectedItem = e.Channel.Server;
                
                var serverSection = GetServerSection(e.Channel.Server);
                serverSection.AddNotification(new NotificationMessage("You have left {0}", e.Channel.Name));
            });
        }
        
        void HandleOpenChannel(object sender, OpenChannelEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                CreateSection(e.Channel.Server, e.Channel);
                Update();
                
                if (e.ShouldFocus)
                    channelList.SelectedItem = e.Channel;
                
                var serverSection = GetServerSection(e.Channel.Server);
                serverSection.AddNotification(new NotificationMessage("Joined {0}", e.Channel.Name));
            });
        }
        
        void HandleChannelInfoChanged(object sender, ChannelEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                if (SelectedChannel == e.Channel && this.ParentWindow.HasFocus)
                {
                    e.Channel.ResetUnreadCount();
                }
                SetUnreadCount();
                Update();
            });
        }
        
        void SetUnreadCount()
        {
            var count = this.EnumerateChannels().Sum(r => r.UnreadCount);
            
            var form = this.ParentWindow as MainForm;
            if (form != null)
            {
                var section = this.GetCurrentSection();
                form.SetUnreadCount(section != null ? section.TitleLabel : null, count);
            }
        }
        
        void HandleConnected(object sender, EventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                var server = sender as Server;
                if (!server.Channels.Any())
                {
                    var action = new Actions.ChannelList(this);
                    action.Activate();
                }
                else
                {
                    foreach (var channel in server.Channels)
                    {
                        CreateSection(channel.Server, channel);
                    }
                }
                Update();
            });
        }
        
        void HandleServerRemoved(object sender, ServerEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                UnRegister(e.Server);
                servers.Remove(e.Server);
                Update(true);
                OnChannelChanged(EventArgs.Empty);
            });
        }
        
        void HandleServerAdded(object sender, ServerEventArgs e)
        {
            Application.Instance.Invoke(delegate
            {
                servers.Add(e.Server);
                Register(e.Server);
                Update(true);
                channelList.SelectedItem = e.Server;
                OnChannelChanged(EventArgs.Empty);
            });
        }
        
        void RemoveSection(Server server, ITreeItem item)
        {
            var generator = item as ISectionGenerator;
            if (generator != null)
            {
                var key = new Tuple<Server, string>(server, item.Key);
                if (sectionCache.ContainsKey(key))
                {
                    sectionCache.Remove(key);
                }
            }
        }

        void RemoveSections(Server server)
        {
            var serverSection = GetServerSection(server);
            var items = sectionCache.Where(r => r.Key.Item1 == server && r.Value != serverSection).Select(r => r.Key).ToArray();
            foreach (var item in items)
            {
                sectionCache.Remove(item);
            }
        }
        
        MessageSection GetServerSection(Server server)
        {
            if (server == null)
                return null;
            return CreateSection(server, server) as MessageSection;
        }
        
        MessageSection GetChannelSection(Channel channel)
        {
            if (channel == null)
                return null;
            return CreateSection(channel.Server, channel) as MessageSection;
        }
        
        public MessageSection GetCurrentSection()
        {
            var current = channelList.SelectedItem;
            if (current == null)
                return null;
            var server = current as Server;
            if (server == null && current is Channel)
                server = ((Channel)current).Server;
            return CreateSection(server, current) as MessageSection;
        }
        
        Control CreateSection(Server server, ITreeItem item)
        {
            var generator = item as ISectionGenerator;
            
            Control section = null;
            if (generator != null)
            {
                var key = new Tuple<Server, string>(server, item.Key);
                if (!sectionCache.TryGetValue(key, out section))
                {
                    section = generator.GenerateSection();
                    
                    sectionCache.Add(key, section);
                }
            }
            
            return section;
        }
        
        IEnumerable<Server> EnumerateServers()
        {
            return Config.Servers.OrderBy(r => r.Name);
        }
        
        IEnumerable<Channel> EnumerateChannels()
        {
            foreach (var item in EnumerateServers ())
            {
                foreach (var channel in item.Channels)
                {
                    yield return channel;
                }
            }
        }
        
        public void JoinChannel(Server server, string name)
        {
            var channel = server.Channels.FirstOrDefault(r => r.Name == name);
            if (channel != null)
                channelList.SelectedItem = channel;
            else
            {
                server.JoinChannel(name);
            }
        }
        
        void NavigateChannel(bool unreadOnly, bool reverse)
        {
            var channels = EnumerateChannels();
            var listOfItems = channels;
            var channel = channelList.SelectedItem as Channel;
            if (channel != null)
                listOfItems = channels.SkipWhile(r => r != channel).Skip(1).Union(channels.TakeWhile(r => r != channel));
            
            if (reverse)
                listOfItems = listOfItems.Reverse();
            
            var next = listOfItems.FirstOrDefault(r => !unreadOnly || r.UnreadCount > 0);
            if (next != null)
                channelList.SelectedItem = next;
        }
        
        public void GoToNextChannel(bool unreadOnly)
        {
            NavigateChannel(unreadOnly, false);
        }
        
        public void GoToPreviousChannel(bool unreadOnly)
        {
            NavigateChannel(unreadOnly, true);
        }
        
        void Update(bool sort = false)
        {
            if (sort)
                servers.Sort((x,y) => x.Text.CompareTo(y.Text));
            channelList.RefreshData();
        }
        
        public void CreateActions(GenerateActionArgs args)
        {
            args.Actions.Add(new Actions.NextChannel(this));
            args.Actions.Add(new Actions.NextUnreadChannel(this));
            args.Actions.Add(new Actions.PrevChannel(this));
            args.Actions.Add(new Actions.PrevUnreadChannel(this));
            args.Actions.Add(new Actions.LeaveChannel(this));
            
            var channel = args.Menu.GetSubmenu("&Channel", 800);
            
            channel.Actions.Add(Actions.NextChannel.ActionID);
            channel.Actions.Add(Actions.NextUnreadChannel.ActionID);
            channel.Actions.Add(Actions.PrevChannel.ActionID);
            channel.Actions.Add(Actions.PrevUnreadChannel.ActionID);
            channel.Actions.AddSeparator();
            
            channel.Actions.Add(Actions.LeaveChannel.ActionID);
        }
    }
    
}

