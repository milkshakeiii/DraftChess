using UnityEngine;
using System.Collections;

public class ControlPoint : BoardObserver {

	private int x;
	private int z;
	private int size;

	public void SetX(int newX)
	{
		x = newX;
	}
	
	public void SetZ(int newZ)
	{
		z = newZ;
	}

	public void SetSize(int newSize)
	{
		size = newSize;
	}
	
	public int GetX()
	{
		return x;
	}
	
	public int GetZ()
	{
		return z;
	}

	public override void BoardChangedNotification()
	{
		//print ("I was notified of a board change");
	}

	void OnEnable ()
	{
		NetworkManager.NextTurn += OnNextTurn;
	}
	
	void OnDisable()
	{
		NetworkManager.NextTurn -= OnNextTurn;
	}

	public void OnNextTurn()
	{
		int numberOfPiecesInRange = 0;
		//print ("I handled a next turn event");
		for (int i = -size; i <= size; i++)
		{
			for (int j = -size; j <= size; j++)
			{
				if (ScoringPieceExistsAt(x + i, j + z))
				{
					numberOfPiecesInRange++;
				}
			}
		}
		//print ("I found " + numberOfPiecesInRange + " of my pieces near the control point");
		Score.currentScore.ScoreIncreasedEvent (numberOfPiecesInRange);
	}

	private bool ScoringPieceExistsAt(int x, int z)
	{
		if (!Board.CurrentBoard.SquareOccupied(x, z))
			return false;
		Piece pieceThere = Board.CurrentBoard.GetPieceAt (x, z);
		return pieceThere.IsMine ();
	}
}
