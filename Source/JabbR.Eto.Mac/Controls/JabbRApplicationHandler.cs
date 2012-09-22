using System;
using Eto.Platform.Mac.Forms;
using MonoMac.Security;
using System.Text;


namespace JabbR.Eto.Mac.Controls
{
	public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
	{
		public JabbRApplicationHandler ()
		{
		}

		public string BadgeLabel {
			get { return Control.DockTile.BadgeLabel; }
			set { Control.DockTile.BadgeLabel = value ?? string.Empty; }
		}
		
		public string EncryptString (string serverName, string accountName, string password)
		{
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

