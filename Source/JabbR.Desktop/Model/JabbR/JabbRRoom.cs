using System;
using Eto.Forms;
using System.Threading.Tasks;
using System.Linq;
using jab = JabbR.Client;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using jm = JabbR.Models;

namespace JabbR.Desktop.Model.JabbR
{
    public class JabbRRoom : JabbRChannel
    {
        Dictionary<string, User> users = new Dictionary<string, User>();
        List<string> owners = new List<string>();
        TaskCompletionSource<IEnumerable<ChannelMessage>> recentMessages;
        ChannelMessage firstMessage;
        bool historyLoaded;
        TaskCompletionSource<Channel> getChannelInfo = new TaskCompletionSource<Channel>();
        static Image image = Bitmap.FromResource("JabbR.Desktop.Resources.room.png", typeof(JabbRRoom).Assembly);
        
        public override Image Image { get { return image; } }
        
        public override IEnumerable<User> Users { get { return users.Values; } }
        
        public override IEnumerable<string> Owners { get { return owners; } }
        
        public new JabbRServer Server
        {
            get { return base.Server as JabbRServer; }
        }
        
        public JabbRRoom(JabbRServer server, jab.Models.Room room)
            : base(server)
        {
            Set(room);
        }

        void Set(jab.Models.Room room)
        {
            this.Name = room.Name;
            this.Id = room.Name;
            this.Topic = room.Topic;
            this.Private = room.Private;
            
            recentMessages = new TaskCompletionSource<IEnumerable<ChannelMessage>>();
            getChannelInfo = new TaskCompletionSource<Channel>();
        }

        public async Task LoadRoomInfo()
        {
            try
            {
                var roomInfo = await Server.Client.GetRoomInfo(this.Id);
                this.Topic = roomInfo.Topic;
                this.Private = roomInfo.Private;
        
                lock (users)
                {
                    this.users.Clear();
                    foreach (var user in from r in roomInfo.Users select new JabbRUser (this.Server, r))
                        this.users.Add(user.Id, user);
                }
                lock (owners)
                {
                    this.owners.Clear();
                    this.owners.AddRange(roomInfo.Owners);
                }
                IEnumerable<ChannelMessage> messages = (from m in roomInfo.RecentMessages select CreateMessage(m)).ToArray();
                historyLoaded = true;
                if (firstMessage != null)
                {
                    // filter up to the first already received message
                    messages = messages.TakeWhile(r => r.When < firstMessage.When || (r.When == firstMessage.When && r.Content != firstMessage.Content));
                    // must call .ToArray() otherwise firstMessage will be null when this is iterated
                    messages = messages.ToArray();
                    firstMessage = null;
                }
                recentMessages.SetResult(messages);
                getChannelInfo.SetResult(this);
            }
            catch (Exception ex)
            {
                getChannelInfo.SetException(ex);
                recentMessages.SetException(ex);
            }
        }
        
        User GetUser(string id)
        {
            User user;
            lock (users)
            {
                return users.TryGetValue(id, out user) ? user : null;
            }
        }
        
        IEnumerable<User> GetUsers(IEnumerable<jab.Models.User> jabusers)
        {
            foreach (var jabuser in jabusers)
            {
                var user = GetUser(jabuser.Name);
                if (user != null)
                {
                    user.Active = jabuser.Active;
                    user.IsAfk = jabuser.IsAfk;
                    yield return user;
                }
            }
        }
        
        public override void TriggerActivityChanged(IEnumerable<jab.Models.User> users)
        {
            var theusers = GetUsers(users);
            OnUsersActivityChanged(new UsersEventArgs(theusers, DateTimeOffset.Now));
        }
        
        public override async Task<IEnumerable<ChannelMessage>> GetHistory(string fromId)
        {
            if (recentMessages != null)
            {
                Server.MakeRoomLoadNext(this);
                var messages = await recentMessages.Task;
                recentMessages = null;
                return messages;
            }
            else
            {
                var prev = await Server.Client.GetPreviousMessages(fromId);
                return from m in prev select CreateMessage(m);
            }
        }
        
        public override async void SendMessage(string command)
        {
            var message = new jm.ClientMessage{
                Id = Guid.NewGuid ().ToString (),
                Room = this.Name,
                Content = command
            };
            if (!command.TrimStart().StartsWith("/", StringComparison.Ordinal))
            {
                OnMessageReceived(new MessageEventArgs(new ChannelMessage(message.Id, DateTimeOffset.Now, Server.CurrentUser.Name, command)));
            }
            try
            {
                await Server.Client.Send(message);
            }
            catch (Exception ex)
            {
                Application.Instance.Invoke(() => {
                    MessageBox.Show(
                        Application.Instance.MainForm,
                        string.Format("Error sending message: {0}", ex)
                    );
                });
            }
        }
        
