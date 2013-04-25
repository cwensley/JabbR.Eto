using System;
using System.Threading.Tasks;
using Eto.Drawing;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using Eto;

namespace JabbR.Desktop.Model.JabbR
{
    public class JabbRUser : User
    {
        public global::JabbR.Client.Models.User InnerUser { get; private set; }

        public string Gravatar { get; set; }
        
        public JabbRUser(JabbRServer server, string userName)
            : base (server)
        {
            this.Id = userName;
            this.Name = userName;
        }
        
        public JabbRUser(JabbRServer server, global::JabbR.Client.Models.User user)
            : base (server)
        {
            this.InnerUser = user;
            this.Id = user.Name;
            this.Name = user.Name;
            this.IsAfk = user.IsAfk;
            this.Active = user.Active;
            this.Gravatar = user.Hash;
        }
    
        public JabbRUser(User user)
            : base (user.Server)
        {
            var jabbr = user as JabbRUser;
            if (jabbr != null)
                this.InnerUser = jabbr.InnerUser;
            this.Id = user.Id;
            this.Name = user.Name;
            this.IsAfk = user.IsAfk;
            this.Active = user.Active;
            this.Gravatar = jabbr.Gravatar;
        }

        public override async Task<Bitmap> GetIcon()
        {
            if (string.IsNullOrEmpty(Gravatar))
                return DefaultUserIcon;

            var url = string.Format("http://www.gravatar.com/avatar/{0}?s=16&d=404", Gravatar);
            //Debug.Print("Getting icon {0}", url);
            var request = HttpWebRequest.Create(url);
            var response = await Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            try
            {
                using (var stream = response.GetResponseStream ())
                {
                    if (Generator.Current.IsWpf)
                    {
                        using (var ms = new MemoryStream ())
                        {
                            stream.CopyTo(ms);
                            ms.Position = 0;
                            return new Bitmap(ms);
                        }
                    }
                    else
                    {
                        var bmp = new Bitmap(stream);
                        //Debug.Print ("Got Image for user '{0}', {1}", this.Name, url);
                        return bmp;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Error getting Icon for user '{0}', {1}, {2}", this.Name, url, ex);
                return DefaultUserIcon;
            }
        }
    }
}

