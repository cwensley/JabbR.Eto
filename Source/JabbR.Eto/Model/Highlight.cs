using System;
using Eto;
using System.Xml;

namespace JabbR.Eto.Model
{
	public class Highlight : IXmlReadable
	{
		public string Keyword { get; set; }
		
		public void ReadXml (XmlElement element)
		{
			Keyword = element.GetStringAttribute ("keyword");
		}
		
		public void WriteXml (XmlElement element)
		{
			element.SetAttribute ("keyword", Keyword);
		}
		
		public static Highlight CreateFromXml (XmlElement element)
		{
			return new Highlight();
		}
		
	}
}

