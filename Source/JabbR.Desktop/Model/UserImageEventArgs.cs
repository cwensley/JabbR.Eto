using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JabbR.Desktop.Model
{
    public class UserImageEventArgs : UserEventArgs
    {
        public Image Image { get; private set; }

        public UserImageEventArgs(User user, DateTimeOffset when, Image image)
            : base (user, when)
        {
            this.Image = image;
        }
    }
}
