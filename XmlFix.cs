using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections.Generic;

[Serializable]
public class Unlock :  IXmlSerializable
{
    public List<object> ObjectList { get; set; }

    public string name { get; set; }

    public XmlSchema GetSchema()
    {
        return new XmlSchema();
    }

    public void ReadXml(XmlReader reader)
    {

    }

    public void WriteXml(XmlWriter writer)
    {
        foreach (var obj in ObjectList)
        {
            writer.WriteStartElement(name);
            var properties = obj.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                writer.WriteElementString(propertyInfo.Name, propertyInfo.GetValue(obj).ToString());
            }
            writer.WriteEndElement();
        }
    }
}