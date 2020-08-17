using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace mirai
{
    class Compress
    {
        public static void FromFolder (string folder)
        {
            var SpriteData = new List<Sprite>();
            int SpriteOffset = 0;
            int SpriteLength = 0;
            int SpriteHexSize = 0;
            var TextureData = new List<Texture>();
            int TextureOffset = 0;
            int TextureLength = 0;
            int TextureHexSize = 0;
            int TexNameOffset = 40;

            var Names = new List<string>();
            var DInfo = new DirectoryInfo(folder);
            var imageFile = DInfo.GetFiles("*.ctpk");
            var sprFile = DInfo.GetFiles("*.xml");

            // Read Sprite File

            var doc = new XmlDocument();
            doc.Load(sprFile[0].FullName);
            var ReadNodes = new List<dynamic>();

            foreach (var node in doc.DocumentElement.ChildNodes)
            {
                ReadNodes.Add(node);
            }
            string[] xmlData = {};
            for (int i = 0; i < ReadNodes.Count; i++)
            {
                Array.Resize(ref xmlData, xmlData.Length + 1);
                xmlData[i] = ReadNodes[i].InnerXml;
                string[] xmlDataSplit = xmlData[i].Split(new string[] { "<", "/<", ">" }, StringSplitOptions.None);
                var TBA = new Sprite() 
                {
                    TexIndex = Convert.ToInt32(xmlDataSplit[2]),
                    Flags = Convert.ToInt32(xmlDataSplit[6]),
                    Name = xmlDataSplit[10],
                    X = float.Parse(xmlDataSplit[14]),
                    Y = float.Parse(xmlDataSplit[18]),
                    Z = float.Parse(xmlDataSplit[22]),
                    W = float.Parse(xmlDataSplit[26]),
                    PX = Convert.ToInt32(xmlDataSplit[30]),
                    PY = Convert.ToInt32(xmlDataSplit[34]),
                    PWidth = Convert.ToInt32(xmlDataSplit[38]),
                    PHeight = Convert.ToInt32(xmlDataSplit[42])
                };
                SpriteData.Add(TBA);
            }
            SpriteLength = ReadNodes.Count;
            SpriteHexSize = 24*4*SpriteLength;
            
            for (int i = 0; i < imageFile.Length; i++)
            {
                Names.Add(Convert.ToString(imageFile[i]).Replace(".ctpk" , ""));
            }

            
        }
    }
}