using System;
using Eto.Forms;
using System.Threading.Tasks;
using System.Linq;
using jab = JabbR.Client;
using System.Collections.Generic;
using System.Diagnostics;

namespace JabbR.Eto.Model.JabbR
{
    public abstract class JabbRChannel : Channel
    {
        public new JabbRServer Server
        {
            get { return base.Server as JabbRServer; }
        }
        
        public JabbRChannel(JabbRServer server)
            : base(server)
        {
        }
        
        public override Task<Channel> GetChannelInfo()
        {
            var tcs = new TaskCompletionSource<Channel>();
            tcs.SetResult(this);
            return tcs.Task;
        }

        internal void TriggerClosed(EventArgs e)
        {
            OnClosed(e);
        }
        
        public virtual void TriggerMessage(ChannelMessage message)
        {
            UnreadCount ++;
            OnMessageReceived(new MessageEventArgs(message));
        }
        
        public abstract void TriggerActivityChanged(IEnumerable<jab.Models.User> users);
        
        public virtual void TriggerMessageContent(string messageId, string content)
        {
            OnMessageContent(new MessageContentEventArgs(new MessageContent(messageId, content)));
        }
        
        public void SetNewTopic(string topic)
        {
            this.Topic = topic;
            OnTopicChanged(EventArgs.Empty);
        }
    }
}

