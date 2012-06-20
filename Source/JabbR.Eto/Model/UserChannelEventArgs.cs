using System;
using Eto;
using System.Xml;

namespace JabbR.Eto.Model
{
	public class UserChannelEventArgs : EventArgs
	{
		public User User { get; private set; }
		
		public Channel Channel { get; private set; }
		
		public UserChannelEventArgs(User user, Channel channel)
		{
			this.User = user;
			this.Channel = channel;
		}
	}

}

