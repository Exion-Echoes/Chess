﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    public override List<Tile> PossibleMoves() //Any allowed movement must be part of this list
    {
        List<Tile> moves = new List<Tile>();

        int y = pos.y;
        for (int x = pos.x - 1; x >= 0; x--) //Upper left diagonal
        {
            if (y < 7)
            {
                y++;
                Tile tile = board.TileAt(new Vector2Int(x, y));
                if (tile != null)
                {
                    if (IsAnAlly(tile))
                        break;

                    moves.Add(tile);
                    if (IsAnEnemy(tile))
                        break;
                }
            }
        }
        y = pos.y;
        for (int x = pos.x - 1; x >= 0; x--) //Down left diagonal
        {
            if (y > 0)
            {
                y--;
                Tile tile = board.TileAt(new Vector2Int(x, y));
                if (tile != null)
                {
                    if (IsAnAlly(tile))
                        break;

                    moves.Add(tile);
                    if (IsAnEnemy(tile))
                        break;
                }
            }
        }
        y = pos.y;
        for (int x = pos.x + 1; x <= 7; x++) //Upper right diagonal
        {
            if (y < 7)
            {
                y++;
                Tile tile = board.TileAt(new Vector2Int(x, y));
                if (tile != null)
                {
                    if (IsAnAlly(tile))
                        break;

                    moves.Add(tile);
                    if (IsAnEnemy(tile))
                        break;
                }
            }
        }
        y = pos.y;
        for (int x = pos.x + 1; x <= 7; x++) //Down right diagonal
        {
            if (y > 0)
            {
                y--;
                Tile tile = board.TileAt(new Vector2Int(x, y));
                if (tile != null)
                {
                    if (IsAnAlly(tile))
                        break;

                    moves.Add(tile);
                    if (IsAnEnemy(tile))
                        break;
                }
            }
        }
        return moves;
    }
}