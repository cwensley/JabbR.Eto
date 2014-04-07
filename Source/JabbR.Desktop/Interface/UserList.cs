using System;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using JabbR.Desktop.Model;
using Eto;
using Eto.Drawing;

namespace JabbR.Desktop.Interface
{
    public class UserList : Panel
    {
        TreeView tree;
        TreeItem owners;
        TreeItem online;
        TreeItem away;
        TreeItemCollection items;
        
        public Channel Channel { get; private set; }
                
        public UserList(Channel channel)
        {
            Channel = channel;
            Size = new Size(150, 100);
            tree = new TreeView();
            tree.Style = "userList";
            tree.Activated += HandleActivated;

            items = new TreeItemCollection();
            items.Add(owners = new TreeItem { Text = "Room Owners", Expanded = true });
            items.Add(online = new TreeItem { Text = "Online", Expanded = true });
            items.Add(away = new TreeItem { Text = "Away", Expanded = true });
            if (Generator.IsMac)
            {
                foreach (var item in items.OfType<TreeItem>())
                {
                    item.Text = item.Text.ToUpperInvariant();
                }
            }
            tree.DataStore = items;
            
            Content = tree;
        }

        void HandleActivated(object sender, TreeViewItemEventArgs e)
        {
            var item = e.Item as TreeItem;
            if (item != null)
            {
                var user = item.Tag as User;
                if (user != null)
                {
                    Channel.Server.StartChat(user);
                }
            }
        }
        
        TreeItem CreateItem(User user)
        {
            return new TreeItem { Text = user.Name, Key = user.Id, Tag = user, Image = Channel.Server.GetUserIcon(user) };
        }

        public void OwnerAdded(User user)
        {
            if (RemoveUser(user))
            {
                AddUser(user);
                Update();
            }
        }
        
        public void OwnerRemoved(User user)
        {
            if (RemoveUser(user))
            {
                AddUser(user);
                Update();
            }
        }

        public void UserJoined(User user)
        {
            RemoveUser(user);
            AddUser(user);
            Update();
        }
        
        void AddUser(User user)
        {
            var isOwner = Channel.Owners.Contains(user.Id);
            TreeItem item = isOwner ? owners : user.Active ? online : away;
            item.Children.Add(CreateItem(user));
        }

        bool RemoveUser(User user)
        {
            return RemoveUser(user.Id);
        }
        
        bool RemoveUser(string userId)
        {
            return RemoveItem(items, userId);
        }

        public void UsersActivityChanged(IEnumerable<User> users)
        {
            bool changed = false;
            foreach (var user in users)
            {
                if (RemoveUser(user))
                {
                    AddUser(user);
                    changed = true;
                }
            }
            if (changed)
                Update();
        }
            
        bool RemoveItem(TreeItemCollection items, string key)
        {
            foreach (var item in items.OfType<TreeItem>())
            {
                if (item.Key == key)
                {
                    items.Remove(item);
                    return true;
                }
                if (RemoveItem(item.Children, key))
                    return true;
            }
            return false;
        }

        ITreeItem FindUserItem(User user)
        {
            var item = owners.Children.FirstOrDefault(r => r.Key == user.Id);
            if (item != null)
                return item;
            item = online.Children.FirstOrDefault(r => r.Key == user.Id);
            if (item != null)
                return item;
            item = away.Children.FirstOrDefault(r => r.Key == user.Id);
            if (item != null)
                return item;
            return null;
        }

        public void UserIconChanged(User user, Image image)
        {
            var item = FindUserItem(user) as TreeItem;
            if (item != null)
            {
                item.Image = image;
                //tree.RefreshData ();
                tree.RefreshItem(item);
            }
        }

        public void UserLeft(User user)
        {
            RemoveUser(user);
            Update();
        }
        
        void Update()
        {
            owners.Children.Sort((x, y) => string.Compare(x.Text, y.Text, StringComparison.CurrentCulture));
            online.Children.Sort((x, y) => string.Compare(x.Text, y.Text, StringComparison.CurrentCulture));
            away.Children.Sort((x, y) => string.Compare(x.Text, y.Text, StringComparison.CurrentCulture));
            tree.RefreshData();
        }
    
        public void SetUsers(IEnumerable<User> users)
        {
            owners.Children.Clear();
            online.Children.Clear();
            away.Children.Clear();
            foreach (var user in users)
                AddUser(user);
            Update();
        }

        public void UsernameChanged(string oldUserId, User user)
        {
            if (RemoveUser(oldUserId))
            {
                AddUser(user);
            }
            Update();
        }
    }
}

