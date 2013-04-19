using System;

namespace JabbR.Eto.Model
{
    public class MessageContentEventArgs : EventArgs
    {
        public MessageContent Content { get; private set; }
        
        public MessageContentEventArgs(MessageContent content)
        {
            this.Content = content;
        }
    }
}

