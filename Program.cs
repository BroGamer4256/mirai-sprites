using System;
using System.IO;
using System.Collections.Generic;

namespace mirai
{
    class Extract
    {
        public static void Main(string[] args)
        {
            var fs = new FileStream(args[0], FileMode.Open);
            var Textures = new List<string>();
            int hexIn;
            var hex = new List<string>();
            var tempHex = new List<string>();
            int tempCount = 0;
            var TextureData = new List<Texture>();

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
                var BWriter = new BinaryWriter(File.OpenWrite(TextureData[i].Name + ".ctpk"));
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