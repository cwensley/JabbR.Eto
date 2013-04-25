using System;
using JabbR.Eto.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace JabbR.Eto.Interface
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
                return this.Server.Name;
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
        
        protected override Task<IEnumerable<string>> GetAutoCompleteNames(string search)
        {
            var task = new TaskCompletionSource<IEnumerable<string>>();
            if (search.StartsWith("#") && Server.IsConnected)
            {
                search = search.TrimStart('#');
                var getChannels = Server.GetCachedChannels();
                getChannels.ContinueWith(t => {
                    task.TrySetResult(t.Result.Where(r => r.Name.StartsWith(search, StringComparison.CurrentCultureIgnoreCase)).Select(r => r.Name));
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                getChannels.ContinueWith(t => {
                    task.TrySetException(t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            return task.Task;
        }
        
        public override string TranslateAutoCompleteText(string selection, string search)
        {
            return '#' + base.TranslateAutoCompleteText(selection, search) + ' ';
        }
        
    }
}

