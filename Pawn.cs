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
        if (isWhite)
        {
            if (boardCoords.y == 1)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneUp = boardCoords + Vector2Int.up;
                Vector2Int twoUp = boardCoords + Vector2Int.up + Vector2Int.up;
                if (CheckForAnEnemyPiece(oneUp) == null && !WillMovingPiecePutKingInCheck(boardCoords, oneUp))
                    allowedDestinations.Add(oneUp);
                if (CheckForAnEnemyPiece(twoUp) == null && !WillMovingPiecePutKingInCheck(boardCoords, twoUp))
                    allowedDestinations.Add(twoUp);
            }
            else if (boardCoords.y >= 2 && boardCoords.y <= 6)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneUp = boardCoords + Vector2Int.up;
                if (CheckForAnEnemyPiece(oneUp) == null && !WillMovingPiecePutKingInCheck(boardCoords, oneUp))
                    allowedDestinations.Add(oneUp);
            }

            //Check if there are pieces that may be eaten
            Vector2Int leftDiagonalBoardCoords = boardCoords + Vector2Int.left + Vector2Int.up;
            Vector2Int rightDiagonalBoardCoords = boardCoords + Vector2Int.right + Vector2Int.up;

            if (CheckForAnEnemyPiece(leftDiagonalBoardCoords) != null && !WillMovingPiecePutKingInCheck(boardCoords, leftDiagonalBoardCoords))
                allowedDestinations.Add(leftDiagonalBoardCoords);
            if (CheckForAnEnemyPiece(rightDiagonalBoardCoords) != null && !WillMovingPiecePutKingInCheck(boardCoords, rightDiagonalBoardCoords))
                allowedDestinations.Add(rightDiagonalBoardCoords);

            //Check for en-passant pawns
            if (enPassantPawn != null && !WillMovingPiecePutKingInCheck(boardCoords, enPassantPawn.boardCoords + Vector2Int.up))
                allowedDestinations.Add(enPassantPawn.boardCoords + Vector2Int.up);
        }

        else if (!isWhite)
        {
            if (boardCoords.y == 6)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneDown = boardCoords + Vector2Int.down;
                Vector2Int twoDown = boardCoords + Vector2Int.down + Vector2Int.down;
                if (CheckForAnEnemyPiece(oneDown) == null && !WillMovingPiecePutKingInCheck(boardCoords, oneDown))
                    allowedDestinations.Add(oneDown);
                if (CheckForAnEnemyPiece(twoDown) == null && !WillMovingPiecePutKingInCheck(boardCoords, twoDown))
                    allowedDestinations.Add(twoDown);
            }
            else if (boardCoords.y >= 2 && boardCoords.y <= 6)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneDown = boardCoords + Vector2Int.down;
                if (CheckForAnEnemyPiece(oneDown) == null && !WillMovingPiecePutKingInCheck(boardCoords, oneDown))
                    allowedDestinations.Add(oneDown);
            }

            //Check if there are pieces that may be eaten
            Vector2Int leftDiagonalBoardCoords = boardCoords + Vector2Int.left + Vector2Int.down;
            Vector2Int rightDiagonalBoardCoords = boardCoords + Vector2Int.right + Vector2Int.down;

            if (CheckForAnEnemyPiece(leftDiagonalBoardCoords) != null && !WillMovingPiecePutKingInCheck(boardCoords, leftDiagonalBoardCoords))
                allowedDestinations.Add(leftDiagonalBoardCoords);
            if (CheckForAnEnemyPiece(rightDiagonalBoardCoords) != null && !WillMovingPiecePutKingInCheck(boardCoords, rightDiagonalBoardCoords))
                allowedDestinations.Add(rightDiagonalBoardCoords);

            //Check for en-passant pawns
            if (enPassantPawn != null && !WillMovingPiecePutKingInCheck(boardCoords, enPassantPawn.boardCoords + Vector2Int.down))
                allowedDestinations.Add(enPassantPawn.boardCoords + Vector2Int.down);
        }

        //May eat diagonally left or right

        //May eat pawn en-passant if the condition's right

        //May be turned into a rook, knight, bishop or queen if it reaches the end line

        //Check if there is a piece blocking the allowed destinations

        //Check if moving the pawn will put its King in check
    }

    public void CheckIfEnPassantAlertNeedsToBeSent(Pawn pawn, Vector2Int testBoardCoords)
    {
        bool whiteEnPassantTrigger = pawn.isWhite && testBoardCoords.y - 2 == pawn.boardCoords.y;
        bool blackEnPassantTrigger = !pawn.isWhite && testBoardCoords.y + 2 == pawn.boardCoords.y;
        if (whiteEnPassantTrigger || blackEnPassantTrigger)
        {
            if (pawn.CheckForAnEnemyPiece(testBoardCoords + Vector2Int.right) != null)
            {
                if (board.boardArray[(testBoardCoords + Vector2Int.right).x, (testBoardCoords + Vector2Int.right).y].GetComponent<Pawn>() != null)
                {
                    board.boardArray[(testBoardCoords + Vector2Int.right).x, (testBoardCoords + Vector2Int.right).y].GetComponent<Pawn>().enPassantPawn = pawn;
                    board.pawnThatMayEatEnPassant = board.boardArray[(testBoardCoords + Vector2Int.right).x, (testBoardCoords + Vector2Int.right).y].GetComponent<Pawn>();
                }
            }
            else if (pawn.CheckForAnEnemyPiece(testBoardCoords + Vector2Int.left) != null)
            {
                if (board.boardArray[(testBoardCoords + Vector2Int.left).x, (testBoardCoords + Vector2Int.left).y].GetComponent<Pawn>() != null)
                {
                    board.boardArray[(testBoardCoords + Vector2Int.left).x, (testBoardCoords + Vector2Int.left).y].GetComponent<Pawn>().enPassantPawn = pawn;
                    board.pawnThatMayEatEnPassant = board.boardArray[(testBoardCoords + Vector2Int.left).x, (testBoardCoords + Vector2Int.left).y].GetComponent<Pawn>();
                }
            }
        }
    }

    public override void CheckIfThisPieceChecksOpposingKing()
    {
        Vector2Int[] testBoardCoords = new Vector2Int[2];

        if (isWhite)
        {
            testBoardCoords[0] = boardCoords + new Vector2Int(-1, 1);
            testBoardCoords[1] = boardCoords + new Vector2Int(1, 1);
        }
        else
        {
            testBoardCoords[0] = boardCoords + new Vector2Int(-1, -1);
            testBoardCoords[1] = boardCoords + new Vector2Int(1, -1);
        }

        for (int i = 0; i < 2; i++)
        {
            if (testBoardCoords[i].x >= 0 && testBoardCoords[i].x <= 7 && testBoardCoords[i].y >= 0 && testBoardCoords[i].y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (board.boardArray[testBoardCoords[i].x, testBoardCoords[i].y] == opposingKing)
                {
                    opposingKing.isChecked = true;
                    break;
                }
            }
        }
    }
}
