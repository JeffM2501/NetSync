using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using NetworkingMessages;
using NetworkingMessages.Messages;
using NetworkingMessages.Messages.Connection;
using NetworkingMessages.Messages.Authentication;
using NetworkingMessages.Messages.ClientState;


namespace ClientLib
{
	public partial class Client
	{
		protected AuthenticationResponceMessagecs LastAuthenticationMessage = null;

		protected SetClientState.ClientStates CurrentState = SetClientState.ClientStates.Connecting;

		protected void SetClientConnecting()
		{
			ChangeClientState(SetClientState.ClientStates.Connecting);
		}
		protected void SetClientDisconnecting()
		{
			ChangeClientState(SetClientState.ClientStates.Disconnecting);
		}

		public event EventHandler StartAuthentication = null;
		public event EventHandler JoinedLobby = null;
		public event EventHandler LeftLobby = null;
		public event EventHandler JoinedGame = null;
		public event EventHandler LeftGame = null;
		public event EventHandler Disconnecting = null;


		protected void ChangeClientState(SetClientState.ClientStates state)
		{
			switch(CurrentState)
			{
				case SetClientState.ClientStates.InLobby:
					LeftLobby?.Invoke(this, EventArgs.Empty);
					break;

				case SetClientState.ClientStates.Playing:
					LeftGame?.Invoke(this, EventArgs.Empty);
					break;
			}
			CurrentState = state;

			switch(CurrentState)
			{
				case SetClientState.ClientStates.Authenticating:
					StartAuthentication?.Invoke(this, EventArgs.Empty);
					break;

				case SetClientState.ClientStates.InLobby:
					JoinedLobby?.Invoke(this, EventArgs.Empty);
					CurrentLobby = new LobbySession(LastAuthenticationMessage.UserID);
					break;

				case SetClientState.ClientStates.Playing:
					JoinedGame?.Invoke(this, EventArgs.Empty);
					break;

				case SetClientState.ClientStates.Disconnecting:
					Disconnecting?.Invoke(this, EventArgs.Empty);
					break;

			}

		}

		public static int MaxMessagesPerTick = 100;

		public void Update()
		{
			NetworkMessage msg = PopMessage();
			int i = 0;

			while(msg != null && i < MaxMessagesPerTick)
			{
				switch(CurrentState)
				{
					case SetClientState.ClientStates.Connecting:
						ProcessConnectionMessage(msg);
						break;

					case SetClientState.ClientStates.Authenticating:
						ProcessAuthenticationMessage(msg);
						break;

					case SetClientState.ClientStates.InLobby:
						ProcessLobbyMessage(msg);
						break;
				}
			}
		}

		protected void ProcessConnectionMessage(NetworkMessage msg)
		{
			SetClientState stateMsg = msg as SetClientState;
			if(stateMsg != null)
			{
				if(stateMsg.State == SetClientState.ClientStates.Authenticating)
				{
					CurrentState = SetClientState.ClientStates.Authenticating;
					return;
				}
			}
		}

		public class AuthenticationCredentialsEventArgs : EventArgs
		{
			public string UserID = string.Empty;
			public string Token = string.Empty;
			public string TokenSource = string.Empty;
		}

		public event EventHandler<AuthenticationCredentialsEventArgs> GetAuthenticationCredentials = null;

		protected void ProcessAuthenticationMessage(NetworkMessage msg)
		{
			SetClientState stateMsg = msg as SetClientState;
			if(stateMsg != null)
			{
				if(stateMsg.State == SetClientState.ClientStates.InLobby || stateMsg.State == SetClientState.ClientStates.Playing || stateMsg.State == SetClientState.ClientStates.Disconnecting)
				{
					ChangeClientState(stateMsg.State);
					return;
				}
			}

			RequestClientVersionMessage versMsg = msg as RequestClientVersionMessage;
			if(versMsg != null)
			{
				ClientVersionMessage myVersion = new ClientVersionMessage();
				SendMessage(myVersion);
			}

			RequestAuthentication authRequestMsg = msg as RequestAuthentication;
			if(authRequestMsg != null)
			{
				AuthenticationMessage myAuth = new AuthenticationMessage();

				AuthenticationCredentialsEventArgs args = new AuthenticationCredentialsEventArgs();
				GetAuthenticationCredentials?.Invoke(this, args);

				myAuth.UserID = args.UserID;
				myAuth.AuthenticationToken = args.Token;
				myAuth.AuthenticationSourceToken = args.TokenSource;

				SendMessage(myAuth);
			}
		}
		protected void ProcessLobbyMessage(NetworkMessage msg)
		{
			SetClientState stateMsg = msg as SetClientState;
			if(stateMsg != null)
			{
				if(stateMsg.State == SetClientState.ClientStates.InLobby || stateMsg.State == SetClientState.ClientStates.Playing || stateMsg.State == SetClientState.ClientStates.Disconnecting)
				{
					ChangeClientState(CurrentState);
					return;
				}
			}

			RequestClientVersionMessage versMsg = msg as RequestClientVersionMessage;
			if(versMsg != null)
			{
				ClientVersionMessage myVersion = new ClientVersionMessage();
				SendMessage(myVersion);
				return;
			}

			RequestAuthentication authRequestMsg = msg as RequestAuthentication;
			if(authRequestMsg != null)
			{
				AuthenticationMessage authMsg = new AuthenticationMessage();

				AuthenticationCredentialsEventArgs args = new AuthenticationCredentialsEventArgs();
				GetAuthenticationCredentials?.Invoke(this, args);

				authMsg.UserID = args.UserID;
				authMsg.AuthenticationToken = args.Token;
				authMsg.AuthenticationSourceToken = args.TokenSource;

				SendMessage(LastAuthenticationMessage);
				return;
			}

			AuthenticationResponceMessagecs authResponce = msg as AuthenticationResponceMessagecs;
			if (authResponce != null)
			{

			}
		}
	}
}
