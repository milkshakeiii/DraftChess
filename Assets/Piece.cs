using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour {

	public delegate void PieceMovedAction(Piece piece, Move move);
	public static event PieceMovedAction PieceMoved;

	public float Spacing = 1f;

	private int owningPlayerNumber;
	private bool selected = false;
	private int positionX = 0;
	private int positionZ = 0;
	private MovementStrategy myMovementStrategy;
	private bool everMoved = false;
	private ArrayList moveHistory = new ArrayList();

	private ArrayList availableMoveMarkers = new ArrayList();

	// Use this for initialization
	void Start ()
	{

	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) 
	{
		int serialX = 0 ;
		int serialZ = 0 ;
		if (stream.isWriting) 
		{
			serialX = GetX ();
			serialZ = GetZ ();
			stream.Serialize(ref serialX);
			stream.Serialize(ref serialZ);
		} 
		else 
		{
			stream.Serialize(ref serialX);
			stream.Serialize(ref serialZ);
			if (positionX != serialX || positionZ != serialZ)
			{
				Move(serialX, serialZ);
			}
		}
	}

	void OnEnable ()
	{
		Tile.TileClicked += CheckMoveClick;
	}

	void OnDisable()
	{
		Tile.TileClicked -= CheckMoveClick;
	}

	public void SetOwner(int newOwningPlayerNumber)
	{
		owningPlayerNumber = newOwningPlayerNumber;
	}

	public int GetOwner()
	{
		return owningPlayerNumber;
	}

	public int GetX()
	{
		return positionX;
	}

	public int GetZ()
	{
		return positionZ;
	}

	public void SetX(int newX)
	{
		positionX = newX;
	}

	public void SetZ(int newZ)
	{
		positionZ = newZ;
	}

	public void SetNeverMoved()
	{
		everMoved = false;
	}

	public void PlayerTurnMove(int x, int z)
	{
		NetworkManager.CurrentManager.PassMyTurn ();
		Move (x, z);
	}

	public void Move (int x, int z)
	{
		everMoved = true;
		unselect ();
		Move thisMove = new Move (positionX, positionZ, x, z);
		moveHistory.Add(thisMove);
		setPosition(x, z);
		PieceMoved (this, thisMove);
	}

	private void setPosition (int x, int z)
	{
		positionX = x;
		positionZ = z;
		this.transform.position = new Vector3 (Spacing * x, this.transform.position.y, Spacing * z);
	}

	public void MoveUp()
	{
		Move (positionX, positionZ + 1);
	}

	public void MoveDown()
	{
		Move (positionX, positionZ - 1);
	}

	public void MoveLeft()
	{
		Move (positionX - 1, positionZ);
	}

	public void MoveRight()
	{
		Move (positionX + 1, positionZ);
	}

	// Update is called once per frame
	void Update () {
		if (IsMine ())
		{
			if (selected & Input.GetKeyUp (KeyCode.LeftArrow))
				MoveLeft();
			if (selected & Input.GetKeyUp (KeyCode.RightArrow))
				MoveRight();
			if (selected & Input.GetKeyUp (KeyCode.UpArrow))
				MoveUp();
			if (selected & Input.GetKeyUp (KeyCode.DownArrow))
				MoveDown();
			if (selected && Input.GetMouseButtonDown(0) && mouseOutsideOfMe())
				unselect();
		}
	}

	private bool mouseOutsideOfMe()
	{
		RaycastHit hit = new RaycastHit ();
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast(ray, out hit))
		{
			if (hit.collider.gameObject == this.gameObject)
			{
				return false;
			}
		}

		return true;

	}

	void OnMouseDown()
	{
		Tile.ClickTile (positionX, positionZ);
		select ();
	}

	void OnMouseUp()
	{

	}

	private void select ()
	{
		if (!selected) paintMovementSquares();
		selected = true;
	}

	private void unselect()
	{
		selected = false;
		unpaintMovementSquares ();
	}

	private void paintMovementSquares()
	{
		ArrayList availableMoves = myMovementStrategy.AvailableMoves (this);
		print (availableMoves.Count);
		for (int i = 0; i < availableMoves.Count; i++)
		{
			GameObject newHighlight = Instantiate(Resources.Load("highlight")) as GameObject;
			Move moveToPaint = (Move)availableMoves[i];
			newHighlight.transform.position = new Vector3(moveToPaint.ToX * Spacing, 0.2f, moveToPaint.ToZ * Spacing);
			availableMoveMarkers.Add(newHighlight);
		}
	}

	private void unpaintMovementSquares()
	{
		while (availableMoveMarkers.Count != 0)
		{
			Destroy(availableMoveMarkers[0] as GameObject);
			availableMoveMarkers.RemoveAt(0);
		}
	}
	

	public void SetMovementStrategy(MovementStrategy newMovementStrategy)
	{
		myMovementStrategy = newMovementStrategy;
	}

	void CheckMoveClick(int tileX, int tileZ)
	{
		Move potentialMove = new Move (positionX, positionZ, tileX, tileZ);

		if (!NetworkManager.CurrentManager.IsMyTurn())
			Debug.Log("It's not your turn.");

		if (selected && 
		    myMovementStrategy.MoveAvailable(this, potentialMove) && 
		    NetworkManager.CurrentManager.IsMyTurn())
				PlayerTurnMove(tileX, tileZ);

	}

	public bool HasEverMoved()
	{
		return everMoved;
	}

	public void WasCaptured()
	{
		Board.CurrentBoard.RemovePieceAt(positionX, positionZ);
		Network.Destroy (gameObject);
	}

	public bool IsNorthFacing()
	{
		return GetOwner () % 2 == 1;
	}

	public bool IsMine()
	{
		return NetworkManager.CurrentManager.MyPlayerNumber() == GetOwner();
	}
}
