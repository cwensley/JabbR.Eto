using System;
using System.Collections.Generic;
using Eto;
using System.Collections.ObjectModel;


namespace JabbR.Eto.Model
{
	public class Configuration : IXmlReadable
	{
		List<Server> servers = new List<Server>();
		
		public IEnumerable<Server> Servers { get { return servers; } }
		
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
		
		public void DisconnectAll ()
		{
			foreach (var server in Servers) {
				server.Disconnect ();
			}
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
		}

		public void WriteXml (System.Xml.XmlElement element)
		{
			element.WriteChildListXml(servers, "server", "servers");
		}
		
		#endregion
	}
}

