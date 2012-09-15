using System;
using Eto.Forms;
using JabbR.Client;
using JabbR.Client.Models;
using Eto;
using JabbR.Eto.Model;

namespace JabbR.Eto.Interface
{
	public class TopSection : Panel, IXmlReadable
	{
		Splitter splitter;
		
		public Channels Channels { get; private set; }
		
		public Configuration Config { get; private set; }
		
		public TopSection (Configuration config)
		{
			this.Config = config;
			Channels = new Channels (config);
			Channels .ChannelChanged += HandleChannelChanged;
			
			splitter = new Splitter{
				Panel1 = Channels ,
				Position = 200
			};
			
			this.AddDockedControl (splitter);
			
			SetChannel ();
		}

		public void Initialize ()
		{
			Channels.Initialize ();
			SetChannel ();
		}		
		void HandleChannelChanged (object sender, EventArgs e)
		{
			SetChannel ();
		}

		void SetChannel ()
		{
			var channel = Channels.CreateChannel ();
			splitter.Panel2 = channel;
			if (channel != null)
				channel.Focus ();
		}

		public void CreateActions (GenerateActionArgs args)
		{
			Channels.CreateActions (args);
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

