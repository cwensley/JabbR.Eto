using System;
using Eto.Platform.GtkSharp;
using System.Text;
using System.Security.Cryptography;

namespace JabbR.Eto.Gtk
{
    public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
    {
        const string Salt = "JabbR.Eto";
        static byte[] saltBytes = Encoding.UTF8.GetBytes(Salt);
        
        public string EncryptString(string serverName, string accountName, string password)
        {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(password ?? string.Empty), saltBytes, DataProtectionScope.CurrentUser));
            
        }

        public string DecryptString(string serverName, string accountName, string value)
        {
            try
            {
                if (!string.IsNullOrEmpty(value))
                    return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value), saltBytes, DataProtectionScope.CurrentUser));
            }
            catch
            {
            }
            return null;
        }

        public void SendNotification(string text)
        {
            
        }
    }
}

