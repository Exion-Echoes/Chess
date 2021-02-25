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

        if (pos == new Vector2Int(2, 2))
        {
            for (int i = 0; i < 8; i++)
            {
    //            Debug.Log(i + ", " + possibleMoves[i].pos);
  //              if (IsAnAlly(possibleMoves[i]))
//                    Debug.Log(board.state[1].piece + ", " + board.state[18].piece + ", " + possibleMoves[i].piece + ", " + board.TileAt(new Vector2Int(2,2)).piece);
            }
        }

        for (int i = 0; i < 8; i++)
        {
//            if (pos == new Vector2Int(2, 2))
  //              Debug.Log(i + ", " + possibleMoves[i].pos);
            if (possibleMoves[i] != null && (!IsAnAlly(possibleMoves[i]) || IsAnEnemy(possibleMoves[i])))
                moves.Add(board.TileAt(possibleMoves[i].pos));
        }
        return moves;
    }
}