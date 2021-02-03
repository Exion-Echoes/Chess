using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public bool becameAnEnPassantPawn;
    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        becameAnEnPassantPawn = false;

        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        bool moveAllowed = InTheList(PossibleMoves(), endTile);
        if (moveAllowed)
        {
            if (!MovingChecksOwnKing(startTile, endTile))
            {
                //I'm assuming that if the code makes it here, the pawn movement will go through
                if (endTile == board.TileAt(new Vector2Int(pos.x, pos.y + 2 * (isWhite ? 1 : -1))))
                {
                    board.enPassantTile = startTile;
                    becameAnEnPassantPawn = true;
                }
                return true;
            }
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
        if (((pos.y == 1 && isWhite) || (pos.y == 6 && !isWhite)) && !IsAnAlly(possibleMoves[1]) && !IsAnEnemy(possibleMoves[1]))
            moves.Add(possibleMoves[1]);

        for (int i = 2; i <= 3; i++) //Attacks
        {
            //If an enemy is present on the diagonal, or an enpassant pawn is ready, the movement is allowed
            if (IsAnEnemy(possibleMoves[i]) || (board.enPassantTile != null && board.enPassantTile.pos.x == possibleMoves[i].pos.x))
                moves.Add(possibleMoves[i]);
        }

        return moves;
    }
}