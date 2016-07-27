using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;


namespace NetworkingMessages.Messages.Lobby
{
    public class RequestRoomListUpdateMessage : NetworkMessage
    {
        public static readonly RequestRoomListUpdateMessage Request = new RequestRoomListUpdateMessage();
    }
}
