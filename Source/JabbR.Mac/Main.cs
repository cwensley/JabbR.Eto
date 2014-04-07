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
using JabbR.Mac.Controls;
using System.IO;
using JabbR.Desktop;

namespace JabbR.Mac
{
    class MainClass
    {
        static void Main(string[] args)
        {
#if DEBUG
            Debug.Listeners.Add (new ConsoleTraceListener());
#endif
            // so we don't link out mono.runtime for detection

            var generator = Generator.Detect;
            generator.Add <IJabbRApplication>(() => new JabbRApplicationHandler());

            NSApplication.CheckForIllegalCrossThreadCalls = false;

            Style.Add<TreeViewHandler>("channelList", handler => {
                handler.Control.Delegate = new CustomTreeViewDelegate { Handler = handler, AllowGroupSelection = true };
                handler.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
                handler.Scroll.BorderType = NSBorderType.NoBorder;
            });
            
            Style.Add<FormHandler>("mainForm", handler => handler.Control.CollectionBehavior |= NSWindowCollectionBehavior.FullScreenPrimary);
            
            Style.Add<ApplicationHandler>("application", handler => handler.EnableFullScreen());
            
            Style.Add<TreeViewHandler>("userList", handler => {
                handler.Control.Delegate = new CustomTreeViewDelegate { Handler = handler };
                handler.Control.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;
                handler.Scroll.BorderType = NSBorderType.NoBorder;
                
            });
            
            Style.Add<WebViewHandler>(null, handler => handler.Control.Preferences.UsesPageCache = false);
            
            var app = new JabbRApplication();
            app.Initialized += delegate
            {
#if DEBUG
                NSUserDefaults.StandardUserDefaults.SetBool (true, "WebKitDeveloperExtras");
                NSUserDefaults.StandardUserDefaults.Synchronize();
#endif
            };
            app.Run(args);
        }
    }
}   

