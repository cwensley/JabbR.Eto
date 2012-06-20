using System;

namespace JabbR.Eto.Model
{
	public class MessageContent
	{
		public string Id { get; set; }
		
		public string Content { get; set; }
		
		public MessageContent (string id, string content)
		{
			this.Id = id;
			this.Content = content;
		}
	}
}

