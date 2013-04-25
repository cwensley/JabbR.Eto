using System;

namespace JabbR.Desktop.Model
{
    public class NotificationMessage : Message
    {
        public override string Type { get { return "notification"; } }
        
        public DateTimeOffset When { get; set; }
        
        public string Time { get; set; }

        public string Content { get; set; }

        public NotificationMessage(DateTimeOffset when, string content, params object[] values)
            : this (when, string.Format (content, values))
        {
            
        }

        public NotificationMessage(string content, params object[] values)
            : this (DateTimeOffset.Now, content, values)
        {
            
        }
            
        public NotificationMessage(DateTimeOffset when, string content)
        {
            this.When = when.ToLocalTime();
            this.Time = this.When.ToString("h:mm:ss tt");
            this.Content = content;
        }
        
        public NotificationMessage(string content)
            : this (DateTimeOffset.Now, content)
        {
            
        }
        
    }
    
}
