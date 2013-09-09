using System;
using jab = JabbR.Client;
using jm = JabbR.Models;
using System.Linq;
using Eto.Forms;
using Newtonsoft.Json;
using System.Threading.Tasks;
using JabbR.Desktop.Interface.JabbR;
using Eto;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNet.SignalR.Client.Transports;
using Microsoft.AspNet.SignalR.Client;
using JabbR.Desktop.Interface.Dialogs;
using Microsoft.AspNet.SignalR.Client.Http;
using System.Threading;
using System.Text.RegularExpressions;

namespace JabbR.Desktop.Model.JabbR
{
    public class JabbRServer : Server
    {
        Timer timer;
        List<JabbRRoom> loadingRooms;
        Regex highlighRegex;
        TaskCompletionSource<object> disconnectTask;
        public const string JabbRTypeId = "jabbr";
        
        public jab.JabbRClient Client { get; private set; }
        
        public jab.Models.LogOnInfo LogOnInfo { get; private set; }
        
        public jab.Models.User CurrentJabbRUser
        { 
            get { return ((JabbRUser)this.CurrentUser).InnerUser; }
        }
        
        public override string TypeId { get { return JabbRTypeId; } }
        
        public string Address { get; set; }
        
        public string UserName { get; set; }

        public string Password { get; set; }
        
        public string UserId { get; set; }

        public bool UseSocialLogin { get; set; }
        
        public string JanrainAppName { get; set; }
        
        public JabbRServer()
        {
            //this.UseSocialLogin = true;
            this.JanrainAppName = "jabbr";
        }

        public Regex HighlightRegex
        {
            get
            {
                if (!IsConnected)
                    return null;

                if (highlighRegex == null)
                    highlighRegex = JabbRApplication.Instance.Configuration.GetHighlightRegex("@" + UserName);
                return highlighRegex;
            }
        }
        
        public override bool IsAuthenticated
        {
            get
            {
                if (UseSocialLogin)
                    return !string.IsNullOrEmpty(UserId);
                else
                    return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
            }
        }
        
        public override async Task Connect()
        {
            OnConnecting(EventArgs.Empty);

            if (string.IsNullOrEmpty(Address))
            {
                OnConnectError(new ConnectionErrorEventArgs(this, new Exception("Address is empty")));
                return;
            }
            if (UseSocialLogin)
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    OnConnectError(new ConnectionErrorEventArgs(this, new NotAuthenticatedException("Not authenticated to this server")));
                    return;
                }
            }
            else if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
            {
                OnConnectError(new ConnectionErrorEventArgs(this, new NotAuthenticatedException("Username or password are not specified")));
                return;
            }

            //ServicePointManager.FindServicePoint (new Uri (Address)).ConnectionLimit = 100;

            // force long polling on mono, until SSE works reliably
            Func<IClientTransport> transport;
            /*if (true) //EtoEnvironment.Platform.IsMono)
                transport = () => new LongPollingTransport();
            else*/
                transport = () => new AutoTransport(new DefaultHttpClient());

            Client = new jab.JabbRClient(Address, null, transport);
#if DEBUG
            var settings = Path.Combine (EtoEnvironment.GetFolderPath (EtoSpecialFolder.ApplicationSettings), "jabbr.log");
            Client.TraceWriter = new TextWriterTraceListener (settings).Writer;
            Client.TraceLevel = TraceLevels.All;
#endif

            if (UseSocialLogin)
            {
                throw new NotSupportedException();
            }

