using System;

namespace JabbR.Desktop.Model
{
    public class MeMessage : Message
    {
        public override string Type { get { return "memessage"; } }
        
        public string Content { get; set; }
        
        public DateTimeOffset When { get; set; }
        
        public string Time { get; set; }
        
        public string User { get; set; }
        
        public MeMessage(DateTimeOffset when, string userName, string content)
        {
            this.Content = content;
            this.When = when.ToLocalTime();
            this.Time = this.When.ToString("h:mm:ss tt");
            this.User = userName;
        }
    }
}

