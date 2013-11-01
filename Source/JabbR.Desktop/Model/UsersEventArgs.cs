using System;
using Eto.Forms;
using JabbR.Desktop.Interface;
using System.Collections.Generic;

namespace JabbR.Desktop.Model
{
    public class UsersEventArgs : EventArgs
    {
        public DateTimeOffset When { get; private set; }

        public IEnumerable<User> Users { get; private set; }
        
        public UsersEventArgs(IEnumerable<User> users, DateTimeOffset when)
        {
            this.Users = users;
            this.When = when;
        }
    }
}

