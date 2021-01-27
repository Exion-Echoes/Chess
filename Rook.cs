using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    public bool moved; //Used for determining if castling is available

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

        DestinationFunction(new Vector2Int(0, 1), travelDistanceUp);
        DestinationFunction(new Vector2Int(0, -1), travelDistanceDown);
        DestinationFunction(new Vector2Int(1, 0), travelDistanceRight);
        DestinationFunction(new Vector2Int(-1, 0), travelDistanceLeft);
    }

    public override void DeterminePossibleActions()
    {
        allowedDestinations.Clear();

        //Check along the 4 paths around the rook and stop when it reaches the end of the board or a piece (before friendly and on top of enemy)

        int travelDistanceUp = 7 - boardCoords.y;
        int travelDistanceDown = boardCoords.y;
        int travelDistanceRight = 7 - boardCoords.x;
        int travelDistanceLeft = boardCoords.x;

        DetermineAllowedDestinations(new Vector2Int(0, 1), travelDistanceUp);
        DetermineAllowedDestinations(new Vector2Int(0, -1), travelDistanceDown);
        DetermineAllowedDestinations(new Vector2Int(1, 0), travelDistanceRight);
        DetermineAllowedDestinations(new Vector2Int(-1, 0), travelDistanceLeft);
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