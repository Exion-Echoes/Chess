using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public Pawn enPassantPawn; //This has to be reset as soon as a piece moves, after being first defined

    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        bool moveAllowed = InTheList(PossibleMoves(), endTile);
        if (moveAllowed)
        {
            if (!MovingChecksOwnKing(startTile, endTile))
                return true;
        }

        //
        //WANT ENPASSANT LOGIC HERE IF POSSIBLE
        //It's noteworthy that if CanMove is true at any time, the move happens
        //
        //
        //
        //


        return false;
    }

    public List<Tile> Attacks()
    {
        return new List<Tile> { board.TileAt(pos + new Vector2Int(1, (isWhite ? 1 : -1))), board.TileAt(pos + new Vector2Int(-1, (isWhite ? 1 : -1))) };
    }

    public override List<Tile> PossibleMoves() //Any allowed movement must be part of this list
    {
        List<Tile> moves = new List<Tile>();

        int ahead = (isWhite ? 1 : -1); //General direction ahead of pawns

        Tile[] possibleMoves = { board.TileAt(new Vector2Int(pos.x, pos.y + ahead)), board.TileAt(new Vector2Int(pos.x, pos.y + ahead + ahead)),
            board.TileAt(new Vector2Int(pos.x - 1, pos.y + ahead)), board.TileAt(new Vector2Int(pos.x + 1, pos.y + ahead)) 
        };

        if(!IsAnAlly(possibleMoves[0]) && !IsAnEnemy(possibleMoves[0]))
            moves.Add(possibleMoves[0]);
        if (((pos.y == 1 && isWhite) || (pos.y == 6 && !isWhite)) && !IsAnAlly(possibleMoves[1]) && !IsAnEnemy(possibleMoves[1]))
            moves.Add(possibleMoves[1]);

        for (int i = 2; i <= 3; i++) //Attacks
        {
            //If an enemy is present on the diagonal, or an enpassant pawn is ready, the movement is allowed
            if (IsAnEnemy(possibleMoves[i]) || (enPassantPawn != null && enPassantPawn.pos.x == possibleMoves[i].pos.x))
                moves.Add(possibleMoves[i]);
        }

        return moves;
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
