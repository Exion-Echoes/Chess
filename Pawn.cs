using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public Pawn enPassantPawn;
    bool overrideEnPassantPawn;

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

        if (!IsAnAlly(possibleMoves[0]) && !IsAnEnemy(possibleMoves[0]))
            moves.Add(possibleMoves[0]);
        if (((pos.y == 1 && isWhite) || (pos.y == 6 && !isWhite)) && !IsAnAlly(possibleMoves[0]) && !IsAnEnemy(possibleMoves[0]) && !IsAnAlly(possibleMoves[1]) && !IsAnEnemy(possibleMoves[1]))
            moves.Add(possibleMoves[1]);

        for (int i = 2; i <= 3; i++) //Attacks
        {
            //If an enemy is present on the diagonal, or an enpassant pawn is ready, the movement is allowed
            if (possibleMoves[i] != null && (IsAnEnemy(possibleMoves[i]) || (enPassantPawn != null && enPassantPawn.pos + (isWhite ? Vector2Int.up : Vector2Int.down) == possibleMoves[i].pos)))//(board.enPassantTile != null && board.enPassantTile.piece != null && board.enPassantTile.piece.isWhite != isWhite && board.enPassantTile.pos.x == possibleMoves[i].pos.x && board.enPassantTile.pos.y == pos.y)))
                moves.Add(possibleMoves[i]);
        }

        if (this.pos == new Vector2Int(5, 4))
        {
  //          for (int i = 0; i < moves.Count; i++)
//                Debug.Log(i + ", " + moves[i].pos);
        }
        return moves;
    }

    public void EnPassantCheck(Piece p, Tile s, Tile e)
    {
        if(p == this && (s.pos.y == 1 || s.pos.y == 6))
        {
            board.moveNotification -= EnPassantCheck; //Unsub from the notifications as soon as the pawn moves once
            if (e.pos.y == s.pos.y + 2 * (int)Mathf.Sign(e.pos.y - s.pos.y)) //If double moved, work out the en passant logic
            {
                void SubscribeEnPassantPawn(Tile tile) //Identify neighbouring tiles to see if they're pawns and to turn on their en passant flag
                {
                    if (tile != null && tile.piece != null && tile.piece.isPawn != null && tile.piece.isWhite != isWhite)
                    {
                        tile.piece.isPawn.overrideEnPassantPawn = true; //Used to handle case where this subs twice to the notification delegate
                        tile.piece.isPawn.enPassantPawn = this;
                        board.moveNotification += tile.piece.isPawn.CanEatEnPassant;
                    }
                }
                SubscribeEnPassantPawn(board.TileAt(e.pos - Vector2Int.right));
                SubscribeEnPassantPawn(board.TileAt(e.pos + Vector2Int.right));
            }
        }
    }

    public void CanEatEnPassant(Piece p, Tile s, Tile e)
    {
        if(p.isWhite == !isWhite) //Guarantees that the en passant flag leaves on this color's next turn
        {
            board.moveNotification -= CanEatEnPassant; //Unsub even if the en passant movement doesn't occur
            if(!overrideEnPassantPawn) //Need to add this one-turn delay in case two pawns in a row activate the en passant functionality
                enPassantPawn = null;
            overrideEnPassantPawn = false;
        }
    }
}