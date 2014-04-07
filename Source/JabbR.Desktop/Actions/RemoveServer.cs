using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using JabbR.Desktop.Model;

namespace JabbR.Desktop.Actions
{
    public class RemoveServer : Command
    {
        public const string ActionID = "RemoveServer";
        Channels channels;
        Configuration config;
        
        public RemoveServer(Channels channels, Configuration config)
        {
            this.channels = channels;
            this.config = config;
            this.ID = ActionID;
            this.MenuText = "Remove Server...";
        }
        
        public override bool Enabled
        {
            get
            {
                return base.Enabled && channels.SelectedServer != null;
            }
            set
            {
                base.Enabled = value;
            }
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            var server = channels.SelectedServer;
            if (server != null)
            {
                var ret = MessageBox.Show(Application.Instance.MainForm, string.Format("Are you sure you wish to remove '{0}'?", server.Name), MessageBoxButtons.YesNo);
                if (ret == DialogResult.Yes)
                {
                    config.RemoveServer(server);
                    JabbRApplication.Instance.SaveConfiguration();
                }
            }
        }
    }
}

