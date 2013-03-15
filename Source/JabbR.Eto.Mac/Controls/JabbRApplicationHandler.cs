using System;
using Eto.Platform.Mac.Forms;
using MonoMac.Security;
using System.Text;
using MonoMac.Foundation;
using MonoMac.AppKit;


namespace JabbR.Eto.Mac.Controls
{
	public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
	{
		public JabbRApplicationHandler ()
		{
		}

		public void SendNotification (string text)
		{
			var notify = NSNotificationCenter.DefaultCenter;
			notify.PostNotificationName(text, null);
			NSApplication.SharedApplication.RequestUserAttention(NSRequestUserAttentionType.InformationalRequest);
		}
		
		public string EncryptString (string serverName, string accountName, string password)
		{
			if (string.IsNullOrEmpty (password))
				return null;
			byte[] passwordBytes = Encoding.UTF8.GetBytes (password);
			var code = SecKeyChain.AddInternetPassword(serverName, accountName, passwordBytes);
			if (code == SecStatusCode.DuplicateItem) {
				var queryRec = new SecRecord (SecKind.InternetPassword) { Server = serverName, Account = accountName };
				queryRec = SecKeyChain.QueryAsRecord (queryRec, out code);
					
				if (code == SecStatusCode.Success ) {
					SecKeyChain.Remove (queryRec);
				}
				code = SecKeyChain.AddInternetPassword(serverName, accountName, passwordBytes);
			}
			return "keychain";
		}
		
		public string DecryptString (string serverName, string accountName, string value)
		{
			if (value == "keychain") {
				byte[] passwordBytes;
				var status = SecKeyChain.FindInternetPassword (serverName, accountName, out passwordBytes);
				if (status == SecStatusCode.Success) {
					return Encoding.UTF8.GetString (passwordBytes);
				}
			}
			return null;
		}
	}
}

