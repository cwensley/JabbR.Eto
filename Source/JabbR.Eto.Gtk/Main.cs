using System;
using Eto;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;

namespace JabbR.Eto.Gtk
{
	class MainClass
	{
		
		public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
 
		public static void Main (string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback = Validator;
			var generator = Generator.Detect;
			generator.Add <IJabbRApplication> (() => new JabbRApplicationHandler ());
			
			var app = new JabbRApplication();
			app.Run (args);
		}
	}
}
