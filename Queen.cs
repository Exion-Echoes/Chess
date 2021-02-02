using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        //
        return false;
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