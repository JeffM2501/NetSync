using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages;

namespace NetworkingMessages.Messages.Connection
{
	public class ClientVersionMessage : NetworkMessage
	{
		public string ClientProduct = "NetSync";
		public int ClientProtocol = 1;
		public string ClientVersion = string.Empty;
	}
}
