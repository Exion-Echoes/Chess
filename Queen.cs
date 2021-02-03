using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    public override List<Tile> PossibleMoves() //Any allowed movement must be part of this list
    {
        List<Tile> moves = new List<Tile>();

        for (int x = pos.x - 1; x >= 0; x--) //Left
        {
            if (IsAnAlly(board.TileAt(new Vector2Int(x, pos.y))))
                break;

            moves.Add(board.TileAt(new Vector2Int(x, pos.y)));
            if (IsAnEnemy(board.TileAt(new Vector2Int(x, pos.y))))
                break;
        }
        for (int y = pos.y - 1; y >= 0; y--) //Down
        {
            if (IsAnAlly(board.TileAt(new Vector2Int(pos.x, y))))
                break;

            moves.Add(board.TileAt(new Vector2Int(pos.x, y)));
            if (IsAnEnemy(board.TileAt(new Vector2Int(pos.x, y))))
                break;
        }
        for (int x = pos.x + 1; x <= 7; x++) //Right
        {
            if (IsAnAlly(board.TileAt(new Vector2Int(x, pos.y))))
                break;

            moves.Add(board.TileAt(new Vector2Int(x, pos.y)));
            if (IsAnEnemy(board.TileAt(new Vector2Int(x, pos.y))))
                break;
        }
        for (int y = pos.y + 1; y <= 7; y++) //Up
        {
            if (IsAnAlly(board.TileAt(new Vector2Int(pos.x, y))))
                break;

            moves.Add(board.TileAt(new Vector2Int(pos.x, y)));
            if (IsAnEnemy(board.TileAt(new Vector2Int(pos.x, y))))
                break;
        }

        int yVal = pos.y;
        for (int x = pos.x - 1; x >= 0; x--) //Upper left diagonal
        {
            if (yVal <= 7)
            {
                yVal++;
                if (IsAnAlly(board.TileAt(new Vector2Int(x, yVal))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, yVal)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, yVal))))
                    break;
            }
        }
        yVal = pos.y;
        for (int x = pos.x - 1; x >= 0; x--) //Down left diagonal
        {
            if (yVal >= 0)
            {
                yVal--;
                if (IsAnAlly(board.TileAt(new Vector2Int(x, yVal))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, yVal)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, yVal))))
                    break;
            }
        }
        yVal = pos.y;
        for (int x = pos.x + 1; x <= 7; x++) //Upper right diagonal
        {
            if (yVal <= 7)
            {
                yVal++;
                if (IsAnAlly(board.TileAt(new Vector2Int(x, yVal))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, yVal)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, yVal))))
                    break;
            }
        }
        yVal = pos.y;
        for (int x = pos.x + 1; x <= 7; x++) //Down right diagonal
        {
            if (yVal >= 0)
            {
                yVal--;
                if (IsAnAlly(board.TileAt(new Vector2Int(x, yVal))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, yVal)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, yVal))))
                    break;
            }
        }

        return moves;
    }








    /*
    public override void UpdateAllowedDestinations()
    {
        allowedDestinations.Clear();
        positionsDefended.Clear();

        void DestinationFunction(Vector2Int testDirection, int travelDistance)
        {
            for (int i = 1; i <= travelDistance; i++)
            {
                Vector2Int testBoardCoords = boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
                if (!IsThisOOB(testBoardCoords) && !IsOwnKingChecked(testBoardCoords))
                {
                    if (IsThereAnEnemy(testBoardCoords))
                    {
                        allowedDestinations.Add(testBoardCoords);
                        break; //Can eat this piece, but can't move farther
                    }

                    else if (IsThereAnAlly(testBoardCoords))
                    {
                        positionsDefended.Add(testBoardCoords);
                        break; //Has to stop before this piece, so no further allowed destination is added
                    }

                    else if (!IsThereAnEnemy(testBoardCoords) && !IsThereAnAlly(testBoardCoords))
                        allowedDestinations.Add(testBoardCoords);
                }
            }
        }

        int travelDistanceUp = 7 - boardCoords.y;
        int travelDistanceDown = boardCoords.y;
        int travelDistanceRight = 7 - boardCoords.x;
        int travelDistanceLeft = boardCoords.x;
        int travelDistanceUpRight = Mathf.Max(travelDistanceUp, travelDistanceRight);
        int travelDistanceUpLeft = Mathf.Max(travelDistanceUp, travelDistanceLeft);
        int travelDistanceDownRight = Mathf.Max(travelDistanceDown, travelDistanceRight);
        int travelDistanceDownLeft = Mathf.Max(travelDistanceDown, travelDistanceLeft);

        DestinationFunction(new Vector2Int(0, 1), travelDistanceUp);
        DestinationFunction(new Vector2Int(0, -1), travelDistanceDown);
        DestinationFunction(new Vector2Int(1, 0), travelDistanceRight);
        DestinationFunction(new Vector2Int(-1, 0), travelDistanceLeft);
        DestinationFunction(new Vector2Int(1, 1), travelDistanceUpRight);
        DestinationFunction(new Vector2Int(-1, 1), travelDistanceUpLeft);
        DestinationFunction(new Vector2Int(1, -1), travelDistanceDownRight);
        DestinationFunction(new Vector2Int(-1, -1), travelDistanceDownLeft);
    }*/
}