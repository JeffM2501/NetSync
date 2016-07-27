using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.Lobby
{
    public class RoomListUpdateMessage : NetworkMessage
    {
        public long RoomID = long.MinValue;
        public string RoomName = string.Empty;

        public RoomListUpdateMessage() { }
        public RoomListUpdateMessage(Tuple<long,string> d) { RoomID = d.Item1; RoomName = d.Item2; }
    }
}
