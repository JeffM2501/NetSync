using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Gameplay.Players
{
	public class RemovePlayerMessage : NetworkingMessages.Messages.NetworkMessage
	{
		public int PlayerID = int.MinValue;
	}
}
