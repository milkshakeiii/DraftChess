using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public delegate void TileClickedAction(int tileX, int tileZ);
	public static event TileClickedAction TileClicked;
		
	private Piece occupant;
	private int x;
	private int z;

	// Use this for initialization
	void Start () {
	
	}

	public static void ClickTile (int clickX, int clickZ)
	{
		TileClicked (clickX, clickZ);
	}

	public void SetOccupant(Piece piece)
	{
		occupant = piece;
	}

	public void RemoveOccupant()
	{
		occupant = null;
	}

	public Piece GetOccupant()
	{
		return occupant;
	}


	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown()
	{
		ClickTile (x, z);
	}

	public void SetX(int newX)
	{
		x = newX;
	}

	public void SetZ(int newZ)
	{
		z = newZ;
	}

	public int GetX()
	{
		return x;
	}

	public int GetZ()
	{
		return z;
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		int serialX = 0;
		int serialZ = 0;
		if (stream.isWriting) {
			serialX = GetX ();
			serialZ = GetZ ();
			stream.Serialize(ref serialX);
			stream.Serialize(ref serialZ);
		} else {
			stream.Serialize(ref serialX);
			stream.Serialize(ref serialZ);
			SetX(serialX);
			SetZ(serialZ);
		}
	}

}
