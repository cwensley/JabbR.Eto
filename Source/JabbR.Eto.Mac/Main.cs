using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Eto;
using Eto.Platform.Mac.Forms.Controls;
using Eto.Platform.Mac.Forms;
using System.Diagnostics;
using MonoMac.Security;
using MonoMac.WebKit;
using JabbR.Eto.Mac.Controls;

namespace JabbR.Eto.Mac
{
	class MainClass
	{
		static void Main (string[] args)
		{
#if DEBUG
			Debug.Listeners.Add (new ConsoleTraceListener());
#endif
			var generator = Generator.Detect;
			generator.Add <IJabbRApplication> (() => new JabbRApplicationHandler ());

            NSApplication.CheckForIllegalCrossThreadCalls = false;

			Style.Add<TreeViewHandler>("channelList", h => {
				h.Control.Delegate = new CustomTreeViewDelegate { Handler = h, AllowGroupSelection = true };
				h.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
				h.Scroll.BorderType = NSBorderType.NoBorder;
			});
			
			Style.Add<FormHandler> ("mainForm", h => {
				h.Control.CollectionBehavior |= NSWindowCollectionBehavior.FullScreenPrimary;
			});
			
			Style.Add<ApplicationHandler> ("application", h => {
				h.EnableFullScreen ();
			});
			
			Style.Add<TreeViewHandler> ("userList", h => {
				h.Control.Delegate = new CustomTreeViewDelegate { Handler = h };
				h.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
				h.Scroll.BorderType = NSBorderType.NoBorder;
				
			});
			
			Style.Add<WebViewHandler> (null, h => {
				h.Control.Preferences.UsesPageCache = false;
			});
			
			var app = new JabbRApplication ();
			app.Initialized += delegate {
#if DEBUG
				NSUserDefaults.StandardUserDefaults.SetBool (true, "WebKitDeveloperExtras");
				NSUserDefaults.StandardUserDefaults.Synchronize();
#endif
			};
			app.Run (args);
		}
	}
}	

