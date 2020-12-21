using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    //    public delegate void enPassantPossible();
    //    public event Action<Pawn> enPassantPossible;
    public Pawn enPassantPawn;
//    public bool enPassantAllowedThisTurn = false;

    public override void Awake()
    {
        base.Awake();
//        allowedDestinations = new Vector2Int[5]; //There are 5 things to consider with pawns
    }

    public void Start()
    {
        //sub to en passant events of enemy pawns
    }

    public override void DeterminePossibleActions()
    {
        //Pawn rules:
        //Moves one or two square forward when at its starting position, otherwise only moves one square
        //Eats pieces diagonally and up
        //Can eat pieces en-passant
        //Can turn into another piece when it reaches the end

        allowedDestinations.Clear();

        //**Can be made cleaner with a Vector2Int being up or down depending on isWhite = true or isWhite = false (might need isWhite checks, so i dunno anymore if it's cleaner)
        //**Need event call when pawn reaches last row

        //At the starting line, pawns can move one or two squares forward
        if(isWhite)
        {
            if (boardCoords.y == 1)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneUp = boardCoords + Vector2Int.up;
                Vector2Int twoUp = boardCoords + Vector2Int.up + Vector2Int.up;
                if (CheckForAnEnemyPiece(oneUp) == null && !WillMovingPiecePutKingInCheck(oneUp))
                    allowedDestinations.Add(oneUp);
                if (CheckForAnEnemyPiece(twoUp) == null && !WillMovingPiecePutKingInCheck(twoUp))
                    allowedDestinations.Add(twoUp);
            }
            else if (boardCoords.y >= 2 && boardCoords.y <= 6)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneUp = boardCoords + Vector2Int.up;
                if (CheckForAnEnemyPiece(oneUp) == null && !WillMovingPiecePutKingInCheck(oneUp))
                    allowedDestinations.Add(oneUp);
            }

            //Check if there are pieces that may be eaten
            Vector2Int leftDiagonalBoardCoords = boardCoords + Vector2Int.left + Vector2Int.up;
            Vector2Int rightDiagonalBoardCoords = boardCoords + Vector2Int.right + Vector2Int.up;

            if (CheckForAnEnemyPiece(leftDiagonalBoardCoords) && !WillMovingPiecePutKingInCheck(leftDiagonalBoardCoords))
                allowedDestinations.Add(leftDiagonalBoardCoords);
            if (CheckForAnEnemyPiece(rightDiagonalBoardCoords) && !WillMovingPiecePutKingInCheck(rightDiagonalBoardCoords))
                allowedDestinations.Add(rightDiagonalBoardCoords);

            //Check for en-passant pawns
            if (enPassantPawn != null && !WillMovingPiecePutKingInCheck(enPassantPawn.boardCoords + Vector2Int.up))
                allowedDestinations.Add(enPassantPawn.boardCoords + Vector2Int.up);

        }

        else if (!isWhite)
        {
            if (boardCoords.y == 6)
            {
                allowedDestinations.Add(boardCoords + Vector2Int.down);
                allowedDestinations.Add(boardCoords + Vector2Int.down + Vector2Int.down);
            }
            else if (boardCoords.y >= 2 && boardCoords.y <= 6)
                allowedDestinations.Add(boardCoords + Vector2Int.down);
        }


        //allowedDestinations[0] = allowedDestinations[1] = boardCoords + (isWhite ? Vector2Int.up : Vector2Int.down);

        //May move two squares if posOnBoard is on the starting line
        //if (isWhite && boardCoords.y == 1)
        //      allowedDestinations[1] = boardCoords + new Vector2Int(0, 2);
        //    else if (!isWhite && boardCoords.y == 6)
        //          allowedDestinations[1] = boardCoords - new Vector2Int(0, 2);


        //May eat diagonally left or right

        //May eat pawn en-passant if the condition's right

        //May be turned into a rook, knight, bishop or queen if it reaches the end line

        //Check if there is a piece blocking the allowed destinations

        //Check if moving the pawn will put its King in check

        for (int i = 0; i < 2; i++)
        {
//            Piece piece = board.boardArray[allowedDestinations[i].x, allowedDestinations[i].y];
 //           if (piece != null && this.isWhite != piece.isWhite)
  //              Debug.Log("At " + allowedDestinations[i] + ", " + piece + " is in the way");
            //            if (board.FindPieceAtBoardCoordinate(allowedDestinations[i]) != null)
            //                allowedDestinations[i] = posOnBoard;
        }


//        Debug.Log(posOnBoard + ", " + allowedDestinations[0] + ", " + allowedDestinations[1]);
  //      for (int i = 0; i < 5; i++)
//            Debug.Log(allowedDestinations[i]);
    }

//    public void EnPassantPossible(Pawn pawn) //En passant eating is possible with this pawn and the argument pawn
//    {
        //
//    }
    
    public void AlertNearbyEnemyPawnsAboutEnPassant(Pawn pawn)
    {
        //This would be called when a successful double move is done
    }
}
