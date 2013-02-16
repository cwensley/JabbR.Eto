using System;
using System.Collections.Generic;
using Eto;
using System.Collections.ObjectModel;


namespace JabbR.Eto.Model
{
	public enum BadgeDisplayMode
	{
		None,
		All,
		Highlighted
	}

	public class Configuration : IXmlReadable
	{
		List<Server> servers = new List<Server>();
		List<Highlight> highlights = new List<Highlight>();
		
		public IEnumerable<Server> Servers { get { return servers; } }
		
		public List<Highlight> Highlights { get { return highlights; } }

		public BadgeDisplayMode BadgeDisplay { get; set; }

		public bool UseMetroTheme { get; set; }
		
		public event EventHandler<ServerEventArgs> ServerAdded;

		protected virtual void OnServerAdded (ServerEventArgs e)
		{
			if (ServerAdded != null)
				ServerAdded (this, e);
		}
		
		public event EventHandler<ServerEventArgs> ServerRemoved;

		protected virtual void OnServerRemoved (ServerEventArgs e)
		{
			if (ServerRemoved != null)
				ServerRemoved (this, e);
		}
		
		
		public Configuration ()
		{
		}

		class DisconnectHelper
		{
			bool finishedCalled;

			public int DisconnectCount { get; set; }

			public Action Finished { get; set; }

			public void HookServer (Server server)
			{
				DisconnectCount++;
				if (Finished != null)
					server.Disconnected += Disconnected;
			}

			public void FinishDisconnect ()
			{
				lock (this)
				{
					if (DisconnectCount == 0 && Finished != null && !finishedCalled)
					{
						finishedCalled = true;
						Finished ();
					}
				}
			}

			public void Disconnected (object sender, EventArgs e)
			{
				var server = sender as Server;
				server.Disconnected -= Disconnected;
				DisconnectCount--;
				lock (this)
				{
					if (DisconnectCount == 0 && !finishedCalled)
					{
						finishedCalled = true;
						Finished ();
					}
				}
			}
		}
		
		public void DisconnectAll (Action finished = null)
		{
			var helper = new DisconnectHelper { Finished = finished };
			foreach (var server in Servers) {
				if (server.IsConnected)
				{
					helper.HookServer (server);
					server.Disconnect ();
				}
			}
			helper.FinishDisconnect ();
		}

		public void RemoveServer (Server server)
		{
			if (server.IsConnected)
				server.Disconnect ();
			servers.Remove (server);
			OnServerRemoved (new ServerEventArgs(server));
		}
		
		public void AddServer (Server server)
		{
			servers.Add (server);
			OnServerAdded (new ServerEventArgs(server));
		}
		
		#region IXmlReadable implementation
		
		public void ReadXml (System.Xml.XmlElement element)
		{
			element.ReadChildListXml(servers, Server.CreateFromXml, "server", "servers");
			element.ReadChildListXml(highlights, Highlight.CreateFromXml, "highlight", "highlights");
			this.BadgeDisplay = element.GetEnumAttribute<BadgeDisplayMode> ("badgeDisplay") ?? BadgeDisplayMode.Highlighted;
			this.UseMetroTheme = element.GetBoolAttribute ("useMetroTheme") ?? false;
		}

		public void WriteXml (System.Xml.XmlElement element)
		{
			element.WriteChildListXml(servers, "server", "servers");
			element.WriteChildListXml(highlights, "highlight", "highlights");
			if (this.BadgeDisplay != BadgeDisplayMode.Highlighted)
				element.SetAttribute ("badgeDisplay", this.BadgeDisplay);
			if (this.UseMetroTheme)
				element.SetAttribute ("useMetroTheme", this.UseMetroTheme);
		}
		
		#endregion
	}
}

