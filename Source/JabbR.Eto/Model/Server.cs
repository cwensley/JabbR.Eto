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

		List<Channel> channels = new List<Channel> ();

		public string Id { get; set; }
		
		public abstract string TypeId { get; }
		
		public string Name { get; set; }
		
		public bool ConnectOnStartup { get; set; }

		public User CurrentUser { get; protected set; }
		
		public IEnumerable<Channel> Channels {
			get { return channels; }
		}

		public event EventHandler<EventArgs> Disconnected;
		
		protected virtual void OnDisconnected (EventArgs e)
		{
			this.IsConnected = false;
			OnGlobalMessageReceived (new NotificationEventArgs (new NotificationMessage (DateTimeOffset.Now, "Disconnected")));
			if (Disconnected != null)
				Disconnected (this, e);
		}
		
		public event EventHandler<EventArgs> Connected;

		protected virtual void OnConnected (EventArgs e)
		{
			this.IsConnected = true;
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

		public event EventHandler<ChannelEventArgs> OpenChannel;
		
		protected virtual void OnOpenChannel (ChannelEventArgs e)
		{
			channels.Add (e.Channel);
			channels.Sort ((x,y) => x.Name.CompareTo (y.Name));
			
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
			this.channels.Clear();
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
		
		public abstract void Connect ();
		
		public abstract void Disconnect ();

		public abstract void SendMessage (string command);

		public abstract void JoinChannel (string name);
		
		public virtual void GenerateEditControls (DynamicLayout layout)
		{
		}

		public virtual Control GenerateSection ()
		{
			return null;
		}
		
		public abstract Task<IEnumerable<ChannelInfo>> GetChannelList ();

		string IListItem.Text { get { return this.Name; } }
		
		string IListItem.Key { get { return this.Id; } }
		
		Image IImageListItem.Image { get { return null; } }
		
		bool ITreeItem<ITreeItem>.Expanded { get; set; }
		
		bool ITreeItem<ITreeItem>.Expandable { get { return true; } }
		
		int IDataStore<ITreeItem>.Count { get { return channels.Count; } }
		
		ITreeItem IDataStore<ITreeItem>.this[int index] { get { return channels[index]; } }
		
		ITreeItem ITreeItem<ITreeItem>.Parent { get; set; }
	}
}

