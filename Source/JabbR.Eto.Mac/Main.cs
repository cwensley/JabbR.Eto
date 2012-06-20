using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Eto;
using Eto.Platform.Mac.Forms.Controls;
using Eto.Platform.Mac.Forms;
using System.Diagnostics;

namespace JabbR.Eto.Mac
{
	class MainClass
	{
		
		static void Main (string[] args)
		{
#if DEBUG
			Debug.Listeners.Add (new ConsoleTraceListener());
#endif
			
			Style.AddHandler<TreeViewHandler>("mainList", h => {
				h.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
				h.Scroll.BorderType = NSBorderType.NoBorder;
			});
			
			Style.AddHandler<FormHandler> ("mainForm", h => {
				h.Control.CollectionBehavior |= NSWindowCollectionBehavior.FullScreenPrimary;
			});
			
			Style.AddHandler<ApplicationHandler> ("application", h => {
				h.EnableFullScreen ();
			});
			
			Style.AddHandler<TreeViewHandler> ("userList", h => {
				h.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
				h.Scroll.BorderType = NSBorderType.NoBorder;
			});
			
			Generator.Detect.AddAssembly(typeof(MainClass).Assembly);
			
			var app = new JabbRApplication ();
			app.Initialized += delegate {
				NSUserDefaults.StandardUserDefaults.SetBool (true, "WebKitDeveloperExtras");
				NSUserDefaults.StandardUserDefaults.Synchronize();
			};
			app.Run (args);
		}
	}
}	

