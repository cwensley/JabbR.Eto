using System;
using System.Collections.Generic;
using Eto;
using System.Collections.ObjectModel;

namespace JabbR.Eto.Model
{
    public class ServerEventArgs : EventArgs
    {
        public Server Server { get; private set; }
        
        public ServerEventArgs(Server server)
        {
            this.Server = server;
        }
        
    }

}

