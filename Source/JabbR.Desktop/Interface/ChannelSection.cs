using System;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using Eto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR.Desktop.Model;
using System.Diagnostics;
using System.Threading;

namespace JabbR.Desktop.Interface
{
    public class ChannelSection : MessageSection
    {
        bool noHistory;
        bool retrievingHistory;
        const string JOIN_ROOM_PREFIX = "?join-room=";
        const string LOAD_HISTORY_PREFIX = "?load-history";
        
        public UserList UserList { get; private set; }
        
        public Channel Channel { get; private set; }
        
        public Server Server { get { return Channel.Server; } }
        
        public override bool SupportsAutoComplete { get { return true; } }
        
        public override bool AllowNotificationCollapsing { get { return true; } }

        public override string TitleLabel
        {
            get
            {
                return "#" + this.Channel.Name;
            }
        }
        
        public ChannelSection(Channel channel)
        {
            this.Channel = channel;
            this.Channel.MessageReceived += HandleMessageReceived;
            this.Channel.UserJoined += HandleUserJoined;
            this.Channel.UserLeft += HandleUserLeft;
            this.Channel.UserIconChanged += HandleUserIconChanged;
            this.Channel.OwnerAdded += HandleOwnerAdded;
            this.Channel.OwnerRemoved += HandleOwnerRemoved;
            this.Channel.UsersActivityChanged += HandleUsersActivityChanged;
            this.Channel.MessageContent += HandleMessageContent;
            this.Channel.TopicChanged += HandleTopicChanged;
            this.Channel.MeMessageReceived += HandleMeMessageReceived;
            this.Channel.UsernameChanged += HandleUsernameChanged;
        }

