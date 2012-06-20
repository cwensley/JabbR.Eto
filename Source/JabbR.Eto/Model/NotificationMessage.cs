using System;

namespace JabbR.Eto.Model
{

	public class NotificationMessage
	{
		public DateTimeOffset When { get; set; }
		
		public string Time { get; set; }

		public string Content { get; set; }

		public NotificationMessage(DateTimeOffset when, string content)
		{
			this.When = when.ToLocalTime ();
			this.Time = this.When.ToString ("h:mm:ss tt");
			this.Content = content;
		}
	}
	
}
