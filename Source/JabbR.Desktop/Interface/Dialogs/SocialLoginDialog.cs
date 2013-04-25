using System;
using Eto.Forms;

namespace JabbR.Eto.Interface.Dialogs
{
	public class SocialLoginDialog : Dialog
	{
		WebView web;
		
		public SocialLoginDialog ()
		{
			web = new WebView();
			web.DocumentLoaded += HandleDocumentLoaded;
			
			this.AddDockedControl(web);
		}

		void HandleDocumentLoaded (object sender, WebViewLoadedEventArgs e)
		{
			
		}
	}
}

