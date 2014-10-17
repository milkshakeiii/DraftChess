using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	public static NetworkManager CurrentManager;

	public delegate void NextTurnAction();
	public static event NextTurnAction NextTurn;

	private const string typeName = "JunGameName";
	private const string gameName = "JunRoomName";
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

	public void BeginGame()
	{
		SetActivePlayer (1);
		for (int i = 0; i < maxPlayers; i++)
			networkView.RPC ("setupPlayer", players[i], i+1);
	}

	[RPC]
	public void setupPlayer(int playerNumber)
	{
		Debug.Log("setup myself, player number " + playerNumber);
		PieceFactory.BuildPlayerArmy (playerNumber);
		myPlayerNumber = playerNumber;
		numberOfPlayers = maxPlayers;
		SetActivePlayer (1);
	}

	public bool IsMyTurn()
	{
		//print ("I'm number " + myPlayerNumber + ".  Active is " + currentlyActivePlayerNumber);
		return (myPlayerNumber == currentlyActivePlayerNumber);
	}

	public void PassMyTurn()
	{
		IncrementLocalActivePlayer();
		Debug.Log ("My Turn Over.  Currently Active: " + currentlyActivePlayerNumber);
		networkView.RPC ("PlayerTurnOver", RPCMode.Server);
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
	public void PlayerTurnOver()
	{
		IncrementLocalActivePlayer();
		Debug.Log ("Player Turn Over.  Currently Active: " + currentlyActivePlayerNumber);
		for (int i = 0; i < maxPlayers; i++)
		{
			networkView.RPC ("SetActivePlayer", players[i], currentlyActivePlayerNumber);
			networkView.RPC ("NextTurnEvent", players[i]);
		}
	}

	[RPC]
	public void NextTurnEvent()
	{
		NextTurn ();
	}

	void OnConnectedToServer()
	{
		Debug.Log("Joined");
		networkView.RPC ("NewPlayerJoined", RPCMode.Server);
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

	// Use this for initialization
	void Start () {
		players = new NetworkPlayer[maxPlayers];
		CurrentManager = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static bool PlayerIsNorthFacing(int playerNumber)
	{
		return (playerNumber % 2 == 1);
	}

	public NetworkPlayer Owner()
	{
		return players[myPlayerNumber - 1];
	}

	public NetworkPlayer PlayerNumbered(int playerNumber)
	{
		return players[playerNumber - 1];
	}

	public int PlayerNumberOf(NetworkPlayer player)
	{
		return System.Array.IndexOf<NetworkPlayer> (players, player);
	}

	public int MyPlayerNumber()
	{
		return myPlayerNumber;
	}
}
