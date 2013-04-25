using System;
using Eto.Forms;
using JabbR.Desktop.Interface;

namespace JabbR.Desktop.Model
{
    public class UserEventArgs : EventArgs
    {
        public DateTimeOffset When { get; private set; }

        public User User { get; private set; }
        
        public UserEventArgs(User user, DateTimeOffset when)
        {
            this.User = user;
            this.When = when;
        }
    }
}

