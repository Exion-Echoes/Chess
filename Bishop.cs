using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
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
                if (!WillMovingPiecePutKingInCheck(boardCoords, testBoardCoords))
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