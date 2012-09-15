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
		
		class MyDel : TreeViewHandler.EtoOutlineDelegate
		{
			public bool AllowGroupSelection { get; set; }
			
			public override bool IsGroupItem (NSOutlineView outlineView, NSObject item)
			{
				return outlineView.LevelForItem(item) == 0;
			}
			
			public override void WillDisplayCell (NSOutlineView outlineView, NSObject cell, NSTableColumn tableColumn, NSObject item)
			{
				var textCell = cell as MacImageListItemCell;
				if (textCell != null) {
					textCell.UseTextShadow = true;
					textCell.SetGroupItem (this.IsGroupItem (outlineView, item), outlineView, NSFont.SmallSystemFontSize, NSFont.SmallSystemFontSize);
				}
			}
			
			public override float GetRowHeight (NSOutlineView outlineView, NSObject item)
			{
				return 18;
			}
			
			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				return AllowGroupSelection || !IsGroupItem(outlineView, item);
			}
		}
		
		static void Main (string[] args)
		{
#if DEBUG
			//Debug.Listeners.Add (new ConsoleTraceListener());
#endif
			Generator.Detect.AddAssembly(typeof(MainClass).Assembly);

			Style.Add<TreeViewHandler>("channelList", h => {
				h.Control.Delegate = new MyDel { Handler = h, AllowGroupSelection = true };
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
				h.Control.Delegate = new MyDel { Handler = h };
				h.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
				h.Scroll.BorderType = NSBorderType.NoBorder;
				
			});
			
			
			
			var app = new JabbRApplication ();
			app.Initialized += delegate {
				NSUserDefaults.StandardUserDefaults.SetBool (true, "WebKitDeveloperExtras");
				NSUserDefaults.StandardUserDefaults.Synchronize();
			};
			app.Run (args);
		}
	}
}	

