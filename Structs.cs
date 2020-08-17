using System;
using System.Collections.Generic;

namespace mirai
{
    public struct Header
    {
        public int Flags {get; set;}
        public int TexOffset {get; set;}
        public int TexCount {get; set;}
        public int TexNamesOffset {get; set;}
        public int Unk01 {get; set;}
        public int Unk02 {get; set;}
        public int SprCount {get; set;}
        public int SprOffset {get; set;}
    }

    public struct Texture
    {
        public string TextureData {get; set;}
        public string Name {get; set;}
    }

    public struct Sprite
    {
        
    }
}