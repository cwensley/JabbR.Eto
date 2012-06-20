using System;
using JabbR.Eto.Model;


namespace JabbR.Eto.Interface
{
	public class ServerSection : MessageSection
	{
		public Server Server { get; private set; }
		
		public ServerSection (Server server)
		{
			this.Server = server;
			this.Server.GlobalMessageReceived += HandleGlobalMessageReceived;
		}

		void HandleGlobalMessageReceived (object sender, NotificationEventArgs e)
		{
			AddNotification (e.Message);
		}
		
		public override void ProcessCommand (string command)
		{
			if (string.IsNullOrWhiteSpace (command))
				return;
			
			Server.SendMessage(command);
		}
		
	}
}

