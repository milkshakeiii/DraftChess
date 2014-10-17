using UnityEngine;
using System.Collections;

public struct Move
{
	public int FromX;
	public int ToX;
	public int FromZ;
	public int ToZ;

	public Move(int newFromX, int newFromZ, int newToX, int newToZ)
	{
		FromX = newFromX;
		FromZ = newFromZ;
		ToX = newToX;
		ToZ = newToZ;
	}

	public bool Equals(Move otherMove)
	{
		return	(FromX == otherMove.FromX) && 
				(ToX == otherMove.ToX) && 
				(FromZ == otherMove.FromZ) && 
				(ToZ == otherMove.ToZ);
	}

}

public abstract class MovementStrategy  
{



	public MovementStrategy()
	{

	}

	public virtual bool MoveAvailable(Piece mover, Move move)
	{
		ArrayList availableMoves = AvailableMoves (mover);
		for (int i = 0; i < availableMoves.Count; i++)
		{
			if (availableMoves[i].Equals(move))
			{
				return true;
			}
		}
		return false;
	}

	public abstract ArrayList AvailableMoves (Piece mover);

	public void RemoveOutOfBounds(ref ArrayList moves)
	{
		foreach (Move move in moves)
		{
			if (move.ToX < 0 || move.ToZ < 0 || move.ToX >= Board.CurrentBoard.Width || move.ToZ >= Board.CurrentBoard.Height)
				moves.Remove(move);
		}
	}


}

public class PawnMovementStrategy : MovementStrategy
{

	public override ArrayList AvailableMoves(Piece mover)
	{
		int allowedRange;
		if (mover.HasEverMoved ())
			allowedRange = 1;
		else
			allowedRange = 2;

		ArrayList availableMoves = new ArrayList();

		int directionalNegater = 1;
		if (!mover.IsNorthFacing())
			directionalNegater = -1;

		for (int i = 0; i < allowedRange; i++)
		{
			Move potentialMove = new Move(mover.GetX(), mover.GetZ(), mover.GetX(), mover.GetZ() + directionalNegater * (i + 1));
			if (Board.CurrentBoard.SquareOccupied(potentialMove.ToX, potentialMove.ToZ) 
			    || !Board.CurrentBoard.InBounds(potentialMove.ToX, potentialMove.ToZ))
				break;
			availableMoves.Add(potentialMove);
		}

		//append captures if available
		int rightDiagonalX = mover.GetX () + 1;
		int leftDiagonalX = mover.GetX () - 1;
		int diagonalZ = mover.GetZ () + directionalNegater;

		Debug.Log ("mover is mine: " + mover.IsMine ().ToString());
		if ( CaptureAvailableAt(rightDiagonalX, diagonalZ, mover) )
		{
			availableMoves.Add(new Move(mover.GetX(), mover.GetZ(), rightDiagonalX, diagonalZ));
		}
		if ( CaptureAvailableAt(leftDiagonalX, diagonalZ, mover))
		{
			availableMoves.Add(new Move(mover.GetX(), mover.GetZ(), leftDiagonalX, diagonalZ));
		}

		return availableMoves;
	}

	private bool CaptureAvailableAt(int x, int z, Piece mover)
	{
		if (!(Board.CurrentBoard.InBounds(x, z) && Board.CurrentBoard.SquareOccupied(x, z)))
		{
			return false;
		}
		bool differentOwners = !Board.CurrentBoard.GetPieceAt(x, z).IsMine ();
		Debug.Log ("different owners: " + differentOwners.ToString ());
		return differentOwners;
	}

}
