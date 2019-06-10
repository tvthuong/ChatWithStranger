using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Library
{
    public abstract class RoomBase
    {
        public string Id { get; set; }
        public Type Type { get; set; }
        public bool IsPrivateRoom => Type == Type.Private;
        public RoomBase() { }
    }
}
