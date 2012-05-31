using System;

namespace JabbR.Eto.Messages
{
	
	public class ChannelMessage
	{
		public string Id { get; set; }
		
		public DateTimeOffset When { get; set; }
		
		public string Time { get; set; }

		public string User { get; set; }

		public string Content { get; set; }

		public ChannelMessage(string id, DateTimeOffset when, string userName, string content)
		{
			this.Id = id;
			this.When = when.ToLocalTime ();
			this.Time = this.When.ToString ("h:MM:ss tt");
			this.User = userName;
			this.Content = content;
		}
	}
	
}