            bool connected = false;
            try
            {
                var logOnInfo = await Client.Connect(UserName, Password);
                highlighRegex = null;
                connected = true;
                HookupEvents();

                this.OnGlobalMessageReceived (new NotificationEventArgs(new NotificationMessage (string.Format ("Using {0} transport", Client.Connection.Transport.Name))));
                var userInfo = await Client.GetUserInfo();
                this.CurrentUser = new JabbRUser(this, userInfo);
                loadingRooms = logOnInfo.Rooms.Select(r => new JabbRRoom(this, r)).ToList();
                InitializeChannels(loadingRooms);

                if (EtoEnvironment.Platform.IsMono) {
                    var keepAliveTime = TimeSpan.FromMinutes(5);
                    if (timer != null)
                        timer.Dispose();
                    timer = new Timer(state => {
                        Client.Send(new jm.ClientMessage{ 
                            Id = Guid.NewGuid().ToString(),
                            Content = string.Format("/where ", this.CurrentUser.Name)
                        });
                    }, null, keepAliveTime, keepAliveTime);
                }

                OnConnected(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.Print(string.Format("Error: {0}", ex.GetBaseException().Message));
                OnConnectError(new ConnectionErrorEventArgs(this, ex));
                if (connected)
                    Client.Disconnect();
            }

            // load all room initial channel info/history
            while (true)
            {
                JabbRRoom room;
                lock (loadingRooms)
                {
                    if (loadingRooms.Count > 0)
                    {
                        room = loadingRooms[0];
                        loadingRooms.Remove(room);
                    }
                    else
                        break;
                }
                //Debug.WriteLine(string.Format("Loading messages for room {0}", room.Name));
                await room.LoadRoomInfo();
            }
        }

        public void MakeRoomLoadNext(JabbRRoom room)
        {
            // bump room to load next
            if (loadingRooms != null)
            {
                lock (loadingRooms)
                {
                    if (loadingRooms.Contains(room))
                    {
                        //Debug.WriteLine(string.Format("Bumping room from {0}", loadingRooms.IndexOf(room)));
                        loadingRooms.Remove(room);
                        loadingRooms.Insert(0, room);
                    }
                }
            }
        }
        
        public override async Task<IEnumerable<ChannelInfo>> GetChannelList()
        {
            var rooms = await Client.GetRooms();
            return rooms.Select(r => new ChannelInfo(this) {
                Name = r.Name,
                Topic = r.Topic,
                Private = r.Private,
                UserCount = r.Count
                }
            );
        }
        
        IEnumerable<ChannelInfo> cachedChannels;
        DateTime? cachedChannelTime;

        public override async Task<IEnumerable<ChannelInfo>> GetCachedChannels()
        {
            if (cachedChannels == null || cachedChannelTime < DateTime.Now)
            {
                var channels = await GetChannelList();

                cachedChannels = channels.ToArray();
                cachedChannelTime = DateTime.Now.Add(new TimeSpan(0, 1, 0)); // re-cache every minute
                return cachedChannels;
            }
            return cachedChannels;
        }
        
        public override Task Disconnect()
        {
            OnDisconnecting(EventArgs.Empty);
            if (Client != null)
            {
                disconnectTask = new TaskCompletionSource<object>();
                Task.Run(() => {
                    try
                    {
                        Client.Disconnect();
                    }
                    finally
                    {
                        disconnectTask.TrySetResult(null);
                    }
                });
                return disconnectTask.Task;
            }
            return Task.FromResult<object>(null);
        }
        
        public JabbRRoom GetRoom(string roomName)
        {
            return Channels.OfType<JabbRRoom>().FirstOrDefault(r => r.Name == roomName);
        }

        public JabbRChat GetChat(string userName)
        {
            return Channels.OfType<JabbRChat>().FirstOrDefault(r => r.Name == userName);
        }
        
