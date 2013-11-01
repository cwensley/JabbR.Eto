using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Platform.Wpf.Forms;
using System.Security.Cryptography;
using JabbR.Desktop;

namespace JabbR.Windows.Controls
{
	public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
	{
		const string Salt = "JabbR";
		static byte[] saltBytes = Encoding.UTF8.GetBytes (Salt);
		
		public string EncryptString (string serverName, string accountName, string password)
		{
			return Convert.ToBase64String (ProtectedData.Protect (Encoding.UTF8.GetBytes (password ?? string.Empty), saltBytes, DataProtectionScope.CurrentUser));
			
		}

		public string DecryptString (string serverName, string accountName, string value)
		{
			try {
				if (!string.IsNullOrEmpty (value))
					return Encoding.UTF8.GetString (ProtectedData.Unprotect (Convert.FromBase64String (value), saltBytes, DataProtectionScope.CurrentUser));
			} catch {
			}
			return null;
		}


		public void SendNotification (string text)
		{
			
		}
	}
}
