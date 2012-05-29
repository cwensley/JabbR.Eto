using System;
using JabbR.Client;
using JabbR.Client.Models;


namespace JabbR.Eto
{
	public class ConnectionInfo
	{
		public JabbRClient Client { get; set; }
		public LogOnInfo LogOnInfo { get; set; }
		public User CurrentUser { get; set; }
		
		public ConnectionInfo ()
		{
		}
	}
}

