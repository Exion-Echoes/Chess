using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    [HideInInspector]
    public bool onStartingLine; //Used to help turn on en passant flag
    public Pawn enPassantPawn;

    public override void UpdateAllowedDestinations()
    {
        allowedDestinations.Clear();
        positionsDefended.Clear();

        //Don't have check OOB destinations as pawns move in a straight line and attack diagonally within the board

        Vector2Int firstPos = isWhite ? boardCoords + Vector2Int.up : boardCoords + Vector2Int.down;
        Vector2Int secondPos = boardCoords + (isWhite ? Vector2Int.up + Vector2Int.up : Vector2Int.down + Vector2Int.down);
        if (!IsOwnKingChecked(firstPos) && !IsThereAnAlly(firstPos) && !IsThereAnEnemy(firstPos))
            allowedDestinations.Add(firstPos);
        if (!IsOwnKingChecked(secondPos) && (isWhite ? boardCoords.y == 1 : boardCoords.y == 6) && !IsThereAnAlly(secondPos) && !IsThereAnEnemy(secondPos))
            allowedDestinations.Add(secondPos);

        Vector2Int firstAttackPos = isWhite ? boardCoords + Vector2Int.up + Vector2Int.right : boardCoords + Vector2Int.down + Vector2Int.right;
        Vector2Int secondAttackPos = isWhite ? boardCoords + Vector2Int.up - Vector2Int.right : boardCoords + Vector2Int.down - Vector2Int.right;
        Vector2Int firstEnPassantPos = firstAttackPos + (isWhite ? Vector2Int.down : Vector2Int.up);
        Vector2Int secondEnPassantPos = secondAttackPos + (isWhite ? Vector2Int.down : Vector2Int.up);
        //These have to be (uniquely) checked to see whether an enPassantPawn exists, in addition to checking for diagonal targets
        if (!IsOwnKingChecked(firstAttackPos) && !IsThereAnAlly(firstAttackPos) && (IsThereAnEnemy(firstAttackPos) || (enPassantPawn != null && IsThereAnEnemy(firstEnPassantPos) && board.GetBoardPiece(firstEnPassantPos) == enPassantPawn)))
            allowedDestinations.Add(firstAttackPos);
        else if (IsThereAnAlly(firstAttackPos))
            positionsDefended.Add(firstAttackPos);
        if (!IsOwnKingChecked(secondAttackPos) && !IsThereAnAlly(secondAttackPos) && (IsThereAnEnemy(secondAttackPos) || (enPassantPawn != null && IsThereAnEnemy(secondEnPassantPos) && board.GetBoardPiece(secondEnPassantPos) == enPassantPawn)))
            allowedDestinations.Add(secondAttackPos);
        else if (IsThereAnAlly(secondAttackPos))
            positionsDefended.Add(secondAttackPos);
    }

    public override void InitialAllowedDestinations()
    {
        allowedDestinations.Add(boardCoords + (isWhite ? Vector2Int.up : Vector2Int.down));
        allowedDestinations.Add(boardCoords + (isWhite ? Vector2Int.up + Vector2Int.up : Vector2Int.down + Vector2Int.down));
    }

    public override void DeterminePossibleActions()
    {
        //Pawn rules:
        //Moves one or two square forward when at its starting position, otherwise only moves one square
        //Eats pieces diagonally and up
        //Can eat pieces en-passant
        //Can turn into another piece when it reaches the end

        allowedDestinations.Clear();
        onStartingLine = false;

        //**Can be made cleaner with a Vector2Int being up or down depending on isWhite = true or isWhite = false (might need isWhite checks, so i dunno anymore if it's cleaner)

        //At the starting line, pawns can move one or two squares forward
        if (isWhite)
        {
            if (boardCoords.y == 1)
            {
                onStartingLine = true;
                //Check if there is a path ahead of the pawn
                Vector2Int oneUp = boardCoords + Vector2Int.up;
                Vector2Int twoUp = boardCoords + Vector2Int.up + Vector2Int.up;
                if (CheckForAnEnemyPiece(oneUp) == null && CheckForAFriendlyPiece(oneUp) == null && !WillMovingPiecePutOwnKingInCheck(boardCoords, oneUp))
                    allowedDestinations.Add(oneUp);
                if (CheckForAnEnemyPiece(twoUp) == null && CheckForAFriendlyPiece(twoUp) == null && CheckForAnEnemyPiece(oneUp) == null && CheckForAFriendlyPiece(oneUp) == null && !WillMovingPiecePutOwnKingInCheck(boardCoords, twoUp))
                    allowedDestinations.Add(twoUp);
            }
            else if (boardCoords.y >= 2 && boardCoords.y <= 6)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneUp = boardCoords + Vector2Int.up;
                if (CheckForAnEnemyPiece(oneUp) == null && CheckForAFriendlyPiece(oneUp) == null && !WillMovingPiecePutOwnKingInCheck(boardCoords, oneUp))
                    allowedDestinations.Add(oneUp);
            }

            //Check if there are pieces that may be eaten
            Vector2Int leftDiagonalBoardCoords = boardCoords + Vector2Int.left + Vector2Int.up;
            Vector2Int rightDiagonalBoardCoords = boardCoords + Vector2Int.right + Vector2Int.up;

            if (CheckForAnEnemyPiece(leftDiagonalBoardCoords) != null && !WillMovingPiecePutOwnKingInCheck(boardCoords, leftDiagonalBoardCoords))
                allowedDestinations.Add(leftDiagonalBoardCoords);
            if (CheckForAnEnemyPiece(rightDiagonalBoardCoords) != null && !WillMovingPiecePutOwnKingInCheck(boardCoords, rightDiagonalBoardCoords))
                allowedDestinations.Add(rightDiagonalBoardCoords);

            //Check for en-passant pawns
            if(enPassantPawn != null && !WillMovingPiecePutOwnKingInCheck(boardCoords, enPassantPawn.boardCoords + Vector2Int.up))
                allowedDestinations.Add(enPassantPawn.boardCoords + Vector2Int.up);
        }

        else if (!isWhite)
        {
            if (boardCoords.y == 6)
            {
                onStartingLine = true;
                //Check if there is a path ahead of the pawn
                Vector2Int oneDown = boardCoords + Vector2Int.down;
                Vector2Int twoDown = boardCoords + Vector2Int.down + Vector2Int.down;
                if (CheckForAnEnemyPiece(oneDown) == null && CheckForAFriendlyPiece(oneDown) == null && !WillMovingPiecePutOwnKingInCheck(boardCoords, oneDown))
                    allowedDestinations.Add(oneDown);
                if (CheckForAnEnemyPiece(twoDown) == null && CheckForAFriendlyPiece(twoDown) == null && CheckForAnEnemyPiece(oneDown) == null && CheckForAFriendlyPiece(oneDown) == null && !WillMovingPiecePutOwnKingInCheck(boardCoords, twoDown))
                    allowedDestinations.Add(twoDown);
            }
            else if (boardCoords.y >= 1 && boardCoords.y <= 5)
            {
                //Check if there is a path ahead of the pawn
                Vector2Int oneDown = boardCoords + Vector2Int.down;
                if (CheckForAnEnemyPiece(oneDown) == null && CheckForAFriendlyPiece(oneDown) == null && !WillMovingPiecePutOwnKingInCheck(boardCoords, oneDown))
                    allowedDestinations.Add(oneDown);
            }

            //Check if there are pieces that may be eaten
            Vector2Int leftDiagonalBoardCoords = boardCoords + Vector2Int.left + Vector2Int.down;
            Vector2Int rightDiagonalBoardCoords = boardCoords + Vector2Int.right + Vector2Int.down;

            if (CheckForAnEnemyPiece(leftDiagonalBoardCoords) != null && !WillMovingPiecePutOwnKingInCheck(boardCoords, leftDiagonalBoardCoords))
                allowedDestinations.Add(leftDiagonalBoardCoords);
            if (CheckForAnEnemyPiece(rightDiagonalBoardCoords) != null && !WillMovingPiecePutOwnKingInCheck(boardCoords, rightDiagonalBoardCoords))
                allowedDestinations.Add(rightDiagonalBoardCoords);

            //Check for en-passant pawns
            if (enPassantPawn != null && !WillMovingPiecePutOwnKingInCheck(boardCoords, enPassantPawn.boardCoords + Vector2Int.down))
                allowedDestinations.Add(enPassantPawn.boardCoords + Vector2Int.down);
        }
    }

    public void EnPassantFlags(Vector2Int dropPos)
    {
        for(int i = 0; i < 2; i++)
        {
            //Check left and right of this pawn for enemy pawns that may need their en passant flag turned on
            Piece piece = board.PieceOnBoard(new Vector2Int(dropPos.x - 1 + 2 * i, dropPos.y));
            if (piece != null && piece.isAPawn != null && piece.isWhite != isWhite)
            {
                piece.isAPawn.enPassantPawn = this;
                board.pawnsThatMayEatEnPassant.Add(piece.isAPawn);
            }
        }
    }

    public void BeginPawnPromotion()
    {
        board.promotedPawn = this;
        board.ongoingPawnPromotion = true;

        //Obtain precise position of board square in unity units
        Vector3 boardPos = board.UnityBoardCoordinates(board.BoardCoordinates(transform.position));

        for (int i = 0; i < 4; i++)
            board.promotionPieces[isWhite ? i : i + 4].transform.position = new Vector3(boardPos.x - 45 + 30 * i, boardPos.y, -2f);

        board.promotionBackground.transform.position = new Vector3(boardPos.x, boardPos.y, -1.5f);
    }
}