        void HookupEvents()
        {
            Client.MessageReceived += (message, room) => {
                Debug.Print("MessageReceived, Room: {3}, When: {0}, User: {1}, Content: {2}", message.When, message.User.Name, message.Content, room);
                var channel = GetRoom(room);
                if (channel != null)
                {
                    channel.TriggerMessage(channel.CreateMessage(message));
                    OnChannelInfoChanged(new ChannelEventArgs(channel));
                }
            };
            Client.Disconnected += () => {
                Debug.Print("Disconnected");
                if (disconnectTask != null)
                    disconnectTask.TrySetResult(null);
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
                OnDisconnected(EventArgs.Empty);
            };
            Client.FlagChanged += (user, flag) => {
                Debug.Print("FlagChanged, User: {0}, Flag: {1}", user.Name, flag);
            };
            Client.GravatarChanged += (user, gravatar) => {
                Debug.Print("GravatarChanged, User: {0}, Gravatar: {1}", user.Name, gravatar);
            };
            Client.Kicked += (user) => {
                Debug.Print("Kicked: {0}", user);
            };
            Client.LoggedOut += (rooms) => {
                Debug.Print("LoggedOut, Rooms: {0}", string.Join(", ", rooms));
            };
            Client.MeMessageReceived += (user, content, room) => {
                Debug.Print("MeMessageReceived, User: {0}, Content: {1}, Room: {2}", user, content, room);
                var channel = GetRoom(room);
                if (channel != null) 
                    channel.TriggerMeMessage(user, content);
            };
            Client.NoteChanged += (user, room) => {
                Debug.Print("NoteChanged, User: {0}, Room: {1}", user.Name, room);
            };
            Client.OwnerAdded += (user, room) => {
                Debug.Print("OwnerAdded, User: {0}, Room: {1}", user.Name, room);
                var channel = GetRoom(room);
                if (channel != null) 
                    channel.TriggerOwnerAdded(user);
            };
            Client.OwnerRemoved += (user, room) => {
                Debug.Print("OwnerRemoved, User: {0}, Room: {1}", user.Name, room);
                var channel = GetRoom(room);
                if (channel != null) 
                    channel.TriggerOwnerRemoved(user);
            };
            Client.PrivateMessage += (from, to, message) => {
                Debug.Print("PrivateMessage, From: {0}, To: {1}, Message: {2} ", from, to, message);
                
                var user = from == this.CurrentUser.Name ? to : from;
                JabbRChat chat;
                if (InternalStartChat(new JabbRUser(this, user) { Active = true }, false, message, out chat))
                {
                    chat.TriggerMessage(new ChannelMessage(Guid.NewGuid().ToString(), DateTimeOffset.Now, from, message));
                    OnChannelInfoChanged(new ChannelEventArgs(chat));
                }
            };
            Client.RoomChanged += (room) => {
                Debug.Print("RoomChanged, Room: {0}", room.Name);
            };
            Client.TopicChanged += (roomName, topic, who) => {
                Debug.Print("TopicChanged, Room: {0}, Topic: {1}", roomName, topic);
                var channel = GetRoom(roomName);
                if (channel != null)
                {
                    channel.SetNewTopic(topic, who);
                }
            };
            Client.UserActivityChanged += (user) => {
                Debug.Print("UserActivityChanged, User: {0}, Activity: {1}", user.Name, user.Active);
                foreach (var channel in this.Channels.OfType<JabbRChannel>())
                {
                    channel.TriggerActivityChanged(new jab.Models.User[] { user });
                }
            };
            Client.JoinedRoom += (room) => {
                Debug.Print("JoinedRoom, Room: {0}", room.Name);
                var channel = GetRoom(room.Name);
                bool newlyJoined = false;
                if (channel == null)
                {
                    channel = new JabbRRoom(this, room);
                    channel.LoadRoomInfo().Start();
                    newlyJoined = true;
                }
                OnOpenChannel(new OpenChannelEventArgs(channel, true, newlyJoined));
            };
            Client.UserJoined += (user, room, isOwner) => {
                Debug.Print("UserJoined, User: {0}, Room: {1}", user.Name, room);
                var channel = GetRoom(room);
                if (channel != null)
                {
                    if (isOwner)
                        channel.TriggerOwnerAdded(user);
                    channel.TriggerUserJoined(new UserEventArgs(new JabbRUser(this, user), DateTimeOffset.Now));
                }
            };
            Client.UserLeft += (user, room) => {
                Debug.Print("UserLeft, User: {0}, Room: {1}", user.Name, room);
                var channel = GetRoom(room);
                if (channel != null)
                {
                    if (user.Name == CurrentJabbRUser.Name)
                    {
                        channel.TriggerClosed(EventArgs.Empty);
                        OnCloseChannel(new ChannelEventArgs(channel));
                    }
                    else
                    {
                        channel.TriggerUserLeft(new UserEventArgs(new JabbRUser(this, user), DateTimeOffset.Now));
                    }
                }
            };
            Client.UsernameChanged += (oldUserName, user, room) => {
                Debug.Print("UsernameChanged, OldUserName: {0}, NewUserName: {1}, Room: {2}", oldUserName, user.Name, room);
                if (oldUserName == CurrentUser.Name)
                {
                    // current user, so update all chat channels with new name as well
                    CurrentUser.Name = user.Name;
                    foreach (var chat in Channels.OfType<JabbRChat> ())
                    {
                        chat.TriggerUsernameChanged(oldUserName, user, true);
                    }
                }
                else
                {
                    var chat = GetChat(oldUserName);
                    if (chat != null)
                    {
                        chat.TriggerUsernameChanged(oldUserName, user, false);
                    }
                }
                
                var channel = GetRoom(room);
                if (channel != null)
                {
                    channel.TriggerUsernameChanged(oldUserName, user);
                }
            };
            Client.UsersInactive += (users) => {
                Debug.Print("UsersInactive, Users: {0}", string.Join(", ", users.Select(r => r.Name)));
                foreach (var channel in this.Channels.OfType<JabbRChannel>())
                {
                    channel.TriggerActivityChanged(users);
                }
            };
            Client.UserTyping += (user, room) => {
                Debug.Print("UserTyping, User: {0}, Room: {1}", user.Name, room);   
            };
            Client.AddMessageContent += (messageId, content, room) => {
                Debug.Print("AddMessageContent, Id: {0}, Room: {1}, Content: {2}", messageId, room, content);
                var channel = GetRoom(room);
                if (channel != null)
                    channel.TriggerMessageContent(messageId, content);
            };
            Client.StateChanged += (status) => {
                Debug.Print("StateChange, Old State: {0}, New State: {1}", status.OldState, status.NewState);
                /*if (this.IsConnected && status.NewState == ConnectionState.Disconnected) {
                    OnDisconnected(EventArgs.Empty);
                }*/
            };
        }
        
