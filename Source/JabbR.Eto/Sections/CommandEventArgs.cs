using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Client;
using JabbR.Client.Models;
using System.IO;
using Eto;
using Newtonsoft.Json;

namespace JabbR.Eto.Sections
{
	public sealed class CommandEventArgs : EventArgs
	{
		public string Command { get; private set; }
		
		public CommandEventArgs (string command)
		{
			this.Command = command;
		}
	}
	
}