        void HandleUsernameChanged(object sender, UsernameChangedEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                this.UserList.UsernameChanged(e.OldUsername, e.User);
            });
            AddNotification(new NotificationMessage(DateTimeOffset.Now, "{0}'s nick has changed to {1}.", e.OldUsername, e.User.Name));
        }

        void HandleMeMessageReceived(object sender, MeMessageEventArgs e)
        {
            MeMessage(e.Message);
        }

        void HandleTopicChanged(object sender, EventArgs e)
        {
            AddNotification(new NotificationMessage(DateTimeOffset.Now, "Topic was changed to \"{0}\".", Channel.Topic));
            SetTopic(Channel.Topic);
        }

        void HandleMessageContent(object sender, MessageContentEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                AddMessageContent(e.Content);
            });
        }

        void HandleUsersActivityChanged(object sender, UsersEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                UserList.UsersActivityChanged(e.Users);
            });
        }

        void HandleOwnerRemoved(object sender, UserEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                UserList.OwnerRemoved(e.User);
            });
            AddNotification(new NotificationMessage(
                DateTimeOffset.Now,
                string.Format("{0} was removed as an owner", e.User.Name)
            ));
        }

        void HandleOwnerAdded(object sender, UserEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                UserList.OwnerAdded(e.User);
            });
            AddNotification(new NotificationMessage(
                DateTimeOffset.Now,
                string.Format("{0} was added as an owner", e.User.Name)
            ));
        }

        void HandleUserIconChanged(object sender, UserImageEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                UserList.UserIconChanged(e.User, e.Image);
            });
        }

        void HandleUserLeft(object sender, UserEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                UserList.UserLeft(e.User);
            });
            AddNotification(new NotificationMessage(
                DateTimeOffset.Now,
                string.Format("{0} left {1}", e.User.Name, Channel.Name)
            ));
        }

        void HandleUserJoined(object sender, UserEventArgs e)
        {
            Application.Instance.AsyncInvoke(delegate
            {
                UserList.UserJoined(e.User);
            });
            AddNotification(new NotificationMessage(
                DateTimeOffset.Now,
                string.Format("{0} just entered {1}", e.User.Name, Channel.Name)
            ));
        }

        void HandleMessageReceived(object sender, MessageEventArgs e)
        {
            AddMessage(e.Message);
        }

        protected override Control CreateLayout()
        {
            var split = new Splitter{
                Size = new Size(200, 200),
                Position = 50,
                FixedPanel = SplitterFixedPanel.Panel2
            };
            
            split.Panel1 = base.CreateLayout();
            split.Panel2 = UserList = new UserList(Channel);
            
            return split;
        }
        
        void LoadError(Exception ex, string message)
        {
            Debug.Print("{0} {1}", message, ex);
            StartLive();
            AddNotification(new NotificationMessage("{0} {1}", message, ex != null ? ex.GetBaseException().Message : null));
            SetMarker();
            ReplayDelayedCommands();
            FinishLoad();
        }

        protected override void HandleDocumentLoaded(object sender, WebViewLoadedEventArgs e)
        {
            if (this.Channel != null && Server.IsConnected)
            {
                BeginLoad();
                var getChannelInfo = this.Channel.GetChannelInfo();
                if (getChannelInfo != null)
                {
                    getChannelInfo.ContinueWith(t => {
                        if (t.IsFaulted)
                        {
                            LoadError(t.Exception, "Error getting channel info");
                            return;
                        }
                        var channel = t.Result;
                        Application.Instance.AsyncInvoke(delegate
                        {
                            SetTopic(channel.Topic);
                            UserList.SetUsers(channel.Users);
                            var getHistory = channel.GetHistory(LastHistoryMessageId);
                            if (getHistory != null)
                            {
                                getHistory.ContinueWith(r => {
                                    if (r.IsFaulted)
                                    {
                                        LoadError(r.Exception, "Error getting history");
                                        return;
                                    }
                                    Application.Instance.AsyncInvoke(delegate
                                    {
                                        StartLive();
                                        AddHistory(r.Result, true);
                                        AddNotification(new NotificationMessage(DateTimeOffset.Now, string.Format("You just entered {0}", Channel.Name)));
                                        SetMarker();
                                        ReplayDelayedCommands();

                                        FinishLoad();
                                    });
                                });
                            }
                            else
                                FinishLoad();
                        });
                    });
                }
                else
                    FinishLoad();
            }
            else
            {
                StartLive();
                ReplayDelayedCommands();
                AddNotification(new NotificationMessage(DateTimeOffset.Now, "Disconnected"));
            }
        }

        protected override void HandleAction(WebViewLoadingEventArgs e)
        {
            var historyIndex = e.Uri.LocalPath.IndexOf(LOAD_HISTORY_PREFIX);
            if (historyIndex >= 0)
            {
                LoadHistory();
                return;
            }

            var joinRoomIndex = e.Uri.LocalPath.IndexOf(JOIN_ROOM_PREFIX);
            if (joinRoomIndex >= 0)
            {
                Channel.Server.JoinChannel(e.Uri.PathAndQuery.Substring(joinRoomIndex + JOIN_ROOM_PREFIX.Length));
                return;
            }
            
            base.HandleAction(e);
        }
        
        protected void LoadHistory()
        {
            if (!noHistory && !retrievingHistory)
            {
                retrievingHistory = true;
                //Debug.Print ("**LOADING HISTORY");
                var history = Channel.GetHistory(LastHistoryMessageId);
                history.ContinueWith(t => {
                    //Debug.Print ("History loaded");
                    if (t.Result != null)
                    {
                        noHistory = !t.Result.Any();
                        AddHistory(t.Result);
                    }
                    FinishLoad();
                    retrievingHistory = false;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                
                history.ContinueWith(t => {
                    FinishLoad();
                    retrievingHistory = false;
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            else if (noHistory)
                FinishLoad();
                
        }

        public void MeMessage(MeMessage message)
        {
            SendCommand("addMeMessage", message);
        }
        
        public override void UserTyping()
        {
            Channel.UserTyping();
        }
        
        public override void ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;
            
            Channel.SendMessage(command);
        }

        protected override async Task<IEnumerable<string>> GetAutoCompleteNames(string search)
        {
            if (Channel.Server.IsConnected)
            {
                if (search.StartsWith("#"))
                {
                    search = search.TrimStart('#');
                    var channels = await Channel.Server.GetCachedChannels();
                    return channels.Where(r => r.Name.StartsWith(search, StringComparison.CurrentCultureIgnoreCase)).Select(r => r.Name);
                }
                else
                {
                    search = search.TrimStart('@');
                    return Channel.Users.Where(r => r.Name.StartsWith(search, StringComparison.CurrentCultureIgnoreCase)).Select(r => r.Name);
                }
            }
            return Enumerable.Empty<string>();
        }

        public override string TranslateAutoCompleteText(string selection, string search)
        {
            if (search.StartsWith("#"))
                return '#' + base.TranslateAutoCompleteText(selection, search) + ' ';
            else
                return '@' + base.TranslateAutoCompleteText(selection, search) + ' ';
        }
    }
}

