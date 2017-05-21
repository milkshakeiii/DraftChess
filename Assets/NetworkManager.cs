using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    //each client holds one network manager (a singleton) which facilitates network communication
	public static NetworkManager CurrentManager;

	private const string typeName = "DraftChessTestGameName";
	private const string gameName = "DraftChessTestRoomName";
	private HostData[] hostList;

	private int numberOfPlayers = 0;
	private int myPlayerNumber = 0;
	private int currentlyActivePlayerNumber = 0;
	private int maxPlayers = 2;
	private NetworkPlayer[] players;
	
	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);
	}


	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList();
	}

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}

    //in the current architecture, a dedicated server tracks players as they join
	[RPC]
	public void NewPlayerJoined(NetworkMessageInfo info)
	{
		Debug.Log("New Player Joined");
		numberOfPlayers++;
		players [numberOfPlayers - 1] = info.sender;

		if (numberOfPlayers == maxPlayers)
		{
			BeginGame();
		}

	}

	private void BeginGame()
	{
		SetActivePlayer (1);
		for (int i = 0; i < maxPlayers; i++)
			GetComponent<NetworkView>().RPC ("SetUpPlayer", players[i], i+1);
	}

	[RPC]
	public void SetUpPlayer(int playerNumber)
	{
		myPlayerNumber = playerNumber;
		numberOfPlayers = maxPlayers;
		SetActivePlayer (1);
	}

	public bool IsMyTurn()
	{
		return (numberOfPlayers == maxPlayers && myPlayerNumber == currentlyActivePlayerNumber);
	}

    //have the server communicate your turn's completion
	public void MoveComplete(Move move)
	{
		IncrementLocalActivePlayer();
		Debug.Log ("My Turn Over.  Currently Active: " + currentlyActivePlayerNumber);
		GetComponent<NetworkView>().RPC ("PlayerMoveComplete", RPCMode.Server, move.FromX, move.FromZ, move.ToX, move.ToZ);
	}

	private void IncrementLocalActivePlayer()
	{
		currentlyActivePlayerNumber = (currentlyActivePlayerNumber) % numberOfPlayers + 1;
	}

	[RPC]
	public void SetActivePlayer(int newActivePlayerNumber)
	{
		currentlyActivePlayerNumber = newActivePlayerNumber;
		Debug.Log ("New active player number recieved.  Currently Active: " + currentlyActivePlayerNumber);
	}

	[RPC]
	public void PlayerMoveComplete(int moveFromX, int moveFromZ, int moveToX, int moveToZ)
	{
		IncrementLocalActivePlayer();
		Debug.Log ("Player Turn Over.  Currently Active: " + currentlyActivePlayerNumber);
		for (int i = 0; i < maxPlayers; i++)
		{
			GetComponent<NetworkView>().RPC ("SetActivePlayer", players[i], currentlyActivePlayerNumber);
            GetComponent<NetworkView>().RPC ("RelayPieceMovedEvent", players[i], moveFromX, moveFromZ, moveToX, moveToZ);
            //pieces are moved when the server relays the move to everyone (including the mover)
        }
	}

	[RPC]
	public void RelayPieceMovedEvent(int fromX, int fromZ, int toX, int toZ)
	{
        Debug.Log("Received another player's move");
        Piece.AnnouncePieceMoved(new Move(fromX, fromZ, toX, toZ));
	}

	void OnConnectedToServer()
	{
		Debug.Log("Joined");
		GetComponent<NetworkView>().RPC ("NewPlayerJoined", RPCMode.Server);
	}

	private void StartServer()
	{
		Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	void OnServerInitialized()
	{
		Debug.Log("Server Initializied");
	}

    //test buttons for hosting and joining
	void OnGUI()
	{
		if (!Network.isClient && !Network.isServer)
		{
			if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
				StartServer();
			
			if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
				RefreshHostList();
			
			if (hostList != null)
			{
				for (int i = 0; i < hostList.Length; i++)
				{
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
		}
	}

	void Start ()
    {
		players = new NetworkPlayer[maxPlayers];
		CurrentManager = this;
	}

	public int MyPlayerNumber()
	{
		return myPlayerNumber;
	}
}
