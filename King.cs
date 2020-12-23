using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public bool isChecked;

    public override void DeterminePossibleActions()
    {
        allowedDestinations.Clear();

        //Check along the 8 paths around the king and stop when it reaches the end of the board or a piece (before friendly and on top of enemy)

        #region HORIZONTAL AND VERTICAL
        DetermineAllowedDestinations(boardCoords + new Vector2Int(0, 1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(0, -1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(1, 0));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(-1, 0));
        #endregion

        #region DIAGONALS
        DetermineAllowedDestinations(boardCoords + new Vector2Int(1, 1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(-1, 1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(1, -1));
        DetermineAllowedDestinations(boardCoords + new Vector2Int(-1, -1));
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