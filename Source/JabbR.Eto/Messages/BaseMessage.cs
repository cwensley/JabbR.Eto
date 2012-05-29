using System;

namespace JabbR.Eto.Messages
{
	public abstract class BaseMessage
	{
		public string Id { get; set; }
		
		public string Time { get; set; }

		public string User { get; set; }

		public string Content { get; set; }

		public bool IsHistory { get; set; }

		public abstract string Type { get; }
		
		public void SetTime(DateTimeOffset time)
		{
			this.Time = time.ToString ("h:MM:ss tt");
		}
	}
}
