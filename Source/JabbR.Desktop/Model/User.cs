using System;
using System.Threading.Tasks;
using Eto.Drawing;

namespace JabbR.Desktop.Model
{
    public abstract class User
    {
        public static Bitmap DefaultUserIcon = Bitmap.FromResource("JabbR.Desktop.Resources.user.png");

        public Server Server  { get; private set; }

        public User(Server server)
        {
            this.Server = Server;
        }

        public string Id { get; set; }
        
        public string Name { get; set; }

        public bool IsAfk { get; set; }
        
        public bool Active { get; set; }
        
        public bool Owner { get; set; }

        public abstract Task<Bitmap> GetIcon();
    }

}

