using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{
	public delegate void PieceMovedAction(Move move);
	public static event PieceMovedAction PieceMoved;
    //invoke this event when the player has successfully executed a legal move

	public float Spacing = 1f;
    public GameObject highlightPrefab; //used to show squares a piece can move to

	private int owningPlayerNumber;
	private bool selected = false;
	private int positionX = 0;
	private int positionZ = 0;
	private MovementStrategy myMovementStrategy; //strategy used to determine legal moves
	private bool everMoved = false; //whether or not this piece has moved once (relevant for pawns)
	private ArrayList moveHistory = new ArrayList();

	private ArrayList availableMoveMarkers = new ArrayList();

    public static void AnnouncePieceMoved(Move move)
    {
        PieceMoved(move);
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
        SetPosition(newX, positionZ);
	}

	public void SetZ(int newZ)
	{
        SetPosition(positionX, newZ);
	}

    //move is confirmed, send it to the network
	private void Move (int x, int z)
	{
		everMoved = true;
		unselect ();
		Move thisMove = new Move (positionX, positionZ, x, z);
		moveHistory.Add(thisMove);
        NetworkManager.CurrentManager.MoveComplete(thisMove);
	}

	public void SetPosition (int x, int z)
	{
		positionX = x;
		positionZ = z;
		this.transform.position = new Vector3 (Spacing * x, this.transform.position.y, Spacing * z);
	}

	void Update ()
    {
		if (selected && Input.GetMouseButtonDown(0) && mouseOutsideOfMe())
			unselect();
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
		for (int i = 0; i < availableMoves.Count; i++)
		{
			GameObject newHighlight = Instantiate(highlightPrefab);
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

    //when a player tries to make a move by clicking a tile, check its validity
	private void CheckMoveClick(int tileX, int tileZ)
	{
        Debug.Log(selected);
		Move potentialMove = new Move (positionX, positionZ, tileX, tileZ);

        if (selected && !NetworkManager.CurrentManager.IsMyTurn())
        {
            Debug.Log("It's not your turn.");
            return;
        }

        if (selected && !IsMine())
        {
            Debug.Log("That's not your piece.");
            Debug.Log(GetOwner());
            Debug.Log(NetworkManager.CurrentManager.MyPlayerNumber());
            return;
        }


		if (selected && myMovementStrategy.MoveAvailable(this, potentialMove))
				Move(tileX, tileZ);

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
