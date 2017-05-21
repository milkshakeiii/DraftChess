using UnityEngine;
using System.Collections;

public class ControlPoint : MonoBehaviour
{
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

	void OnEnable ()
	{
		Piece.PieceMoved += OnPieceMoved;
	}
	
	void OnDisable()
	{
        Piece.PieceMoved -= OnPieceMoved;
    }

    //when a piece moves, award points for that turn
	private void OnPieceMoved(Move move)
	{
		int numberOfPiecesInRange = 0;
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
		Score.currentScore.ScoreIncreasedEvent (numberOfPiecesInRange);
	}

	private bool ScoringPieceExistsAt(int x, int z)
	{
		return Board.CurrentBoard.SquareOccupied(x, z) &&
               Board.CurrentBoard.GetPieceAt(x, z).IsMine ();
	}
}
