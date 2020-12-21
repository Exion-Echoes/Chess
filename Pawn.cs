using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override void Awake()
    {
        base.Awake();
        allowedDestinations = new Vector2Int[5]; //There are 5 things to consider with pawns
    }

    public override void Rule()
    {
        //Pawn rules:
        //Moves one or two square forward when at its starting position, otherwise only moves one square
        //Eats pieces diagonally and up
        //Can eat pieces en-passant
        //Can turn into another piece when it reaches the end

        allowedDestinations[0] = allowedDestinations[1] = posOnBoard + (isWhite ? Vector2Int.up : Vector2Int.down);

        //May move two squares if posOnBoard is on the starting line
        if (isWhite && posOnBoard.y == 1)
            allowedDestinations[1] = posOnBoard + new Vector2Int(0, 2);
        else if (!isWhite && posOnBoard.y == 6)
            allowedDestinations[1] = posOnBoard - new Vector2Int(0, 2);


        //May eat diagonally left or right

        //May eat pawn en-passant if the condition's right

        //May be turned into a rook, knight, bishop or queen if it reaches the end line

        //Check if there is a piece blocking the allowed destinations

        //Check if moving the pawn will put its King in check

        for (int i = 0; i < 2; i++)
        {
            Piece piece = board.boardArray[allowedDestinations[i].x, allowedDestinations[i].y];
            if (piece != null && this.isWhite != piece.isWhite)
                Debug.Log("At " + allowedDestinations[i] + ", " + piece + " is in the way");
            //            if (board.FindPieceAtBoardCoordinate(allowedDestinations[i]) != null)
            //                allowedDestinations[i] = posOnBoard;
        }


//        Debug.Log(posOnBoard + ", " + allowedDestinations[0] + ", " + allowedDestinations[1]);
  //      for (int i = 0; i < 5; i++)
//            Debug.Log(allowedDestinations[i]);
    }
}
