using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        //
        return false;
    }


    public List<Tile> Attacks() //Used to separate PossibleMoves() and Attacks()
    {

        return null;
    }





    /*    [HideInInspector]
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
    */
}
