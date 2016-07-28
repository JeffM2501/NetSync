using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkingMessages.Messages.Lobby.Chat;

namespace ClientLib
{
	public class LobbySession
	{
		public class LobbyUser
		{
			public string Name = string.Empty;
			public List<string> ChatLog = new List<string>();

			public LobbyUser(string n)
			{
				Name = n;
			}
		}

		public Dictionary<string, LobbyUser> LobbyUsers = new Dictionary<string, LobbyUser>();

		public List<Tuple<string, string>> ChatLog = new List<Tuple<string, string>>();

		private bool GotMe = false;

		public class ChatUserMessageEventArgs : EventArgs
		{
			public string User = string.Empty;

			public ChatUserMessageEventArgs(string f) { User = f; }
		}

		public event EventHandler<ChatUserMessageEventArgs> UserJoined = null;
		public event EventHandler<ChatUserMessageEventArgs> UserLeft = null;

		public event EventHandler UserListSetup= null;

		public class ChatMessageEventArgs : EventArgs
		{
			public string From = string.Empty;
			public string ChatText = string.Empty;

			public ChatMessageEventArgs(string f, string t) { From = f;  ChatText = t; }
		}
		public event EventHandler<ChatMessageEventArgs> ChatUpdated = null;
		public event EventHandler<ChatMessageEventArgs> ChatSent = null;

		private string MyUserName = string.Empty;
		public LobbySession(string me)
		{
			MyUserName = me;
		}

		public void ReceiveChatMessage(string from, string messageText)
		{
			LobbyUser user = GetUser(from);
			user.ChatLog.Add(messageText);
			ChatLog.Add(new Tuple<string, string>(from, messageText));
		}

		public void SendChatMessage(string messageText)
		{
			ChatSent?.Invoke(this, new ChatMessageEventArgs(MyUserName, messageText));
		}

		protected LobbyUser GetUser(string UserName)
		{
			if(!LobbyUsers.ContainsKey(UserName))
				AddChatUser(UserName);

			return LobbyUsers[UserName];
		}

		protected void AddChatUser(string UserName)
		{
			LobbyUsers.Add(UserName, new LobbyUser(UserName));
			if(GotMe)
				UserJoined?.Invoke(this, new ChatUserMessageEventArgs(UserName));
			else
			{
				if(UserName == MyUserName)
				{
					GotMe = true;
					UserListSetup?.Invoke(this, EventArgs.Empty);
				}

			}
		}

		public void UpdateChatUser(ChatMemberStatusMessage message)
		{
			switch(message.Status)
			{
				case ChatMemberStatusMessage.StatusTypes.Joined:
					AddChatUser(message.UserName);
					break;

				case ChatMemberStatusMessage.StatusTypes.Disconnected:
				case ChatMemberStatusMessage.StatusTypes.Left:
					LobbyUsers.Remove(message.UserName);
					if (GotMe)
						UserLeft?.Invoke(this, new ChatUserMessageEventArgs(message.UserName));
					break;
			}
		}
	}
}
