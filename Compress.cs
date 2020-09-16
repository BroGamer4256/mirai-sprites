using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace mirai
{
    class Compress
    {
        public static void FromFolder (string folder)
        {
            var MainData = new List<byte>();
            var DataList = new List<string[]>();
            var BWriter = new BinaryWriter(File.OpenWrite(folder + ".bin"));

            var SpriteData = new List<Sprite>();
            long SpriteOffset = 0;
            int SpriteLength = 0;
            long SpriteHexSize = 0;
            var TextureData = new List<Texture>();
            long TextureOffset = 0;
            int TextureLength = 0;
            long TextureHexSize = 0;
            int TexNameOffset = 40;
            long TexNameHexSize = 0;

            var Names = new List<string>();
            var DInfo = new DirectoryInfo(folder);
            var imageFile = DInfo.GetFiles("*.ctpk");
            var sprFile = DInfo.GetFiles("*.xml");

            var hex = new List<string>();

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

            for (int i = 0; i < imageFile.Length; i++)
            {
                var fs = new FileStream(Convert.ToString(imageFile[i].FullName), FileMode.Open);
                for (int j = 0; j < imageFile[i].Length; j++)
                {
                    hex.Add(string.Format("{0:X2}", fs.ReadByte()));
                }
                TextureData.Add(new Texture()
                {
                    TextureData = String.Join(String.Empty, hex),
                    Name = Names[i]
                });
                fs.Close();
                hex.Clear();
                TextureHexSize += imageFile[i].Length;
            }

            GC.Collect();

            TexNameHexSize = TextureData.Count * 32;
            TextureLength = TextureData.Count;
            SpriteOffset = TexNameHexSize + TexNameOffset;
            TextureOffset = SpriteHexSize + SpriteOffset;

            DataList.Add(IntHex(0)); // Flags
            DataList.Add(IntHex((int)TextureOffset));
            DataList.Add(IntHex(TextureLength));
            DataList.Add(IntHex(TexNameOffset));
            DataList.Add(IntHex(0)); // Padding idk
            DataList.Add(IntHex(0)); // Padding idk
            DataList.Add(IntHex(SpriteLength));
            DataList.Add(IntHex((int)SpriteOffset));
            DataList.Add(IntHex(0)); // Padding idk
            DataList.Add(IntHex(0)); // Padding idk

            for (int i = 0; i < TextureLength; i++)
            {
                DataList.Add(StringHex(TextureData[i].Name, 64));
            }

            for (int i = 0; i < SpriteLength; i++)
            {
                DataList.Add(IntHex(SpriteData[i].TexIndex));
                DataList.Add(IntHex(SpriteData[i].Flags));
                DataList.Add(StringHex(SpriteData[i].Name, 128));
                DataList.Add(FloatString(SpriteData[i].X));
                DataList.Add(FloatString(SpriteData[i].Y));
                DataList.Add(FloatString(SpriteData[i].Z));
                DataList.Add(FloatString(SpriteData[i].W));
                DataList.Add(IntHex3(SpriteData[i].PX));
                DataList.Add(IntHex3(SpriteData[i].PY));
                DataList.Add(IntHex3(SpriteData[i].PWidth));
                DataList.Add(IntHex3(SpriteData[i].PHeight));
            }

            for (int i = 0; i < TextureLength; i++)
            {
                DataList.Add(IntHex((int)imageFile[i].Length));
                string[] dummy = {TextureData[i].TextureData};
                DataList.Add(dummy);
            }

            foreach (string[] item in DataList)
            {
                foreach (string item2 in item)
                {
                    string[] why = SO(item2, 2);
                    foreach (string bitch in why)
                    {
                        //Console.WriteLine(bitch);
                        byte[] tempByte = Extract.StringToByteArray(bitch);
                        for (int i = 0; i < tempByte.Length; i++)
                        {
                            MainData.Add(tempByte[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < MainData.Count; i++)
            {
                BWriter.Write(MainData[i]);
            }
            BWriter.Close();
        }

        public static string[] SO (string s, int length)
        {
            var dummy = new List<string>();
            for (var i = 0; i < s.Length; i += length)
                dummy.Add(s.Substring(i, Math.Min(length, s.Length - i)));
            return dummy.ToArray();
        }

        public static string[] StringHex (string bitch, int padLength)
        {
            byte[] ba = Encoding.ASCII.GetBytes(bitch);
            string dummy = BitConverter.ToString(ba);
            dummy = dummy.Replace("-", "");
            dummy = dummy.PadRight(padLength, '0');
            string[] dummyA = {dummy};
            return dummyA;
        }

        public static string[] FloatString (float bitch)
        {
            byte[] bytes = BitConverter.GetBytes(bitch);
            string dummy = BitConverter.ToString(bytes);
            dummy = dummy.Replace("-", "");
            dummy = dummy.PadLeft(8, '0');
            string[] dummyA = {dummy.Substring(0,2), dummy.Substring(2,2), dummy.Substring(4,2), dummy.Substring(6,2)};
            return dummyA;
        }

        public static string[] IntHex (int bitch)
        {
            string dummy = bitch.ToString("X");
            dummy = dummy.PadLeft(8, '0');
            string[] dummyA = {dummy.Substring(6,2), dummy.Substring(4,2), dummy.Substring(2,2), dummy.Substring(0,2)};
            return dummyA;
        }
        
        public static string[] IntHex3 (int bitch)
        {
            string dummy = bitch.ToString("X");
            dummy = dummy.PadLeft(4, '0');
            string[] dummyA = {dummy.Substring(2,2), dummy.Substring(0,2)};
            return dummyA;
        }
    }
}