using System;
using JabbR.Eto.Interface;
using Eto;
using System.Xml;
using System.Threading.Tasks;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;


namespace JabbR.Eto.Model
{
	public abstract class Channel : ISectionGenerator, IXmlReadable, ITreeItem, IComparable
	{
		public string Id { get; set; }
		
		public string Name { get; set; }
		
		public string Topic { get; set; }
		
		public bool Private { get; set; }
		
		public int UnreadCount { get; protected set; }
		
		public Server Server { get; private set; }
		
		public abstract IEnumerable<User> Users { get; }
		
		public abstract IEnumerable<string> Owners { get; }
		
		public abstract Task<IEnumerable<ChannelMessage>> GetHistory(string fromId);
		
		public event EventHandler<MessageEventArgs> MessageReceived;

		protected virtual void OnMessageReceived (MessageEventArgs e)
		{
			if (MessageReceived != null)
				MessageReceived (this, e);
		}
		
		public event EventHandler<MeMessageEventArgs> MeMessageReceived;

		protected virtual void OnMeMessageReceived (MeMessageEventArgs e)
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
		
		public event EventHandler<UsersEventArgs> UsersInactive;

		protected virtual void OnUsersInactive (UsersEventArgs e)
		{
			if (UsersInactive != null)
				UsersInactive (this, e);
		}
		
		public event EventHandler<UsersEventArgs> UsersActivityChanged;
		
		protected virtual void OnUsersActivityChanged (UsersEventArgs e)
		{
			if (UsersActivityChanged != null)
				UsersActivityChanged (this, e);
		}
		
		public event EventHandler<MessageContentEventArgs> MessageContent;
		
		protected virtual void OnMessageContent (MessageContentEventArgs e)
		{
			if (MessageContent != null)
				MessageContent (this, e);
		}
		
		public event EventHandler<EventArgs> TopicChanged;
		
		protected virtual void OnTopicChanged (EventArgs e)
		{
			if (TopicChanged != null)
				TopicChanged (this, e);
		}
		
		
		public Channel (Server server)
		{
			this.Id = Guid.NewGuid ().ToString ();
			this.Server = server;
		}

		public abstract void SendMessage (string command);

		public virtual Control GenerateSection ()
		{
			var section = new ChannelSection(this);
			section.Initialize ();
			return section;
		}
		
		public abstract Task<Channel> GetChannelInfo();
		
		public abstract void UserTyping();
		
		public virtual void ResetUnreadCount ()
		{
			UnreadCount = 0;
		}

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
		
		string IListItem.Text {
			get {
				if (UnreadCount > 0)
					return string.Format ("{0} ({1})", this.Name, UnreadCount);
				else
					return this.Name;
			}
		}
		
		public virtual int CompareTo (object obj)
		{
			var alt = obj as Channel;
			if (alt == null) return -1;
			else return this.Name.CompareTo (alt.Name);
		}

		public virtual Image Image { get { return null; } }
		
		string IListItem.Key { get { return this.Server.Id + "_" + this.Id; } }
		
		bool ITreeItem<ITreeItem>.Expanded { get; set; }

		bool ITreeItem<ITreeItem>.Expandable { get { return false; } }
		
		int IDataStore<ITreeItem>.Count { get { return 0; } }
		
		ITreeItem IDataStore<ITreeItem>.this[int index] { get { return null; } }
		
		ITreeItem ITreeItem<ITreeItem>.Parent {
			get { return Server; }
			set { }
		}
	}
}

