using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

	public static Board CurrentBoard;

	public float Spacing = 1f;
	[Range(0, 1)]
	public float Holeyness = 0.25f;
	public int Width = 32;
	public int Height = 32;
	public GameObject TilePrefab;
	public GameObject PiecePrefab;
	public GameObject ControlPointPrefab;

	private Tile[,] board;
	private ArrayList boardObservers = new ArrayList();

	public Tile[,] GetBoard()
	{
		return board;
	}

	public void SetBoard(Tile[,] newBoard)
	{
		board = newBoard;
	}

	public void AddObserver(BoardObserver newObserver)
	{
		boardObservers.Add (newObserver);
	}

	private void BoardChangedNotifyObservers()
	{
		//print ("notifying " + boardObservers.Count + " observers");
		foreach(BoardObserver observer in boardObservers)
		{
			observer.BoardChangedNotification();
		}
	}

	// Use this for initialization
	void Start () {
		SetupBoard();
		CurrentBoard = this;
	}

	void OnEnable ()
	{
		Piece.PieceMoved += OnPieceMove;
	}
	
	void OnDisable()
	{
		Piece.PieceMoved -= OnPieceMove;
	}

	void OnPieceMove(Piece piece, Move move)
	{
		RemovePieceAt (move.FromX, move.FromZ);
		if (SquareOccupied(move.ToX, move.ToZ))
		{
			Piece capturedPiece = GetPieceAt(move.ToX, move.ToZ);
			capturedPiece.WasCaptured();
		}
		SetPieceAt (move.ToX, move.ToZ, piece);

		BoardChangedNotifyObservers ();
	}

	public Vector3 PositionOf(int x, int z)
	{
		return board[x, z].gameObject.transform.position;
	}

	public Tile TileAtPosition(int x, int z)
	{
		return board [x, z];
	}

	public void SetupBoard()
	{
		board = new Tile[Width, Height];
		GenerateHoleyBoard ();
		AddControlPoint (4, 12, 1);
	}

	public void AddControlPoint(int x, int z, int size)
	{
		Vector3 position = PositionOf (x, z) + new Vector3 (0, 0.05f, 0);
		GameObject controlPoint = Instantiate (ControlPointPrefab, position, new Quaternion ()) as GameObject;
		controlPoint.transform.parent = gameObject.transform;
		ControlPoint controlPointScript = controlPoint.GetComponent<ControlPoint> ();
		controlPointScript.SetX (x);
		controlPointScript.SetZ (z);
		controlPointScript.SetSize(size);
	}

	void GenerateHoleyBoard()
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
		print ("finished generating");
		print (board [0, 0]);
	}

	public void SetPieceAt(int x, int z, Piece piece)
	{
		board [x, z].SetOccupant (piece);
	}

	public void RemovePieceAt(int x, int z)
	{
		board [x, z].RemoveOccupant ();
	}

	public bool SquareOccupied(int x, int z)
	{
		if (x < 0 || z < 0 || x > Width || z > Height)
			return true;
		return GetPieceAt (x, z) != null;
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
