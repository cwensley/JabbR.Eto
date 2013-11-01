using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using Eto;
using System.Xml;

namespace JabbR.Desktop.Model
{
    public sealed class MessageEventArgs : EventArgs
    {
        public ChannelMessage Message { get; private set; }
        
        public MessageEventArgs(ChannelMessage message)
        {
            this.Message = message;
        }
    }
    
}
