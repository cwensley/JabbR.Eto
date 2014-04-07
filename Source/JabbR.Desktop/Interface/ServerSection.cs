using System;
using JabbR.Desktop.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace JabbR.Desktop.Interface
{
    public class ServerSection : MessageSection
    {
        public Server Server { get; private set; }
        
        public override bool SupportsAutoComplete
        {
            get
            {
                return true;
            }
        }

        public override string TitleLabel
        {
            get
            {
                return Server.Name;
            }
        }
        
        public ServerSection(Server server)
        {
            this.Server = server;
            this.Server.GlobalMessageReceived += HandleGlobalMessageReceived;
        }

        void HandleGlobalMessageReceived(object sender, NotificationEventArgs e)
        {
            AddNotification(e.Message);
        }
        
        public override void ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;
            
            Server.SendMessage(command);
        }
        
        protected override async Task<IEnumerable<string>> GetAutoCompleteNames(string search)
        {
            if (search.StartsWith("#", StringComparison.Ordinal) && Server.IsConnected)
            {
                search = search.TrimStart('#');
                var channels = await Server.GetCachedChannels();
                return channels.Where(r => r.Name.StartsWith(search, StringComparison.CurrentCultureIgnoreCase)).Select(r => r.Name);
            }
            return Enumerable.Empty<string>();
        }
        
        public override string TranslateAutoCompleteText(string selection, string search)
        {
            return '#' + base.TranslateAutoCompleteText(selection, search) + ' ';
        }
        
    }
}

