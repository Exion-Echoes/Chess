using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
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
}