using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
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

    public override void CheckIfMovingPutOpposingKingOnCheck()
    {
        DeterminePossibleActions();

        for(int i = 0; i < allowedDestinations.Count; i++)
        {
            if (board.boardArray[allowedDestinations[i].x, allowedDestinations[i].y] == opposingKing)
                opposingKing.isChecked = true;
        }
    }
}