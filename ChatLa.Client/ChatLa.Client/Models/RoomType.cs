using System;
using System.Collections.Generic;
using System.Text;

namespace ChatLa.Client.Models
{
    public class RoomType
    {
        public RoomType() { }
        public Common_Library.Type Type { get; set; }
        public string Description => new Helpers.TranslateExtension() { Text = Type == Common_Library.Type.Public ? "RoomTypePublic" : "RoomTypePrivate" }.ProvideValue(null).ToString();
    }
}
