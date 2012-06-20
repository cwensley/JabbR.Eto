using System;
using Eto;
using System.Xml;
using Eto.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JabbR.Eto.Model.JabbR;
using JabbR.Eto.Interface;

namespace JabbR.Eto.Model
{
	public abstract class Server : IXmlReadable, ISectionGenerator
	{
		public static Server CreateFromXml (XmlElement element)
		{
			var type = element.GetStringAttribute ("type") ?? JabbRServer.JabbRTypeId;
			switch (type) {
			case JabbRServer.JabbRTypeId:
				return new JabbRServer ();
			default:
				return null;
			}
		}
		
		public bool IsConnected { get; protected set; }

		List<Channel> channels = new List<Channel> ();

		public string Id { get; set; }
		
		public abstract string TypeId { get; }
		
		public string Name { get; set; }
		
		public bool ConnectOnStartup { get; set; }

		public User CurrentUser { get; protected set; }
		
		public List<Channel> Channels {
			get { return channels; }
		}

		public event EventHandler<EventArgs> Disconnected;
		
		protected virtual void OnDisconnected (EventArgs e)
		{
			this.IsConnected = false;
			OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage(DateTimeOffset.Now, "Disconnected")));
			if (Disconnected != null)
				Disconnected (this, e);
		}
		
		public event EventHandler<EventArgs> Connected;

		protected virtual void OnConnected (EventArgs e)
		{
			this.IsConnected = true;
			OnGlobalMessageReceived(new NotificationEventArgs(new NotificationMessage(DateTimeOffset.Now, "Connected")));
			if (Connected != null)
				Connected (this, e);
		}
		
		public event EventHandler<UserChannelEventArgs> UserJoined;

		protected virtual void OnUserJoined (UserChannelEventArgs e)
		{
			if (UserJoined != null)
				UserJoined (this, e);
		}
		
		public event EventHandler<UserChannelEventArgs> UserLeft;

		protected virtual void OnUserLeft (UserChannelEventArgs e)
		{
			if (UserLeft != null)
				UserLeft (this, e);
		}
		
		public event EventHandler<ChannelEventArgs> CloseChannel;

		protected virtual void OnCloseChannel (ChannelEventArgs e)
		{
			if (CloseChannel != null)
				CloseChannel (this, e);
		}
		
		public event EventHandler<NotificationEventArgs> GlobalMessageReceived;

		protected virtual void OnGlobalMessageReceived (NotificationEventArgs e)
		{
			if (GlobalMessageReceived != null)
				GlobalMessageReceived (this, e);
		}
		
		public Server ()
		{
			this.Id = Guid.NewGuid ().ToString ();
		}
		
		public virtual void ReadXml (XmlElement element)
		{
			this.Id = element.GetStringAttribute ("id") ?? Guid.NewGuid ().ToString ();
			this.Name = element.GetAttribute ("name");
			this.ConnectOnStartup = element.GetBoolAttribute ("connectOnStartup") ?? true;
		}

		public virtual void WriteXml (XmlElement element)
		{
			element.SetAttribute ("id", this.Id);
			element.SetAttribute ("name", this.Name);
			element.SetAttribute ("connectOnStartup", this.ConnectOnStartup);
		}
		
		public abstract void Connect ();
		
		public abstract void Disconnect ();

		public abstract void SendMessage (string command);
		
		public virtual void GenerateEditControls (DynamicLayout layout)
		{
		}

		public virtual Control GenerateSection ()
		{
			return null;
		}

	}
}

