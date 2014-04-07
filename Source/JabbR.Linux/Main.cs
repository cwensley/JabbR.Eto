using System;
using Eto;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;
using JabbR.Desktop;

namespace JabbR.Linux
{
    class MainClass
    {
        
        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
 
        public static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = Validator;
            var generator = new Eto.Platform.GtkSharp.Generator();
            generator.Add <IJabbRApplication>(() => new JabbRApplicationHandler());
            
            var app = new JabbRApplication(generator);
            app.Run(args);
        }
    }
}