        public override async void SendMessage(string command)
        {
            if (Client == null)
            {
                OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage(DateTimeOffset.Now, "Cannot send command. You are not connected.")));
                return;
            }
            var message = new jm.ClientMessage{
                Id = Guid.NewGuid ().ToString (),
                Content = command
            };
            try
            {
                await Client.Send(message);
            }
            catch (Exception ex)
            {
                Application.Instance.Invoke(() => {
                    MessageBox.Show(Application.Instance.MainForm, string.Format("Error sending message: {0}", ex));
                });
            }
            ;
        }
        
        public override void JoinChannel(string name)
        {
            var room = GetRoom(name);
            if (room == null)
                Client.JoinRoom(name);
            else
                OnOpenChannel(new OpenChannelEventArgs(room, true, false));
        }
        
        public override void LeaveChannel(Channel channel)
        {
            var chat = channel as JabbRChat;
            if (chat != null)
            {
                chat.TriggerClosed(EventArgs.Empty);
                OnCloseChannel(new ChannelEventArgs(chat));
            }
            else
                Client.LeaveRoom(channel.Name);
        }
        
        public override void StartChat(User user)
        {
            JabbRChat chat;
            InternalStartChat(new JabbRUser(user), true, null, out chat);
        }
        
        bool InternalStartChat(User user, bool shouldFocus, string initialMessage, out JabbRChat chat)
        {
            if (user.Name == this.CurrentUser.Name)
            {
                chat = null;
                return false;
            }
            
            chat = GetChat(user.Name);
            if (chat == null)
            {
                chat = new JabbRChat(this, user, initialMessage);
                OnOpenChannel(new OpenChannelEventArgs(chat, shouldFocus, true));
                return false;
            }
            return true;
            
        }
        
        public override void GenerateEditControls(DynamicLayout layout, bool isNew)
        {
            base.GenerateEditControls(layout, isNew);
            new JabbRServerEdit(this, layout);
        }
        
        public override Control GenerateSection()
        {
            var section = new Interface.ServerSection(this);
            section.Initialize();
            return section;
        }
        
        public override bool Authenticate(Control parent)
        {
            if (this.UseSocialLogin)
            {
                var dlg = new JabbRAuthDialog(this.Address, this.JanrainAppName);
                var result = dlg.ShowDialog(parent);
                if (result == DialogResult.Ok)
                {
                    this.UserId = dlg.UserID;
                    return true;
                }
            }
            else
            {
                var dialog = new ServerDialog(this, false, false);
                dialog.DisplayMode = DialogDisplayMode.Attached;
                var ret = dialog.ShowDialog(Application.Instance.MainForm);
                return ret == DialogResult.Ok;
            }
            return false;
        }

        public void TriggerChannelInfoChanged(ChannelEventArgs e)
        {
            OnChannelInfoChanged(e);
        }
        
        public override bool CheckAuthentication(Control parent, bool allowCancel, bool isEditing)
        {
            if (!IsAuthenticated)
            {
                if (!isEditing || UseSocialLogin)
                {
                    var result = MessageBox.Show(parent, "You have not authenticated with this server. Do you want to authenticate now?", allowCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                        return this.Authenticate(parent);
                    else if (allowCancel)
                        return result == DialogResult.No;
                    else
                        return false;
                }
                else if (!UseSocialLogin)
                {
                    var result = MessageBox.Show(parent, "You have not entered your user name and password. Are you sure you want to continue?", MessageBoxButtons.YesNo);
                    return result == DialogResult.Yes;
                }
            }
            return true;
        }

        public override void ClearPasswords()
        {
            base.ClearPasswords();
            JabbRApplication.Instance.EncryptString(this.Address, this.Id + "-user", null);
            JabbRApplication.Instance.EncryptString(this.Address, this.Id + "-pass", null);
        }

        public override void ReadXml(System.Xml.XmlElement element)
        {
            base.ReadXml(element);
            this.UseSocialLogin = false; //element.GetBoolAttribute ("useSocialLogin") ?? false;
            this.Address = element.GetStringAttribute("address");
            if (this.UseSocialLogin)
            {
                this.JanrainAppName = element.GetStringAttribute("janrainAppName") ?? "jabbr";
                this.UserId = JabbRApplication.Instance.DecryptString(this.Address, this.Id, element.GetStringAttribute("userId"));
            }
            else
            {
                this.UserName = JabbRApplication.Instance.DecryptString(this.Address, this.Id + "-user", element.GetStringAttribute("userName"));
                this.Password = JabbRApplication.Instance.DecryptString(this.Address, this.Id + "-pass", element.GetStringAttribute("password"));
            }
        }
        
        public override void WriteXml(System.Xml.XmlElement element)
        {
            base.WriteXml(element);
            element.SetAttribute("useSocialLogin", this.UseSocialLogin);
            element.SetAttribute("address", this.Address);
            if (this.UseSocialLogin)
            {
                element.SetAttribute("janrainAppName", JanrainAppName);
                element.SetAttribute("userId", JabbRApplication.Instance.EncryptString(this.Address, this.Id, this.UserId));
            }
            else
            {
                element.SetAttribute("userName", JabbRApplication.Instance.EncryptString(this.Address, this.Id + "-user", this.UserName));
                element.SetAttribute("password", JabbRApplication.Instance.EncryptString(this.Address, this.Id + "-pass", this.Password));
            }
        }
    }
}

