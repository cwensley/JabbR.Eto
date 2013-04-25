using System;

namespace JabbR.Desktop.Model
{
    public class ConnectionErrorEventArgs : EventArgs
    {
        public Server Server { get; private set; }
        
        public Exception Exception { get; private set; }
        
        public ConnectionErrorEventArgs(Server server, Exception exception)
        {
            this.Server = server;
            this.Exception = exception;
        }
    }
}

