using System;
using Eto.Forms;
using JabbR.Client;
using JabbR.Client.Models;
using Eto;
using JabbR.Eto.Controls;

namespace JabbR.Eto.Sections
{
	public class TopSection : Panel, IXmlReadable
	{
		Splitter splitter;
		Channels channels;
		//ChannelSection channel;
		
		public TopSection ()
		{
			channels = new Channels ();
			channels.ChannelChanged += HandleChannelChanged;
			
			splitter = new Splitter{
				Panel1 = channels,
				Position = 200
			};
			
			this.AddDockedControl (splitter);
			
			SetChannel ();
		}
		
		public void Initialize (ConnectionInfo info)
		{
			channels.Initialize (info);
		}
		
		public void Connected ()
		{
			channels.Connected ();
		}
		
		void HandleChannelChanged (object sender, EventArgs e)
		{
			SetChannel ();
		}
		
		void SetChannel ()
		{
			var channel = channels.CreateChannel ();
			splitter.Panel2 = channel;
			if (channel != null)
				channel.Focus ();
		}
		
		#region IXmlReadable implementation
		
		public void ReadXml (System.Xml.XmlElement element)
		{
			splitter.Position = element.GetIntAttribute ("split") ?? 200;
		}

		public void WriteXml (System.Xml.XmlElement element)
		{
			element.SetAttribute ("split", splitter.Position);
		}
		
		#endregion
	}
}

