using System;

namespace JabbR.Eto.Model
{
	public class UsernameChangedEventArgs : UserEventArgs
	{
		public string OldUsername { get; private set; }
		
		public UsernameChangedEventArgs (string oldName, User user, DateTimeOffset when)
			: base (user, when)
		{
			this.OldUsername = oldName;
		}
	}
}

