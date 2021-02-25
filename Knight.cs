using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    public override List<Tile> PossibleMoves()
    {
        List<Tile> moves = new List<Tile>();

        Tile[] possibleMoves = { //8 tiles around Knight
            board.TileAt(pos + new Vector2Int(1, 2)), board.TileAt(pos + new Vector2Int(2, 1)), board.TileAt(pos + new Vector2Int(2, -1)), board.TileAt(pos + new Vector2Int(1, -2)),
            board.TileAt(pos + new Vector2Int(-1, -2)), board.TileAt(pos + new Vector2Int(-2, -1)), board.TileAt(pos + new Vector2Int(-2, 1)), board.TileAt(pos + new Vector2Int(-1, 2))
        };

        for (int i = 0; i < 8; i++)
        {
            if (possibleMoves[i] != null && (!IsAnAlly(possibleMoves[i]) || IsAnEnemy(possibleMoves[i])))
                moves.Add(board.TileAt(possibleMoves[i].pos));
        }
        return moves;
    }
}