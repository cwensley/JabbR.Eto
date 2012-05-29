using System;

namespace JabbR.Eto.Messages
{
	
	public class ChannelMessage : BaseMessage
	{
		public override string Type { get { return "message"; } }
		
		public ChannelMessage(string id, DateTimeOffset time, string userName, string content)
		{
			this.Id = id;
			SetTime (time);
			this.User = userName;
			this.Content = content;
		}
		
	}
	
}
