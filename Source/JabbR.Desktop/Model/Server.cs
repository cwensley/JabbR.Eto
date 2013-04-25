using System;
using Eto;
using System.Xml;
using Eto.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JabbR.Eto.Model.JabbR;
using JabbR.Eto.Interface;
using Eto.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Diagnostics;

namespace JabbR.Eto.Model
{
    public enum ServerState
    {
        Connected,
        Connecting,
        Disconnected,
        Disconnecting
    }

    public abstract class Server : IXmlReadable, ISectionGenerator, ITreeItem
    {
        MemoryCache iconCache = new MemoryCache("server-icons");

        public static Server CreateFromXml(XmlElement element)
        {
            var type = element.GetStringAttribute("type") ?? JabbRServer.JabbRTypeId;
            switch (type)
            {
                case JabbRServer.JabbRTypeId:
                    return new JabbRServer();
                default:
                    return null;
            }
        }

        public bool IsConnected { get { return State == ServerState.Connected; } }
        
        public ServerState State { get; protected set; }

        List<Channel> channels = new List<Channel>();

        public string Id { get; set; }
        
        public abstract string TypeId { get; }
        
        public string Name { get; set; }
        
        public bool ConnectOnStartup { get; set; }

        public User CurrentUser { get; protected set; }
        
        public abstract bool IsAuthenticated { get; }
        
        public IEnumerable<Channel> Channels
        {
            get { return channels; }
        }
        
        public event EventHandler<ConnectionErrorEventArgs> ConnectError;
        
