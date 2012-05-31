using System;
using Eto.Forms;
using System.Collections.Generic;
using JabbR.Client.Models;
using System.Linq;

namespace JabbR.Eto.Controls
{
	public class UserList : Panel
	{
		TreeView tree;
		TreeItem owners;
		TreeItem online;
		TreeItem away;
		TreeItemCollection items;
		HashSet<string> ownerLookup;
		
		public ConnectionInfo Info { get; private set; }
		
		public string RoomName { get; private set; }
				
		public UserList (ConnectionInfo info, string roomName)
		{
			this.Info = info;
			this.RoomName = roomName;
			tree = new TreeView ();
			tree.Style = "userList";
			
			items = new TreeItemCollection ();
			items.Add (owners = new TreeItem { Text = "Room Owners", Expanded = true });
			items.Add (online = new TreeItem { Text = "Online", Expanded = true });
			items.Add (away = new TreeItem { Text = "Away", Expanded = true });
			tree.DataStore = items;
			
			this.AddDockedControl (tree);
		}
		
		TreeItem CreateItem (User user)
		{
			return new TreeItem { Text = user.Name, Key = user.Name };
		}

		public void OwnerAdded (User user)
		{
			if (!ownerLookup.Contains (user.Name))
				ownerLookup.Add (user.Name);
			RemoveUser (user);
			AddUser (user);
			Update ();
		}
		
		public void OwnerRemoved (User user)
		{
			if (ownerLookup.Contains (user.Name))
				ownerLookup.Remove (user.Name);
			RemoveUser (user);
			AddUser (user);
			Update ();
		}

		public void UserJoined (User user)
		{
			RemoveUser (user);
			AddUser (user);
			Update ();
		}
		
		void AddUser (User user)
		{
			var isOwner = ownerLookup.Contains (user.Name);
			TreeItem item = isOwner ? owners : user.IsAfk ? away : online;
			item.Children.Add (CreateItem (user));
		}
		
		void RemoveUser (User user)
		{
			RemoveItem (items, user.Name);
		}

		public void UserActivityChanged (User user)
		{
			if (user.Active) {
				var item = away.Children.FirstOrDefault(r => r.Key == user.Name);
				if (item != null) {
					away.Children.Remove (item);
					online.Children.Add (item);
				}
			}
			Update ();
		}
		
		public void MakeUsersInactive (IEnumerable<User> users)
		{
			var lookup = online.Children.ToDictionary(r => r.Key);
			foreach (var user in users) {
				ITreeItem item;
				if (lookup.TryGetValue (user.Name, out item)) {
					online.Children.Remove (item);
					away.Children.Add (item);
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
			tree.DataStore = items;
		}
	
		public void SetUsers (IEnumerable<User> users, IEnumerable<string> owners)
		{
			ownerLookup = new HashSet<string>(owners);
			
			foreach (var user in users)
				AddUser (user);
			Update ();
		}
	}
}

