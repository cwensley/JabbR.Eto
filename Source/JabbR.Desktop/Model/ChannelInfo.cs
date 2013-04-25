using System;
using Eto.Forms;

namespace JabbR.Desktop.Model
{
    public class ChannelInfo : IGridItem
    {
        public Server Server { get; private set; }
        
        public string Name { get; set; }
        
        public string Topic { get; set; }
        
        public int UserCount { get; set; }
        
        public bool Private { get; set; }
        
        public ChannelInfo(Server server)
        {
            this.Server = server;
        }
    }
}

