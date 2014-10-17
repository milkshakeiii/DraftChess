using UnityEngine;
using System.Collections;

public static class PieceFactory
{

	public static void BuildPlayerArmy(int owningPlayerNumber)
	{
		for (int i = 0; i < 8; i++)
			AddPawnForPlayer (i, owningPlayerNumber * 8, owningPlayerNumber);
	}

	public static void AddPawnForPlayer(int startX, int startZ, int owningPlayerNumber)
	{
		GameObject pawnPiece = buildPieceFromPicture("pawn");
		Piece newPieceScript = pawnPiece.GetComponent<Piece> ();
		newPieceScript.SetMovementStrategy (new PawnMovementStrategy ());
		addPieceToPlayer (owningPlayerNumber, newPieceScript);
		newPieceScript.Move (startX, startZ);
		newPieceScript.SetNeverMoved ();
	}
	
	private static void addPieceToPlayer(int owningPlayerNumber, Piece piece)
	{
		piece.SetOwner (owningPlayerNumber);
	}

	private static GameObject buildPieceFromPicture(string path)
	{
		GameObject pawnPrefab = Resources.Load ("piece") as GameObject;
		GameObject pawnPiece = Network.Instantiate (pawnPrefab, new Vector3(0, 0.1f, 0)
		                                            , new Quaternion(), 0) as GameObject;
		Texture pawnTexture = Resources.Load <Texture> (path);
		pawnPiece.renderer.material.mainTexture = pawnTexture;
		return pawnPiece;
	}

}
