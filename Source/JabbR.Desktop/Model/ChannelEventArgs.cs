using System;
using Eto;
using System.Xml;
using Eto.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JabbR.Desktop.Model.JabbR;
using JabbR.Desktop.Interface;

namespace JabbR.Desktop.Model
{
    public class ChannelEventArgs : EventArgs
    {
        public Channel Channel { get; private set; }
        
        public ChannelEventArgs(Channel channel)
        {
            this.Channel = channel;
        }
    }

    public class OpenChannelEventArgs : ChannelEventArgs
    {
        public bool ShouldFocus { get; private set; }
        
        public bool NewlyJoined { get; private set; }
        
        public OpenChannelEventArgs(Channel channel, bool shouldFocus, bool newlyJoined)
            : base (channel)
        {
            this.ShouldFocus = shouldFocus;
            this.NewlyJoined = newlyJoined;
        }
    }
    
}

