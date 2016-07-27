using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.Lobby
{
    public class CreateRoomMessage : NetworkMessage
    {
        public string RoomName = string.Empty;
    }
}
