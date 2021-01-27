using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
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

        DestinationFunction(new Vector2Int(1, 1), travelDistanceUpRight);
        DestinationFunction(new Vector2Int(-1, 1), travelDistanceUpLeft);
        DestinationFunction(new Vector2Int(1, -1), travelDistanceDownRight);
        DestinationFunction(new Vector2Int(-1, -1), travelDistanceDownLeft);
    }

    public override void DeterminePossibleActions()
    {
        allowedDestinations.Clear();

        //Check along the 4 paths around the bishop and stop when it reaches the end of the board or a piece (before friendly and on top of enemy)
        int travelDistanceUp = 7 - boardCoords.y;
        int travelDistanceDown = boardCoords.y;
        int travelDistanceRight = 7 - boardCoords.x;
        int travelDistanceLeft = boardCoords.x;

        //Diagonal additions to Rook's logic
        //Since it's a square grid, the diagonal squares to consider can be determined by looking at the same coordinates as before
        int travelDistanceUpRight = Mathf.Max(travelDistanceUp, travelDistanceRight);
        int travelDistanceUpLeft = Mathf.Max(travelDistanceUp, travelDistanceLeft);
        int travelDistanceDownRight = Mathf.Max(travelDistanceDown, travelDistanceRight);
        int travelDistanceDownLeft = Mathf.Max(travelDistanceDown, travelDistanceLeft);

        #region DIAGONAL
        DetermineAllowedDestinations(new Vector2Int(1, 1), travelDistanceUpRight);
        DetermineAllowedDestinations(new Vector2Int(-1, 1), travelDistanceUpLeft);
        DetermineAllowedDestinations(new Vector2Int(1, -1), travelDistanceDownRight);
        DetermineAllowedDestinations(new Vector2Int(-1, -1), travelDistanceDownLeft);
        #endregion
    }
    void DetermineAllowedDestinations(Vector2Int testDirection, int travelDistance)
    {
        for (int i = 1; i <= travelDistance; i++)
        {
            Vector2Int testBoardCoords = boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
            if (testBoardCoords.x >= 0 && testBoardCoords.x <= 7 && testBoardCoords.y >= 0 && testBoardCoords.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutOwnKingInCheck(boardCoords, testBoardCoords))
                {
                    if (CheckForAnEnemyPiece(testBoardCoords) != null)
                    {
                        allowedDestinations.Add(testBoardCoords);
                        break; //Queen can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(testBoardCoords) != null)
                        break; //Queen has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(testBoardCoords) == null && CheckForAFriendlyPiece(testBoardCoords) == null)
                        allowedDestinations.Add(testBoardCoords);
                }
            }
        }
    }
}