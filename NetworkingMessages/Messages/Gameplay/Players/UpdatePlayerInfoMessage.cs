using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Gameplay.Players
{
	public class UpdatePlayerInfoMessage : NetworkingMessages.Messages.NetworkMessage
	{
		public int PlayerID = int.MinValue;
		public string DisplayName = string.Empty;

		// TODO, score and status and stuff
	}
}
