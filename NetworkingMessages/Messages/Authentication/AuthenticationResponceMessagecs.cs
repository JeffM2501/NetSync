using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Authentication
{
	public class AuthenticationResponceMessagecs : NetworkingMessages.Messages.NetworkMessage
	{
		public string UserID = string.Empty;
		public string DisplayName = string.Empty;
		public enum AuthenticationResponces
		{
			Accepted,
			Denied,
			DeniedUnverified,
		}
		public AuthenticationResponces Responce = AuthenticationResponces.Denied;

	}
}