        public override void TriggerMessage(ChannelMessage message)
        {
            if (firstMessage == null && !historyLoaded)
                firstMessage = message;
            base.TriggerMessage(message);
        }
        
        public void TriggerMeMessage(string user, string content)
        {
            var message = new MeMessageEventArgs(new MeMessage(DateTimeOffset.Now, user, content));
            OnMeMessageReceived(message);
        }
        
        internal void TriggerUserLeft(UserEventArgs e)
        {
            lock (users)
            {
                if (users.ContainsKey(e.User.Name))
                    users.Remove(e.User.Name);
            }
            OnUserLeft(e);
        }
        
        internal void TriggerUserJoined(UserEventArgs e)
        {
            OnUserJoined(e);
            lock (users)
            {
                var user = GetUser(e.User.Name);
                if (user == null)
                {
                    users.Add(e.User.Id, e.User);
                }
            }
        }
        
        static string MakePaste(string content)
        {
            return string.Format(@"<h3 class=""collapsible_title"">Paste (click to show/hide)</h3><div class=""collapsible_box""><pre class=""multiline"">{0}</pre></div>", content);
        }
        
        static string ParseContent(string content)
        {
            content = WebUtility.HtmlEncode(content);
            if (content.IndexOf('\n') > 0)
            {
                content = MakePaste(content);
            }
            else
            {
                content = Regex.Replace(content, "(https?://[^ \"]+)", "<a href=\"$1\">$1</a>");
                content = Regex.Replace(content, "(?<=^|[ ])#([a-z][a-z0-9-]*)", "<a href=\"#/rooms/$1\">#$1</a>");
            }
            return content;
        }
        
        public ChannelMessage CreateMessage(jab.Models.Message m)
        {
            var content = m.Content;
            if (!m.HtmlEncoded)
                content = ParseContent(content);
            var message = new ChannelMessage(m.Id, m.When, m.User.Name, content);
            message.DetectHighlights(Server.HighlightRegex);
            return message;
        }
        
        public override Task<Channel> GetChannelInfo()
        {
            Server.MakeRoomLoadNext(this);
            return getChannelInfo.Task;
        }

        internal void TriggerOwnerAdded(jab.Models.User user)
        {
            if (!this.owners.Contains(user.Name))
            {
                bool added = false;
                lock (owners)
                {
                    if (!this.owners.Contains(user.Name))
                    {
                        owners.Add(user.Name);
                        added = true;
                    }
                }
                if (added)
                    OnOwnerAdded(new UserEventArgs(new JabbRUser(this.Server, user), DateTimeOffset.Now));
            }
        }

        internal void TriggerOwnerRemoved(jab.Models.User user)
        {
            if (this.owners.Contains(user.Name))
            {
                bool removed = false;
                lock (owners)
                {
                    if (this.owners.Contains(user.Name))
                    {
                        owners.Remove(user.Name);
                        removed = true;
                    }
                }
                if (removed)
                    OnOwnerRemoved(new UserEventArgs(new JabbRUser(this.Server, user), DateTimeOffset.Now));
            }
        }

        internal void TriggerUsernameChanged(string oldUserName, jab.Models.User jabuser)
        {
            var user = GetUser(oldUserName);
            if (user != null)
            {
                lock (users)
                {
                    if (users.ContainsKey(oldUserName))
                    {
                        users.Remove(oldUserName);
                        user.Name = jabuser.Name;
                        user.Id = jabuser.Name;
                        users.Add(user.Id, user);
                    }
                }
                OnUsernameChanged(new UsernameChangedEventArgs(oldUserName, user, DateTimeOffset.Now));
            }
        }
        
        static object typinglock = new object();
        static string lastTypingRoom;
        static DateTime? lastTypingTime;
        
        public override void UserTyping()
        {
            lock (typinglock)
            {
                if (lastTypingRoom != this.Name || (lastTypingTime != null && lastTypingTime.Value < DateTime.Now))
                {
                    lastTypingTime = DateTime.Now.AddSeconds(5);
                    lastTypingRoom = this.Name;
                    Server.Client.SetTyping(this.Name);
                }
            }
        }
        
        public override int CompareTo(object obj)
        {
            if (obj is JabbRChat)
                return -1;
            else
                return base.CompareTo(obj);
        }
    }
}

