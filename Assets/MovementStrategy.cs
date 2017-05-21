using UnityEngine;
using System.Collections;

//this struct represents a possible movement of a piece
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

    //simply searches through the list of moves- not fast
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

    //based on who is moving, movement strategies determine what moves are available
	public abstract ArrayList AvailableMoves (Piece mover);
}

public class PawnMovementStrategy : MovementStrategy
{

    //the logic of a pawn move in chess
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
			Move potentialMove = new Move(mover.GetX(), 
                                          mover.GetZ(),
                                          mover.GetX(),
                                          mover.GetZ() + directionalNegater * (i + 1));
			if (Board.CurrentBoard.SquareOccupied(potentialMove.ToX, potentialMove.ToZ) 
			    || !Board.CurrentBoard.InBounds(potentialMove.ToX, potentialMove.ToZ))
				break;
			availableMoves.Add(potentialMove);
		}

		//append captures if available
		int rightDiagonalX = mover.GetX () + 1;
		int leftDiagonalX = mover.GetX () - 1;
		int diagonalZ = mover.GetZ () + directionalNegater;

		if ( Board.CurrentBoard.CaptureAvailableAt(rightDiagonalX, diagonalZ, mover) )
		{
			availableMoves.Add(new Move(mover.GetX(), mover.GetZ(), rightDiagonalX, diagonalZ));
		}
		if (Board.CurrentBoard.CaptureAvailableAt(leftDiagonalX, diagonalZ, mover))
		{
			availableMoves.Add(new Move(mover.GetX(), mover.GetZ(), leftDiagonalX, diagonalZ));
		}

		return availableMoves;
	}

}

public class KnightMovementStrategy : MovementStrategy
{

    //just like a knight in western chess
    public override ArrayList AvailableMoves(Piece mover)
    {
        ArrayList availableMoves = new ArrayList();

        foreach (int smallerHopPart in new int[] { 1, -1 })
        {
            foreach (int biggerHopPart in new int[] { 2, -2 })
            {
                Move wideHop = new Move(mover.GetX(),
                                        mover.GetZ(),
                                        mover.GetX() + smallerHopPart,
                                        mover.GetZ() + biggerHopPart);
                Move tallHop = new Move(mover.GetX(),
                                        mover.GetZ(),
                                        mover.GetX() + biggerHopPart,
                                        mover.GetZ() + smallerHopPart);
                if (Board.CurrentBoard.InBounds(wideHop.ToX, wideHop.ToZ))
                    availableMoves.Add(wideHop);
                if (Board.CurrentBoard.InBounds(tallHop.ToX, tallHop.ToZ))
                    availableMoves.Add(tallHop);
            }
        }

        return availableMoves;
    }
}
