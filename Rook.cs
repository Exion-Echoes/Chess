using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    public bool moved;
    public override List<Tile> PossibleMoves() //Any allowed movement must be part of this list
    {
        List<Tile> moves = new List<Tile>();

        for (int x = pos.x - 1; x >= 0; x--) //Left
        {
            Tile tile = board.TileAt(new Vector2Int(x, pos.y));
            if (tile != null)
            {
                if (IsAnAlly(tile))
                    break;

                moves.Add(tile);
                if (IsAnEnemy(tile))
                    break;
            }
        }
        for (int y = pos.y - 1; y >= 0; y--) //Down
        {
            Tile tile = board.TileAt(new Vector2Int(pos.x, y));
            if (tile != null)
            {
                if (IsAnAlly(tile))
                    break;

                moves.Add(tile);
                if (IsAnEnemy(tile))
                    break;
            }
        }
        for (int x = pos.x + 1; x <= 7; x++) //Right
        {
            Tile tile = board.TileAt(new Vector2Int(x, pos.y));
            if (tile != null)
            {
                if (IsAnAlly(tile))
                    break;

                moves.Add(tile);
                if (IsAnEnemy(tile))
                    break;
            }
        }
        for (int y = pos.y + 1; y <= 7; y++) //Up
        {
            Tile tile = board.TileAt(new Vector2Int(pos.x, y));
            if (tile != null)
            {
                if (IsAnAlly(tile))
                    break;

                moves.Add(tile);
                if (IsAnEnemy(tile))
                    break;
            }
        }
        return moves;
    }

    public void CheckIfMovedAndIfCastled(Piece p, Tile s, Tile e)
    {
        if (p == this)
        {
            moved = true;
            board.moveNotification -= CheckIfMovedAndIfCastled;
        }
        else if (isWhite ? board.wKTile == s : board.bKTile == s && Mathf.Abs(e.pos.x - s.pos.x) == 2) //Check if king performed a castling move
        {
            //Must only castle one of the rooks, but both must have their observer function taken out of the notification delegate
            //s and e are start and end tiles corresponding to king's movement

            if (e.pos.x - s.pos.x == -2 && pos.x == 0) //Queen's rook
            {
                Tile rookTile = board.state[0 + e.pos.y * 8];
                Tile nextToKing = board.state[3 + e.pos.y * 8];
                nextToKing.piece = this;
                pos = nextToKing.pos;
                transform.position = board.UnityUnits(nextToKing.pos);
                rookTile.piece = null;
            }
            else if (e.pos.x - s.pos.x == 2 && pos.x == 7) //King's rook
            {
                Tile rookTile = board.state[7 + e.pos.y * 8];
                Tile nextToKing = board.state[5 + e.pos.y * 8];
                nextToKing.piece = this;
                pos = nextToKing.pos;
                transform.position = board.UnityUnits(nextToKing.pos);
                rookTile.piece = null;
            }

            board.moveNotification -= CheckIfMovedAndIfCastled;
        }
    }
}