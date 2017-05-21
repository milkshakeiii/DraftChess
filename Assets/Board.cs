using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{
    //there is only one board in a scene, like a singleton
	public static Board CurrentBoard;

	public float Spacing = 1f; //space between tiles
	public int Width = 32;
	public int Height = 32;
	public GameObject TilePrefab;
	public GameObject PiecePrefab;
	public GameObject ControlPointPrefab;

    [Range(0, 1)]
    public float Holeyness = 0.25f; //percent chance to omit a tile, with editor slider

    private Tile[,] board; //the board is implemented as a 2D array of tiles

	
	void Start ()
    {
		SetupBoard();
		CurrentBoard = this;
	}

	void OnEnable ()
	{
        //the board subscribes to piecemoved events so that it can update its tiles
        Piece.PieceMoved += OnPieceMove; 
	}
	
	void OnDisable()
	{
		Piece.PieceMoved -= OnPieceMove;
	}

	private void OnPieceMove(Move move)
	{
        Debug.Log(move.FromX);
        Debug.Log(move.FromZ);
        Piece piece = GetPieceAt(move.FromX, move.FromZ);
        RemovePieceAt (move.FromX, move.FromZ);
        Debug.Log(piece);
        piece.SetPosition(move.ToX, move.ToZ);
        if (SquareOccupied(move.ToX, move.ToZ))
		{
			Piece capturedPiece = GetPieceAt(move.ToX, move.ToZ);
			capturedPiece.WasCaptured();
		}
		SetPieceAt (move.ToX, move.ToZ, piece);

	}

	private Vector3 PositionOf(int x, int z)
	{
		return board[x, z].gameObject.transform.position;
	}

    //build a board with a control point and some pieces to test with
	private void SetupBoard()
	{
		board = new Tile[Width, Height];
		GenerateHoleyBoard ();
		AddControlPoint (4, 4, 1);
        AddPiece(4, 1, new PawnMovementStrategy(), 1);
        AddPiece(4, 6, new PawnMovementStrategy(), 2);
        AddPiece(4, 0, new KnightMovementStrategy(), 1);
        AddPiece(4, 7, new KnightMovementStrategy(), 2);
    }

    private void AddPiece(int x, int z, MovementStrategy movementStrategy, int owner)
    {
        Piece newPiece = Instantiate(PiecePrefab).GetComponent<Piece>();
        newPiece.SetX(x);
        newPiece.SetZ(z);
        newPiece.SetMovementStrategy(movementStrategy);
        newPiece.SetOwner(owner);
        SetPieceAt(x, z, newPiece);
    }

	private void AddControlPoint(int x, int z, int size)
	{
		Vector3 position = PositionOf (x, z) + new Vector3 (0, 0.05f, 0);
		GameObject controlPoint = Instantiate (ControlPointPrefab, position, new Quaternion ()) as GameObject;
		controlPoint.transform.parent = gameObject.transform;
		ControlPoint controlPointScript = controlPoint.GetComponent<ControlPoint> ();
		controlPointScript.SetX (x);
		controlPointScript.SetZ (z);
		controlPointScript.SetSize(size);
	}

	private void GenerateHoleyBoard()
	{
		float bottom = 0;
		float left = 0;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				if (Random.value >= Holeyness)
				{
					Vector3 position = new Vector3(left + i * Spacing, 0, bottom + j * Spacing);
					GameObject newTile = Instantiate(TilePrefab, position, new Quaternion()) as GameObject;
					newTile.transform.parent = gameObject.transform;
					Tile tileScript = newTile.GetComponent<Tile>();
					tileScript.SetX(i);
					tileScript.SetZ(j);
					board[i, j] = tileScript;
				}
			}
		}
	}

	private void SetPieceAt(int x, int z, Piece piece)
	{
		board [x, z].SetOccupant (piece);
	}

	public void RemovePieceAt(int x, int z)
	{
		board [x, z].RemoveOccupant ();
	}

	public bool SquareOccupied(int x, int z)
	{
		return !InBounds(x, z) || GetPieceAt (x, z) != null;
	}

    public bool CaptureAvailableAt(int x, int z, Piece mover)
    {
        return InBounds(x, z) && SquareOccupied(x, z) && !GetPieceAt(x, z).IsMine();
    }

    
    public Piece GetPieceAt(int x, int z)
	{
		return board [x, z].GetOccupant();
	}

	public bool InBounds(int x, int z)
	{
		return (x >= 0 && z >= 0 && x < Width && z < Height);
	}
}
