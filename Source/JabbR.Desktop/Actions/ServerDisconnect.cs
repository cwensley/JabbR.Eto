using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using JabbR.Desktop.Model;

namespace JabbR.Desktop.Actions
{
    public class ServerDisconnect : ButtonAction
    {
        public const string ActionID = "ServerDisconnect";
        Channels channels;

        public ServerDisconnect(Channels channels)
        {
            this.channels = channels;
            this.ID = ActionID;
            this.MenuText = "Disconnect";
            
        }
        
        public override bool Enabled
        {
            get
            {
                return base.Enabled && channels.SelectedServer != null && channels.SelectedServer.IsConnected;
            }
            set
            {
                base.Enabled = value;
            }
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            var server = channels.SelectedServer;
            if (server != null && server.IsConnected)
            {
                server.Disconnect();
            }
        }
    }
}

