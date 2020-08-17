using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace mirai
{
    class Extract
    {
        public static void Main(string[] args)
        {
            if (File.GetAttributes(args[0]).HasFlag(FileAttributes.Directory))
            {
                Compress.FromFolder(args[0]);
                Environment.Exit(0);
            }
            var FI = new FileInfo(args[0]);
            string dir = Convert.ToString(FI.Name).Replace(".bin", "");
            if (args.Length == 0
            || !File.Exists(args[0])
            || !args[0].EndsWith(".bin")
            || !dir.StartsWith("spr_"))
                throw new Exception();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            dir += @"\\";
            var fs = new FileStream(args[0], FileMode.Open);
            var Textures = new List<string>();
            int hexIn;
            var hex = new List<string>();
            var tempHex = new List<string>();
            int tempCount = 0;
            var TextureData = new List<Texture>();
            var SpriteData = new List<object>();

            for (int i = 0; i < 8*4; i++)
            {
                hexIn = fs.ReadByte();
                tempCount++;
                tempHex.Add(string.Format("{0:X2}", hexIn));
                if (tempCount == 4)
                {
                    hex.Add(tempHex[3] + tempHex[2] + tempHex[1] + tempHex[0]);
                    tempCount = 0;
                    tempHex.Clear();
                }
            }

            GC.Collect();

            var HeaderData = new Header()
            {
                Flags = Convert.ToInt32(hex[0], 16),
                TexOffset = Convert.ToInt32(hex[1], 16),
                TexCount = Convert.ToInt32(hex[2], 16),
                TexNamesOffset = Convert.ToInt32(hex[3], 16),
                Unk01 = Convert.ToInt32(hex[4], 16),
                Unk02 = Convert.ToInt32(hex[5], 16),
                SprCount = Convert.ToInt32(hex[6], 16),
                SprOffset = Convert.ToInt32(hex[7], 16)
            };
            hex.Clear();

            fs.Seek(HeaderData.TexOffset, SeekOrigin.Begin);
            for (int j = 0; j < HeaderData.TexCount; j++)
            {
                string texLength = "";
                var texData = new List<string>();
                for (int i = 0; i < 1*4; i++)
                {
                    hexIn = fs.ReadByte();
                    tempCount++;
                    tempHex.Add(string.Format("{0:X2}", hexIn));
                    if (tempCount == 4)
                    {
                        texLength = tempHex[3] + tempHex[2] + tempHex[1] + tempHex[0];
                        tempCount = 0;
                        tempHex.Clear();
                    }
                }
                for (int i = 0; i < Convert.ToInt32(texLength, 16); i++)
                {
                    texData.Add(string.Format("{0:X2}", fs.ReadByte()));
                }
                Textures.Add(String.Join(String.Empty, texData.ToArray()));
            }

            fs.Seek(HeaderData.TexNamesOffset, SeekOrigin.Begin);
            for (int i = 0; i < HeaderData.TexCount; i++)
            {
                var texNames = new List<string>();
                for (int j = 0; j < 32; j++)
                {
                    texNames.Add(string.Format("{0:X2}", fs.ReadByte()) + " ");
                }
                string texString = String.Join(String.Empty, texNames);
                string[] WHY = texString.Split(' ', ' ');
                texNames.Clear();
                for (int j = 0; j < 32 - 1; j++)
                {
                    if (WHY[j] == "00") continue;
                    int fucking = Convert.ToInt32(WHY[j], 16);
                    texNames.Add(Char.ConvertFromUtf32(fucking));
                }
                texString = String.Join(String.Empty, texNames);
                Console.WriteLine(texString);
                TextureData.Add(new Texture()
                {
                    TextureData = Textures[i],
                    Name = String.Join(String.Empty, texNames)
                });
            }

            for (int i = 0; i < TextureData.Count; i++)
            {
                var Data = new List<byte>();
                var BWriter = new BinaryWriter(File.OpenWrite(dir + TextureData[i].Name + ".ctpk"));
                byte[] tempByte = StringToByteArray(TextureData[i].TextureData);

                for (int j = 0; j < tempByte.Length; j++)
                {
                    Data.Add(tempByte[j]);
                }

                foreach (byte bytes in Data)
                {
                    BWriter.Write(bytes);
                }
            }

            // Start sprite shit
            fs.Seek(HeaderData.SprOffset, SeekOrigin.Begin);
            for (int k = 0; k < HeaderData.SprCount; k++)
            {
                // Get flags and texInd
                int texInd = 0;
                int flags = 0;
                hex.Clear();

                for (int i = 0; i < 2*4; i++)
                {
                    hexIn = fs.ReadByte();
                    tempCount++;
                    tempHex.Add(string.Format("{0:X2}", hexIn));
                    if (tempCount == 4)
                    {
                        hex.Add(tempHex[3] + tempHex[2] + tempHex[1] + tempHex[0]);
                        tempCount = 0;
                        tempHex.Clear();
                    }
                }
                texInd = Convert.ToInt32(hex[0], 16);
                flags = Convert.ToInt32(hex[1], 16);
                hex.Clear();

                // Get spr name 
                var sprNames = new List<string>();

                for (int i = 0; i < 64; i++)
                {
                    sprNames.Add(string.Format("{0:X2}", fs.ReadByte()) + " "); 
                }
                string sprString = String.Join(String.Empty, sprNames);
                string[] sega = sprString.Split(' ', ' ');
                sprNames.Clear();
                for (int i = 0; i < 64 - 1; i++)
                {
                    if (sega[i] == "00") continue;
                    int fucker = Convert.ToInt32(sega[i], 16);
                    sprNames.Add(Char.ConvertFromUtf32(fucker));
                }
                sprString = String.Join(String.Empty, sprNames);
            
                // Get co-ords
                var coords = new List<float>();
                for (int i = 0; i < 4*4; i++)
                {
                    hexIn = fs.ReadByte();
                    tempCount++;
                    tempHex.Add(string.Format("{0:X2}", hexIn));
                    if (tempCount == 4)
                    {
                        hex.Add(tempHex[3] + tempHex[2] + tempHex[1] + tempHex[0]);
                        tempCount = 0;
                        tempHex.Clear();
                    }
                }
                for (int i = 0; i < hex.Count; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(hex[i], 16));
                    coords.Add(BitConverter.ToSingle(bytes, 0));
                }
                hex.Clear();

                // Get Pixel values
                int PX = 0;
                int PY = 0;
                int PWidth = 0;
                int PHeight = 0;

                for (int i = 0; i < 4*2; i++)
                {
                    hexIn = fs.ReadByte();
                    tempCount++;
                    tempHex.Add(string.Format("{0:X2}", hexIn));
                    if (tempCount == 2)
                    {
                        hex.Add(tempHex[1] + tempHex[0]);
                        tempCount = 0;
                        tempHex.Clear();
                    }
                }

                PX = Convert.ToInt32(hex[0], 16);
                PY = Convert.ToInt32(hex[1], 16);
                PWidth = Convert.ToInt32(hex[2], 16);
                PHeight = Convert.ToInt32(hex[3], 16);

                SpriteData.Add(new Sprite()
                {
                    TexIndex = texInd,
                    Flags = flags,
                    Name = sprString,
                    X = coords[0],
                    Y = coords[1],
                    Z = coords[2],
                    W = coords[3],
                    PX = PX,
                    PY = PY,
                    PWidth = PWidth,
                    PHeight = PHeight
                });
            }

            // Xml shit
            var xmlWriterSettings = new XmlWriterSettings() { Indent = true };
            var Unlock = new Unlock{ ObjectList = SpriteData, name =  "spriteData" };
            var xsSubmit = new XmlSerializer(typeof(Unlock), new XmlRootAttribute("spriteData"));
            string xml = "";
            using (var sww = new StringWriter())
            {
                using (var writers = XmlWriter.Create(sww, xmlWriterSettings))
                {
                    xsSubmit.Serialize(writers, Unlock);
                    xml = Convert.ToString(sww);
                }
            }
            if (!File.Exists(dir + "Sprite_Data.xml"))
                File.Create(dir + "Sprite_Data.xml");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\"?>");
            File.WriteAllText(dir + "Sprite_Data.xml", xml);
        }
        
        public static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}