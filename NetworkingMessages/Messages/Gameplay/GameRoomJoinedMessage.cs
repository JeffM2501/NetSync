using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.Gameplay
{
	public class GameRoomJoinedMessage: NetworkMessage
	{
		public long GameID = long.MinValue;
		public string GameName = string.Empty;

		// TODO, other data and stuff.... like world and the like

		// the local player ID of the player who joined
		public int PlayerID = int.MinValue;
	}
}
