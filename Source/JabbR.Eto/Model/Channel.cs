using System;
using Eto.Forms;
using JabbR.Eto.Interface;
using Eto;
using System.Xml;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace JabbR.Eto.Model
{
	public abstract class Channel : ISectionGenerator, IXmlReadable
	{
		public string Id { get; set; }
		
		public string Name { get; set; }
		
		public Server Server { get; private set; }
		
		public abstract IEnumerable<User> Users { get; }
		
		public abstract IEnumerable<string> Owners { get; }
		
		public abstract IEnumerable<ChannelMessage> GetHistory(string fromId);
		
		public event EventHandler<MessageEventArgs> MessageReceived;

		protected virtual void OnMessageReceived (MessageEventArgs e)
		{
			if (MessageReceived != null)
				MessageReceived (this, e);
		}
		
		public event EventHandler<MessageEventArgs> MeMessageReceived;

		protected virtual void OnMeMessageReceived (MessageEventArgs e)
		{
			if (MeMessageReceived != null)
				MeMessageReceived (this, e);
		}
		
		public event EventHandler<UserEventArgs> UserLeft;

		protected virtual void OnUserLeft (UserEventArgs e)
		{
			if (UserLeft != null)
				UserLeft (this, e);
		}
		
		public event EventHandler<UserEventArgs> UserJoined;

		protected virtual void OnUserJoined (UserEventArgs e)
		{
			if (UserJoined != null)
				UserJoined (this, e);
		}
		
		public event EventHandler<EventArgs> Closed;

		protected virtual void OnClosed (EventArgs e)
		{
			if (Closed != null)
				Closed (this, e);
		}
		
		public event EventHandler<UserEventArgs> OwnerAdded;

		protected virtual void OnOwnerAdded (UserEventArgs e)
		{
			if (OwnerAdded != null)
				OwnerAdded (this, e);
		}
		
		
		public event EventHandler<UserEventArgs> OwnerRemoved;

		protected virtual void OnOwnerRemoved (UserEventArgs e)
		{
			if (OwnerRemoved != null)
				OwnerRemoved (this, e);
		}
		
		
		public Channel (Server server)
		{
			this.Id = Guid.NewGuid ().ToString ();
			this.Server = server;
		}

		public abstract void SendMessage (string command);

		public virtual Control GenerateSection ()
		{
			return new ChannelSection(this);
		}
		
		public abstract Task<Channel> GetChannelInfo();

		#region IXmlReadable implementation
		
		public void ReadXml (XmlElement element)
		{
			this.Id = element.GetAttribute ("id") ?? Guid.NewGuid ().ToString ();
			this.Name = element.GetAttribute ("name");
		}

		public void WriteXml (XmlElement element)
		{
			element.SetAttribute ("id", this.Id);
			element.SetAttribute ("name", this.Name);
		}
		
		#endregion
	}
}

