using System;
using Eto.Forms;
using JabbR.Eto.Interface;
using Eto;
using System.Xml;

namespace JabbR.Eto.Model
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
