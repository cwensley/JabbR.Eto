using System;
using Eto;
using System.Xml;
using System.Text.RegularExpressions;

namespace JabbR.Desktop.Model
{
    public class Highlight : IXmlReadable
    {
        public string Keyword { get; set; }

        public string RegEx { get { return Regex.Escape(Keyword); } }
        
        public void ReadXml(XmlElement element)
        {
            Keyword = element.GetStringAttribute("keyword");
        }
        
        public void WriteXml(XmlElement element)
        {
            element.SetAttribute("keyword", Keyword);
        }
        
        public static Highlight CreateFromXml(XmlElement element)
        {
            return new Highlight();
        }
        
    }
}

