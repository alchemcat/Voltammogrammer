
/*
    PocketPotentiostat

    Copyright (C) 2019 Yasuo Matsubara

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;


namespace Voltammogrammer
{
    public class XMLDataHolder
    {
        XmlDocument _doc;
        XmlElement _root;

        public XMLDataHolder(string name= "XMLDataHolder")
        {
            _doc = new XmlDocument();
            XmlNode xmlnode = _doc.CreateElement(name);
            _doc.AppendChild(xmlnode);

            _root = _doc[name];

            //XmlAttribute xmlattr = _doc.CreateAttribute("id");
            //xmlattr.Value = id;

            //_root.Attributes.Append(xmlattr);
        }

        public XMLDataHolder(XmlNode node)
        {
            _doc = new XmlDocument();
            XmlNode root = _doc.ImportNode(node, true);

            _doc.AppendChild(root);

            _root = _doc.DocumentElement;
            //XmlElement _root2 = _doc["XMLDataHolder"];
        }

        public XmlNode GetData()
        {
            return _root;
        }

        public XmlNodeList GetData(string name)
        {
            return _doc.SelectNodes("//" + name + "");
        }

        public void AddData(XmlNode node)
        {
            XmlNode root = _doc.ImportNode(node, true);

            _root.AppendChild(root);
        }

        public void LoadDataFromString(string outerxml)
        {
            try
            {
                _doc.LoadXml(outerxml);
                _root = _doc.DocumentElement;
            }
            catch (System.Xml.XmlException)
            {

            }
        }

        //public string Serialize()
        //{
        //    return _doc.OuterXml;
        //}

        //public void Deserialize(string xml)
        //{
        //    _doc.LoadXml(xml);
        //}

        public string GetDatum(string key, string default_value)
        {
            XmlNodeList xmllist = _doc.SelectNodes("//" + key + "");

            if(xmllist.Count == 1)
            {
                return xmllist[0].InnerXml;
            }
            else
            {
                return default_value;
            }
        }

        public string GetDatumAttribute(string key, string attr)
        {
            XmlNodeList xmllist = _doc.SelectNodes("//" + key + "/@" + attr);

            if (xmllist.Count == 1)
            {
                return xmllist[0].Value;
            }
            else
            {
                return "";
            }
        }

        public void SetDatum(string key, string data)
        {
            XmlNodeList xmllist = _doc.SelectNodes("//" + key + "");

            if (xmllist.Count >= 1)
            {
                xmllist[0].InnerXml = data;
            }
            else
            {
                //XmlAttribute xmlattr = _doc.CreateAttribute("id");
                //xmlattr.Value = key;

                XmlNode xmlnode = _doc.CreateElement(key);
                //xmlnode.Attributes.Append(xmlattr);
                xmlnode.InnerXml = data;

                _root.AppendChild(xmlnode);
            }
        }

        public void SetDatumAttribute(string key, string attr, string data)
        {
            XmlNodeList xmllist = _doc.SelectNodes("//" + key + "/@" + attr);

            if (xmllist.Count >= 1)
            {
                xmllist[0].Value = data;
            }
            else
            {
                XmlNodeList xmllist2 = _doc.SelectNodes("//" + key);

                if (xmllist2.Count >= 1)
                {
                    //XmlNode xmlnode = _doc.CreateElement(key);
                    XmlAttribute xmlattr = _doc.CreateAttribute(attr);
                    xmlattr.Value = data;
                    xmllist2[0].Attributes.Append(xmlattr);
                }
                else
                {
                    XmlNode xmlnode = _doc.CreateElement(key);
                    XmlAttribute xmlattr = _doc.CreateAttribute(attr);
                    xmlattr.Value = data;
                    xmlnode.Attributes.Append(xmlattr);
                    _root.AppendChild(xmlnode);
                }
            }
        }
    }
}
