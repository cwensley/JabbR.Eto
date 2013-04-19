using System;
using Eto.Platform.Mac.Forms;
using MonoMac.Security;
using System.Text;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Runtime.InteropServices;
using MonoMac.ObjCRuntime;
using System.Diagnostics;
using MonoMac.CoreFoundation;

namespace JabbR.Eto.Mac.Controls
{
    public class JabbRApplicationHandler : ApplicationHandler, IJabbRApplication
    {
        public JabbRApplicationHandler()
        {
        }

        public void SendNotification(string text)
        {
            var notify = NSNotificationCenter.DefaultCenter;
            notify.PostNotificationName(text, null);
            NSApplication.SharedApplication.RequestUserAttention(NSRequestUserAttentionType.InformationalRequest);
        }

        // remove this code when Xamarin.Mac supports it

        const string securityLibraryName = "/System/Library/Frameworks/Security.framework/Security";
        static IntPtr securityLibrary;
        static IntPtr kSecMatchLimit;
        static IntPtr kSecClassKey;
        static IntPtr kSecClassGenericPassword;
        static IntPtr kSecAttrServer;
        static IntPtr kSecAttrAccount;

        static JabbRApplicationHandler()
        {
            securityLibrary = Dlfcn.dlopen(securityLibraryName, 0);
            kSecClassKey = Dlfcn.GetIntPtr(securityLibrary, "kSecClass");
            kSecMatchLimit = Dlfcn.GetIntPtr(securityLibrary, "kSecMatchLimit");
            kSecClassGenericPassword = Dlfcn.GetIntPtr(securityLibrary, "kSecClassGenericPassword");
            kSecAttrServer = Dlfcn.GetIntPtr(securityLibrary, "kSecAttrServer");
            kSecAttrAccount = Dlfcn.GetIntPtr(securityLibrary, "kSecAttrAccount");
        }

        [DllImport (securityLibraryName)]
        internal static extern SecStatusCode SecItemDelete(IntPtr cfDictRef);

        [DllImport (securityLibraryName)]
        internal static extern SecStatusCode SecItemCopyMatching(IntPtr cfDictRef, out IntPtr result);

        string GetServerName(string serverName)
        {
            return string.Format("JabbReto: {0}", serverName);
        }

        string GetAccountName(string accountName)
        {
            return string.Format("JabbReto-{0}", accountName);
        }

        public string EncryptString(string serverName, string accountName, string password)
        {
            serverName = GetServerName(serverName);
            accountName = GetAccountName(accountName);
            /**/
            var dict = new NSMutableDictionary();
            dict.Add(new NSNumber(kSecMatchLimit), new NSObject(SecMatchLimit.MatchLimitAll));
            dict.Add(new NSObject(kSecClassKey), new NSObject(kSecClassGenericPassword));
            //dict.Add(new NSObject(SecAttrServer), new NSString(serverName));
            dict.Add(new NSObject(kSecAttrAccount), new NSString(accountName));

            var code = SecItemDelete(dict.Handle);
            //Debug.Print("Delete code: {0}", code);

            if (string.IsNullOrEmpty(password))
                return null;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            SecKeyChain.AddGenericPassword(serverName, accountName, passwordBytes);
            /**

            //SecStatusCode code;
            var query = new SecRecord (SecKind.InternetPassword) { Server = serverName, Account = accountName };
            do {
                var queryRec = SecKeyChain.QueryAsRecord (query, out code);

                if (code != SecStatusCode.Success )
                    break;
                SecKeyChain.Remove (queryRec);
            } while (true);

            if (string.IsNullOrEmpty (password))
                return null;
            byte[] passwordBytes = Encoding.UTF8.GetBytes (password);
            code = SecKeyChain.AddInternetPassword(serverName, accountName, passwordBytes);
            /*if (code != SecStatusCode.Success) {
                throw new Exception("Could not save password to keychain");
            }*/

            return "keychain";
        }
        
        public string DecryptString(string serverName, string accountName, string value)
        {
            if (value == "keychain")
            {
                serverName = GetServerName(serverName);
                accountName = GetAccountName(accountName);
                byte[] passwordBytes;
                /**/
                var status = SecKeyChain.FindGenericPassword(serverName, accountName, out passwordBytes);
                /**
                var status = SecKeyChain.FindInternetPassword (serverName, accountName, out passwordBytes);
                /**/
                if (status == SecStatusCode.Success)
                {
                    return Encoding.UTF8.GetString(passwordBytes);
                }
            }
            return null;
        }
    }
}