        protected virtual void OnConnectError(ConnectionErrorEventArgs e)
        {
            this.State = ServerState.Disconnected;
            OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage("Could not connect to server {0}. {1}", this.Name, e.Exception.GetBaseException().Message)));
            if (ConnectError != null)
                ConnectError(this, e);
        }

        public event EventHandler<EventArgs> Disconnecting;

        protected virtual void OnDisconnecting(EventArgs e)
        {
            this.State = ServerState.Disconnecting;
            OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage(DateTimeOffset.Now, "Disconnecting...")));
            if (Disconnecting != null)
                Disconnecting(this, e);
        }
            
        public event EventHandler<EventArgs> Disconnected;
        
        protected virtual void OnDisconnected(EventArgs e)
        {
            channels.Clear();
            this.State = ServerState.Disconnected;
            OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage(DateTimeOffset.Now, "Disconnected")));
            if (Disconnected != null)
                Disconnected(this, e);
        }

        public event EventHandler<EventArgs> Connecting;
        
        protected virtual void OnConnecting(EventArgs e)
        {
            this.State = ServerState.Connecting;
            OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage(DateTimeOffset.Now, "Connecting...")));
            if (Connecting != null)
                Connecting(this, e);
        }
        
        public event EventHandler<EventArgs> Connected;

        protected virtual void OnConnected(EventArgs e)
        {
            this.State = ServerState.Connected;
            OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage(DateTimeOffset.Now, "Connected")));
            if (Connected != null)
                Connected(this, e);
        }
        
        public event EventHandler<UserChannelEventArgs> UserJoined;

        protected virtual void OnUserJoined(UserChannelEventArgs e)
        {
            if (UserJoined != null)
                UserJoined(this, e);
        }
        
        public event EventHandler<UserChannelEventArgs> UserLeft;

        protected virtual void OnUserLeft(UserChannelEventArgs e)
        {
            if (UserLeft != null)
                UserLeft(this, e);
        }
        
        public event EventHandler<ChannelEventArgs> CloseChannel;

        protected virtual void OnCloseChannel(ChannelEventArgs e)
        {
            this.channels.Remove(e.Channel);
            if (CloseChannel != null)
                CloseChannel(this, e);
        }
        
        public event EventHandler<NotificationEventArgs> GlobalMessageReceived;

        protected virtual void OnGlobalMessageReceived(NotificationEventArgs e)
        {
            if (GlobalMessageReceived != null)
                GlobalMessageReceived(this, e);
        }

        public event EventHandler<OpenChannelEventArgs> OpenChannel;
        
        protected virtual void OnOpenChannel(OpenChannelEventArgs e)
        {
            if (!channels.Contains(e.Channel))
            {
                channels.Add(e.Channel);
                channels.Sort((x,y) => x.CompareTo(y));
            }
            
            if (OpenChannel != null)
                OpenChannel(this, e);
        }
        
        public event EventHandler<ChannelEventArgs> ChannelInfoChanged;
        
        public virtual void OnChannelInfoChanged(ChannelEventArgs e)
        {
            if (ChannelInfoChanged != null)
                ChannelInfoChanged(this, e);
        }
        
        protected void InitializeChannels(IEnumerable<Channel> channels)
        {
            this.channels.Clear();
            this.channels.AddRange(channels.OrderBy(r => r.Name));
        }
        
        public Server()
        {
            this.State = ServerState.Disconnected;
            this.Id = Guid.NewGuid().ToString();
            ((ITreeItem)this).Expanded = true;
        }
        
        public virtual void ReadXml(XmlElement element)
        {
            this.Id = element.GetStringAttribute("id") ?? Guid.NewGuid().ToString();
            this.Name = element.GetAttribute("name");
            this.ConnectOnStartup = element.GetBoolAttribute("connectOnStartup") ?? true;
        }

        public virtual void WriteXml(XmlElement element)
        {
            element.SetAttribute("id", this.Id);
            element.SetAttribute("name", this.Name);
            element.SetAttribute("connectOnStartup", this.ConnectOnStartup);
        }
        
        public abstract bool Authenticate(Control parent);

        public abstract bool CheckAuthentication(Control parent, bool allowCancel, bool isEditing);
        
        public abstract Task Connect();
        
        public abstract Task Disconnect();

        public abstract void SendMessage(string command);

        public abstract void JoinChannel(string name);

        public abstract void LeaveChannel(Channel channel);
        
        public virtual void GenerateEditControls(DynamicLayout layout, bool isNew)
        {
        }

        public virtual bool PreSaveSettings(Control parent)
        {
            return true;
        }

        public virtual Control GenerateSection()
        {
            return null;
        }

        public virtual void ClearPasswords()
        {
        }
        
        public abstract void StartChat(User user);
        
        public abstract Task<IEnumerable<ChannelInfo>> GetChannelList();
        
        public abstract Task<IEnumerable<ChannelInfo>> GetCachedChannels();
        
        string IListItem.Text { get { return this.Name; } }
        
        string IListItem.Key { get { return this.Id; } }
        
        Image IImageListItem.Image { get { return null; } }
        
        bool ITreeItem<ITreeItem>.Expanded { get; set; }
        
        bool ITreeItem<ITreeItem>.Expandable { get { return true; } }
        
        int IDataStore<ITreeItem>.Count { get { return channels.Count; } }
        
        ITreeItem IDataStore<ITreeItem>.this [int index]
        {
            get { return channels[index]; }
        }

        ITreeItem ITreeItem<ITreeItem>.Parent { get; set; }

        public Bitmap GetUserIcon(User user)
        {
            if (iconCache.Contains(user.Id))
            {
                var icon = iconCache[user.Id] as Bitmap;
                if (icon != null)
                    return icon;
            }

            user.GetIcon().ThenOnUI(image => {
                var policy = new CacheItemPolicy {
                    SlidingExpiration = TimeSpan.FromMinutes (60)
                };
                iconCache.Set(user.Id, image, policy);
                foreach (var channel in this.Channels)
                {
                    if (channel.Users.Any(r => r.Id == user.Id))
                    {
                        channel.TriggerUserIconChanged(new UserImageEventArgs(user, DateTimeOffset.Now, image));
                    }
                }
            });

            return User.DefaultUserIcon;
        }
    }
}

