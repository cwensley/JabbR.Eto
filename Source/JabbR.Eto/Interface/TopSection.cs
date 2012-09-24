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
			Channels.ChannelChanged += HandleChannelChanged;
			
			splitter = new Splitter{
				Panel1 = Channels ,
				Position = 200
			};
			
			this.AddDockedControl (splitter);
			
			SetView ();
		}

		public new void Initialize ()
		{
			Channels.Initialize ();
			SetView ();
		}

		void HandleChannelChanged (object sender, EventArgs e)
		{
			SetView ();
		}

		void SetView ()
		{
			var view = Channels.CreateSection ();
			splitter.Panel2 = view;
			if (view != null)
				view.Focus ();
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

