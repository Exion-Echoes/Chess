using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{

    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        //Possible moves are such that m x and y movements gives 1 when one component is 0 (hori/vert) and 2 when both components are equal (diagonal)
        Vector2Int move = new Vector2Int(Mathf.Abs(startTile.pos.x - endTile.pos.x), Mathf.Abs(startTile.pos.y - endTile.pos.y));
        bool moveAllowed = move.x * move.y == 2;
        if (moveAllowed)
        {
            if (!MovingChecksOwnKing(startTile, endTile))
                return true;
            return true;
        }

        //
        return false;
    }

    public override List<Tile> PossibleMoves()
    {
        return null;
        return new List<Tile> {
            board.TileAt(pos + new Vector2Int(1, 2)), board.TileAt(pos + new Vector2Int(2, 1)), board.TileAt(pos + new Vector2Int(2, -1)), board.TileAt(pos + new Vector2Int(1, -2)),
            board.TileAt(pos + new Vector2Int(-1, -2)), board.TileAt(pos + new Vector2Int(-2, -1)), board.TileAt(pos + new Vector2Int(-2, 1)), board.TileAt(pos + new Vector2Int(-1, 2)) };
    }

    /*
    public override void UpdateAllowedDestinations()
    {
        allowedDestinations.Clear();
        positionsDefended.Clear();

        void DestinationFunction(Vector2Int testPos)
        {
            if (!IsThisOOB(testPos) && !IsOwnKingChecked(testPos))
            {
                if (!IsThereAnAlly(testPos))
                    allowedDestinations.Add(testPos);
                else
                    positionsDefended.Add(testPos);
            }
        }

        DestinationFunction(boardCoords + new Vector2Int(1, 2));
        DestinationFunction(boardCoords + new Vector2Int(2, 1));
        DestinationFunction(boardCoords + new Vector2Int(2, -1));
        DestinationFunction(boardCoords + new Vector2Int(1, -2));
        DestinationFunction(boardCoords + new Vector2Int(-1, -2));
        DestinationFunction(boardCoords + new Vector2Int(-2, -1));
        DestinationFunction(boardCoords + new Vector2Int(-2, 1));
        DestinationFunction(boardCoords + new Vector2Int(-1, 2));
    }

    public override void InitialAllowedDestinations()
    {
        allowedDestinations.Add(boardCoords + (isWhite ? Vector2Int.up + Vector2Int.up + Vector2Int.right : Vector2Int.down + Vector2Int.down + Vector2Int.right));
        allowedDestinations.Add(boardCoords + (isWhite ? Vector2Int.up + Vector2Int.up - Vector2Int.right : Vector2Int.down + Vector2Int.down - Vector2Int.right));
    }

    */
}