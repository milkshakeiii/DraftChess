using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
	public delegate void TileClickedAction(int tileX, int tileZ);
	public static event TileClickedAction TileClicked;
    //invoke this when a player clicks a tile so that pieces, etc. can respond
    //to the player's commands
		
	private Piece occupant; //this is the piece sitting on this tile on the board
	private int x;
	private int z;


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

	void OnMouseDown()
	{
        Debug.Log(x);
        Debug.Log(z);
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
}
