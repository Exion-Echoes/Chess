using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece //,including Functions? instead of having to write them all the time
{
    public override void DeterminePossibleActions()
    {
        allowedDestinations.Clear();

        #region 8 POSSIBLE MOVEMENTS
        DetermineAllowedDestinations(boardCoords + new Vector2Int(1, 2));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(2, 1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(2, -1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(1, -2));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(-1, -2));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(-2, -1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(-2, 1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(-1, 2));
        #endregion
    }
    void DetermineAllowedDestinations(Vector2Int testBoardCoords)
    {
        if (testBoardCoords.x >= 0 && testBoardCoords.x <= 7 && testBoardCoords.y >= 0 && testBoardCoords.y <= 7) //Have to limit these to not get an out of reach exception 
        {
            if (!WillMovingPiecePutKingInCheck(boardCoords, testBoardCoords))
            {
                if (CheckForAnEnemyPiece(testBoardCoords) != null)
                    allowedDestinations.Add(testBoardCoords);
                else if (CheckForAnEnemyPiece(testBoardCoords) == null && CheckForAFriendlyPiece(testBoardCoords) == null)
                    allowedDestinations.Add(testBoardCoords);
            }
        }
    }
}
