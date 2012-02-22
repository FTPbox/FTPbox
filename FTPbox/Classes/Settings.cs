using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace FTPbox.Classes
{
    public class Settings
    {
        XmlDocument xmlDocument = new XmlDocument();
        string documentPath = Application.UserAppDataPath + "\\settings.xml";

        public  Settings()
        { 
            try {xmlDocument.Load(documentPath);}
            catch {xmlDocument.LoadXml("<settings></settings>");}
        }

        public int Get(string xPath, int defaultValue)
        { 
            return Convert.ToInt16(Get(xPath, Convert.ToString(defaultValue))); 
        }

        public void Put(string xPath, int value)
        { 
            Put(xPath, Convert.ToString(value)); 
        }

        public string Get(string xPath,  string defaultValue)
        { XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath );
          if (xmlNode != null) {return xmlNode.InnerText;}
          else { return defaultValue;}
        }

        public void Put(string xPath,  string value)
        { XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
          if (xmlNode == null) { xmlNode = createMissingNode("settings/" + xPath); }
          xmlNode.InnerText = value;
          xmlDocument.Save(documentPath);
        }

        private XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            XmlNode testNode = null;
            XmlNode currentNode = xmlDocument.SelectSingleNode("settings");
            foreach (string xPathSection in xPathSections)
            {
                currentXPath += xPathSection;
                testNode = xmlDocument.SelectSingleNode(currentXPath);
                if (testNode == null) { currentNode.InnerXml += "<" + xPathSection + "></" + xPathSection + ">"; }
                currentNode = xmlDocument.SelectSingleNode(currentXPath);
                currentXPath += "/";
            }
            return currentNode;
        }
    }
}
