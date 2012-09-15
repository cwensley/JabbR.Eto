using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Client;
using JabbR.Client.Models;
using System.IO;
using Eto;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR.Eto.Model;
using System.Diagnostics;

namespace JabbR.Eto.Interface
{
	public abstract class MessageSection : Panel
	{
		protected string LastHistoryMessageId { get; private set; }
		
		protected WebView History { get; private set; }
		protected TextBox TextEntry { get; private set; }
		
		struct DelayedCommand
		{
			public string Command { get; set; }
			public object Parameter { get; set; }
		}
		
		List<DelayedCommand> delayedCommands;
		bool loaded;
		object sync = new object();
		
		public MessageSection ()
		{
			History = new WebView ();
			History.DocumentLoaded += HandleDocumentLoaded;
			TextEntry = MessageEntry ();
		}
		
		public override void OnPreLoad (EventArgs e)
		{
			base.OnPreLoad (e);
			CreateLayout(this);
		}
		
		protected virtual void CreateLayout(Container container)
		{
			var layout = new DynamicLayout (container, Padding.Empty, Size.Empty);
			layout.Add (History, yscale: true);
			layout.Add (DockLayout.CreatePanel (TextEntry, new Padding (10)));
		}

		void HandleDocumentLoading (object sender, WebViewLoadingEventArgs e)
		{
			e.Cancel = true;
			Console.WriteLine ("Opening {0}", e.Uri.AbsoluteUri);
			Application.Instance.Open (e.Uri.AbsoluteUri);
		}
		
		public override void OnLoadComplete (EventArgs e)
		{
			base.OnLoadComplete (e);
			
			var resourcePath = EtoEnvironment.GetFolderPath (EtoSpecialFolder.ApplicationResources);
			resourcePath = Path.Combine (resourcePath, "Styles", "default");
			resourcePath += Path.DirectorySeparatorChar;
			//History.LoadHtml (File.OpenRead (Path.Combine (resourcePath, "channel.html")), new Uri(resourcePath));
			History.Url = new Uri (Path.Combine (resourcePath, "channel.html"));
		}

		protected virtual void HandleDocumentLoaded (object sender, WebViewLoadedEventArgs e)
		{
			lock (sync) {
				loaded = true;
				if (delayedCommands != null) {
					foreach (var command in delayedCommands) {
						SendCommandInternal (command.Command, command.Parameter);
					}
					delayedCommands = null;
				}
				History.DocumentLoading += HandleDocumentLoading;
			}
		}
		
		public void AddMessage (ChannelMessage message)
		{
			SendCommand("addMessage", message);
		}
		
		public void AddHistory (IEnumerable<ChannelMessage> messages)
		{
			SendCommand("addHistory", messages);
			if (messages.Count() > 0)
				LastHistoryMessageId = messages.First ().Id;
			else
				LastHistoryMessageId = null;
		}
		
		public void AddNotification (NotificationMessage notification)
		{
			SendCommand("addNotification", notification);
		}

		public void AddMessageContent (MessageContent content)
		{
			SendCommand("addMessageContent", content);
		}
		
		protected void SendCommand(string command, object param)
		{
			lock (sync) { 
				if (!loaded) {
					if (delayedCommands == null) delayedCommands = new List<DelayedCommand>();
					delayedCommands.Add (new DelayedCommand { Command = command, Parameter = param });
					return;
				}
			}
			SendCommandInternal(command, param);
		}
		
		void SendCommandInternal (string command, object param)
		{
			var msgString = JsonConvert.SerializeObject (param);
			var script = string.Format ("JabbREto.{0}({1});", command, msgString);
			Application.Instance.Invoke (() => {
				History.ExecuteScript (script);
			});
		}

		TextBox MessageEntry ()
		{
			var control = new TextBox {
				PlaceholderText = "Send Message..."
			};
			control.KeyDown += (sender, e) => {
				if (e.KeyData == Key.Enter) {
					ProcessCommand (control.Text);
					control.Text = string.Empty;
					e.Handled = true;
				}
			};
			control.TextChanged += (sender, e) => {
				UserTyping ();
			};
			return control;
		}
		
		public abstract void ProcessCommand (string command);
		
		public virtual void UserTyping()
		{
		}
		
		public override void Focus ()
		{
			TextEntry.Focus ();
		}
	}
}

