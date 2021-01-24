
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml;

namespace Voltammogrammer
{
    public partial class Show_information : Form
    {
        public Show_information()
        {
            InitializeComponent();
        }

        private void Show_information_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void SetInformation(XmlNode node)
        {
            propertyGrid1.SelectedObject = new XmlNodeWrapper(node);

            propertyGrid1.ExpandAllGridItems();
        }
    }

    [TypeConverter(typeof(XmlNodeWrapperConverter))]
    class XmlNodeWrapper
    {
        private readonly XmlNode node;
        public XmlNodeWrapper(XmlNode node) { this.node = node; }

        class XmlNodeWrapperConverter : ExpandableObjectConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                List<PropertyDescriptor> props = new List<PropertyDescriptor>();
                XmlElement el = ((XmlNodeWrapper)value).node as XmlElement;
                if (el != null)
                {
                    foreach (XmlAttribute attr in el.Attributes)
                    {
                        props.Add(new XmlNodeWrapperPropertyDescriptor(attr));
                    }
                }
                foreach (XmlNode child in ((XmlNodeWrapper)value).node.ChildNodes)
                {
                    props.Add(new XmlNodeWrapperPropertyDescriptor(child));
                }
                return new PropertyDescriptorCollection(props.ToArray(), true);
            }
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                return destinationType == typeof(string)
                    ? ((XmlNodeWrapper)value).node.InnerXml
                    : base.ConvertTo(context, culture, value, destinationType);
            }
        }

        class XmlNodeWrapperPropertyDescriptor : PropertyDescriptor
        {
            private static readonly Attribute[] nix = new Attribute[0];
            private readonly XmlNode node;
            public XmlNodeWrapperPropertyDescriptor(XmlNode node) : base(GetName(node), nix)
            {
                this.node = node;
            }
            static string GetName(XmlNode node)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute: return "@" + node.Name;
                    case XmlNodeType.Element: return node.Name;
                    case XmlNodeType.Comment: return "<!-- -->";
                    case XmlNodeType.Text: return "(text)";
                    default: return node.NodeType + ":" + node.Name;
                }
            }
            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
            public override void SetValue(object component, object value)
            {
                node.Value = (string)value;
            }
            public override bool CanResetValue(object component)
            {
                return !IsReadOnly;
            }
            public override void ResetValue(object component)
            {
                SetValue(component, "");
            }
            public override Type PropertyType
            {
                get
                {
                    switch (node.NodeType)
                    {
                        case XmlNodeType.Element:
                            return typeof(XmlNodeWrapper);
                        default:
                            return typeof(string);
                    }
                }
            }
            public override bool IsReadOnly
            {
                get
                {
                    switch (node.NodeType)
                    {
                        case XmlNodeType.Attribute:
                        case XmlNodeType.Text:
                            return true; // すべての値を編集不可にする
                        default:
                            return true;
                    }
                }
            }
            public override object GetValue(object component)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        return new XmlNodeWrapper(node);
                    default:
                        return node.Value;
                }
            }
            public override Type ComponentType
            {
                get { return typeof(XmlNodeWrapper); }
            }
        }
    }
}
