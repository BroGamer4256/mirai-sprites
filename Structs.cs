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
        public int TexIndex {get; set;}
        public int Flags {get; set;}
        public string Name {get; set;}
        public float X {get; set;}
        public float Y {get; set;}
        public float Z {get; set;}
        public float W {get; set;}
        public int PX {get; set;}
        public int PY {get; set;}
        public int PWidth {get; set;}
        public int PHeight {get; set;}
    }
}