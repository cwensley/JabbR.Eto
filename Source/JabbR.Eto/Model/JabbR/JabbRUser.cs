using System;
namespace JabbR.Eto.Model.JabbR
{
	public class JabbRUser : User
	{
		public global::JabbR.Client.Models.User InnerUser { get; private set; }
		
		public JabbRUser (string userName)
		{
			this.Id = userName;
			this.Name = userName;
		}
		
		public JabbRUser (global::JabbR.Client.Models.User user)
		{
			this.InnerUser = user;
			this.Id = user.Name;
			this.Name = user.Name;
			this.IsAfk = user.IsAfk;
			this.Active = user.Active;
		}
	}
}

