using System;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using JabbR.Eto.Model;
using Eto;

namespace JabbR.Eto.Interface
{
	public class UserList : Panel
	{
		TreeView tree;
		TreeItem owners;
		TreeItem online;
		TreeItem away;
		TreeItemCollection items;
		
		public Channel Channel { get; private set; }
				
		public UserList (Channel channel)
		{
			this.Channel = channel;
			tree = new TreeView ();
			tree.Style = "userList";
			
			items = new TreeItemCollection ();
			items.Add (owners = new TreeItem { Text = "Room Owners", Expanded = true });
			items.Add (online = new TreeItem { Text = "Online", Expanded = true });
			items.Add (away = new TreeItem { Text = "Away", Expanded = true });
			if (Generator.ID == Generators.Mac) {
				foreach (var item in items.OfType<TreeItem>()) {
					item.Text = item.Text.ToUpperInvariant();
				}
			}
			tree.DataStore = items;
			
			this.AddDockedControl (tree);
		}
		
		TreeItem CreateItem (User user)
		{
			return new TreeItem { Text = user.Name, Key = user.Name };
		}

		public void OwnerAdded (User user)
		{
			if (RemoveUser (user)) {
				AddUser (user);
				Update ();
			}
		}
		
		public void OwnerRemoved (User user)
		{
			if (RemoveUser (user)) {
				AddUser (user);
				Update ();
			}
		}

		public void UserJoined (User user)
		{
			RemoveUser (user);
			AddUser (user);
			Update ();
		}
		
		void AddUser (User user)
		{
			var isOwner = Channel.Owners.Contains (user.Name);
			TreeItem item = isOwner ? owners : user.Active ? online : away;
			item.Children.Add (CreateItem (user));
		}
		
		bool RemoveUser (User user)
		{
			return RemoveItem (items, user.Name);
		}

		public void UsersActivityChanged (IEnumerable<User> users)
		{
			var onlineLookup = online.Children.ToDictionary (r => r.Key);
			var awayLookup = away.Children.ToDictionary (r => r.Key);
			foreach (var user in users) {
				ITreeItem item;
				if (user.Active) {
					if (awayLookup.TryGetValue (user.Id, out item)) {
						away.Children.Remove (item);
						online.Children.Add (item);
					}
				} else {
					if (onlineLookup.TryGetValue (user.Id, out item)) {
						online.Children.Remove (item);
						away.Children.Add (item);
					}
				}
			}
			Update ();
		}
			
		bool RemoveItem (TreeItemCollection items, string key)
		{
			foreach (var item in items.OfType<TreeItem>()) {
				if (item.Key == key) {
					items.Remove (item);
					return true;
				}
				if (RemoveItem (item.Children, key))
					return true;
			}
			return false;
		}

		public void UserLeft (User user)
		{
			RemoveUser (user);
			Update ();
		}
		
		void Update ()
		{
			owners.Children.Sort ((x, y) => x.Text.CompareTo (y.Text));
			online.Children.Sort ((x, y) => x.Text.CompareTo (y.Text));
			away.Children.Sort ((x, y) => x.Text.CompareTo (y.Text));
			tree.Invalidate ();
		}
	
		public void SetUsers (IEnumerable<User> users)
		{
			foreach (var user in users)
				AddUser (user);
			Update ();
		}
	}
}

