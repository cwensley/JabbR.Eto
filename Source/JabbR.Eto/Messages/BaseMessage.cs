using System;

namespace JabbR.Eto.Messages
{
	public abstract class BaseMessage
	{
		public string Time { get; set; }

		public string User { get; set; }

		public string Content { get; set; }

		public bool IsHistory { get; set; }

		public abstract string Type { get; }
	}
}
