using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Eto;
using Eto.Platform.Mac.Forms.Controls;

namespace JabbR.Eto.Mac
{
	class MainClass
	{
		
		static void Main (string[] args)
		{
			Style.AddHandler<ListBoxHandler>("mainList", (h) => {
				h.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
				h.Scroll.BorderType = NSBorderType.NoBorder;
			});
			var app = new JabbRApplication ();
			app.Run (args);
		}
	}
}	

