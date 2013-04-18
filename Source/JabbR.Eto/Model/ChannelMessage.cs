using System;
using System.Text.RegularExpressions;

namespace JabbR.Eto.Model
{
	public class ChannelMessage : Message
	{
		public string Id { get; set; }

		public override string Type { get { return "message"; } }

		public DateTimeOffset When { get; set; }

		public string Time { get { return When.ToString ("h:mm:ss tt"); } }

		public bool Highlight { get; set; }

		public string User { get; set; }

		public string Content { get; set; }

		public ChannelMessage ()
		{
			Id = Guid.NewGuid ().ToString ();
		}

		public ChannelMessage(string id, DateTimeOffset when, string userName, string content)
		{
			this.Id = id;
			this.When = when.ToLocalTime ();
			this.User = userName;
			this.Content = content;
		}

		public void DetectHighlights(Regex regex)
		{
			if (regex != null)
				this.Highlight = regex.IsMatch(Content);
		}
	}
}
