using System;
using Eto;
using System.Xml;
using Eto.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JabbR.Eto.Model.JabbR;
using JabbR.Eto.Interface;
using Eto.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR.Eto.Model
{
	public abstract class Server : IXmlReadable, ISectionGenerator, ITreeItem
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
		public bool IsConnecting { get; protected set; }
		
		List<Channel> channels = new List<Channel> ();

		public string Id { get; set; }
		
		public abstract string TypeId { get; }
		
		public string Name { get; set; }
		
		public bool ConnectOnStartup { get; set; }

		public User CurrentUser { get; protected set; }
		
		public abstract bool IsAuthenticated { get; }
		
		public IEnumerable<Channel> Channels {
			get { return channels; }
		}
		
		public event EventHandler<ConnectionErrorEventArgs> ConnectError;
		
		protected virtual void OnConnectError (ConnectionErrorEventArgs e)
		{
			OnGlobalMessageReceived (new NotificationEventArgs(new NotificationMessage ("Could not connect to server {0}. {1}", this.Name, e.Exception.GetBaseException ().Message)));
			if (ConnectError != null)
				ConnectError (this, e);
		}

		public event EventHandler<EventArgs> Disconnecting;

		protected virtual void OnDisconnecting (EventArgs e)
		{
			if (Disconnecting != null)
				Disconnecting (this, e);
		}
			
		public event EventHandler<EventArgs> Disconnected;
		
		protected virtual void OnDisconnected (EventArgs e)
		{
			OnDisconnecting (e);
			channels.Clear ();
			this.IsConnected = false;
			this.IsConnecting = false;
			OnGlobalMessageReceived (new NotificationEventArgs (new NotificationMessage (DateTimeOffset.Now, "Disconnected")));
			if (Disconnected != null)
				Disconnected (this, e);
		}

		public event EventHandler<EventArgs> Connecting;
		
		protected virtual void OnConnecting (EventArgs e)
		{
			this.IsConnecting = true;
			OnGlobalMessageReceived (new NotificationEventArgs (new NotificationMessage (DateTimeOffset.Now, "Connecting...")));
			if (Connecting != null)
				Connecting (this, e);
		}
		
		public event EventHandler<EventArgs> Connected;

		protected virtual void OnConnected (EventArgs e)
		{
			this.IsConnected = true;
			this.IsConnecting = false;
			OnGlobalMessageReceived (new NotificationEventArgs (new NotificationMessage (DateTimeOffset.Now, "Connected")));
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
			this.channels.Remove (e.Channel);
			if (CloseChannel != null)
				CloseChannel (this, e);
		}
		
		public event EventHandler<NotificationEventArgs> GlobalMessageReceived;

		protected virtual void OnGlobalMessageReceived (NotificationEventArgs e)
		{
			if (GlobalMessageReceived != null)
				GlobalMessageReceived (this, e);
		}

		public event EventHandler<OpenChannelEventArgs> OpenChannel;
		
		protected virtual void OnOpenChannel (OpenChannelEventArgs e)
		{
			if (!channels.Contains (e.Channel)) {
				channels.Add (e.Channel);
				channels.Sort ((x,y) => x.CompareTo (y));
			}
			
			if (OpenChannel != null)
				OpenChannel (this, e);
		}
		
		public event EventHandler<ChannelEventArgs> ChannelInfoChanged;
		
		public virtual void OnChannelInfoChanged (ChannelEventArgs e)
		{
			if (ChannelInfoChanged != null)
				ChannelInfoChanged (this, e);
		}
		
		protected void InitializeChannels (IEnumerable<Channel> channels)
		{
			this.channels.Clear ();
			this.channels.AddRange (channels.OrderBy (r => r.Name));
		}
		
		public Server ()
		{
			this.Id = Guid.NewGuid ().ToString ();
			((ITreeItem)this).Expanded = true;
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
		
		public abstract bool Authenticate (Control parent);

		public abstract bool CheckAuthentication (Control parent, bool allowCancel, bool isEditing);
		
		public abstract void Connect ();
		
		public abstract void Disconnect ();

		public abstract void SendMessage (string command);

		public abstract void JoinChannel (string name);

		public abstract void LeaveChannel (Channel channel);
		
		public virtual void GenerateEditControls (DynamicLayout layout, bool isNew)
		{
		}

		public virtual bool PreSaveSettings (Control parent)
		{
			return true;
		}

		public virtual Control GenerateSection ()
		{
			return null;
		}
		
		public abstract void StartChat (User user);
		
		public abstract Task<IEnumerable<ChannelInfo>> GetChannelList ();
		
		public abstract Task<IEnumerable<ChannelInfo>> GetCachedChannels ();
		
		string IListItem.Text { get { return this.Name; } }
		
		string IListItem.Key { get { return this.Id; } }
		
		Image IImageListItem.Image { get { return null; } }
		
		bool ITreeItem<ITreeItem>.Expanded { get; set; }
		
		bool ITreeItem<ITreeItem>.Expandable { get { return true; } }
		
		int IDataStore<ITreeItem>.Count { get { return channels.Count; } }
		
		ITreeItem IDataStore<ITreeItem>.this [int index] { get { return channels [index]; }
		}
		
		ITreeItem ITreeItem<ITreeItem>.Parent { get; set; }
	}
}

