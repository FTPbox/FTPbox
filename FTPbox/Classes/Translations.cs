using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace FTPbox.Classes
{
    public class Translations
    {
        XmlDocument xmlDocument = new XmlDocument();
        string documentPath = Application.StartupPath + "\\translations.xml";

        public Translations()
        {
            try { xmlDocument.Load(documentPath); }
            catch (Exception ex) { Log.Write(l.Info, "?>" + ex.Message); xmlDocument.LoadXml("<translations></translations>"); }
        }

        public int Get(string xPath, int defaultValue)
        {
            return Convert.ToInt16(Get(xPath, Convert.ToString(defaultValue)));
        }

        public void Put(string xPath, int value)
        {
            Put(xPath, Convert.ToString(value));
        }

        public string Get(string xPath, string defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("translations/" + xPath);
            if (xmlNode != null) { return xmlNode.InnerText.Replace("_and", "&"); }
            else { return defaultValue; }
        }

        public void Put(string xPath, string value)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("translations/" + xPath);
            if (xmlNode == null) { xmlNode = createMissingNode("translations/" + xPath); }
            xmlNode.InnerText = value;
            xmlDocument.Save(documentPath);
        }

        private XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            XmlNode testNode = null;
            XmlNode currentNode = xmlDocument.SelectSingleNode("translations");
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
