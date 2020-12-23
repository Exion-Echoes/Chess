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
        Vector2Int iSquareUp = new Vector2Int(boardCoords.x, boardCoords.y + 1);
        DetermineAllowedDestinations(iSquareUp);
        /*       if (iSquareUp.x >= 0 && iSquareUp.x <= 7 && iSquareUp.y >= 0 && iSquareUp.y <= 7) //Have to limit these to not get an out of reach exception 
               {
                   if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareUp))
                   {
                       if (CheckForAnEnemyPiece(iSquareUp) != null)
                           allowedDestinations.Add(iSquareUp);

                       else if (CheckForAnEnemyPiece(iSquareUp) == null && CheckForAFriendlyPiece(iSquareUp) == null)
                           allowedDestinations.Add(iSquareUp);
                   }
               }*/


        Vector2Int iSquareDown = new Vector2Int(boardCoords.x, boardCoords.y - 1);
        DetermineAllowedDestinations(iSquareDown);
        /*      if (iSquareDown.x >= 0 && iSquareDown.x <= 7 && iSquareDown.y >= 0 && iSquareDown.y <= 7) //Have to limit these to not get an out of reach exception 
              {
                  if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareDown))
                  {
                      if (CheckForAnEnemyPiece(iSquareDown) != null)
                          allowedDestinations.Add(iSquareDown);

                      else if (CheckForAnEnemyPiece(iSquareDown) == null && CheckForAFriendlyPiece(iSquareDown) == null)
                          allowedDestinations.Add(iSquareDown);
                  }
              }*/


        Vector2Int iSquareRight = new Vector2Int(boardCoords.x + 1, boardCoords.y);
        DetermineAllowedDestinations(iSquareRight);
        /*      if (iSquareRight.x >= 0 && iSquareRight.x <= 7 && iSquareRight.y >= 0 && iSquareRight.y <= 7) //Have to limit these to not get an out of reach exception 
              {
                  if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareRight))
                  {
                      if (CheckForAnEnemyPiece(iSquareRight) != null)
                          allowedDestinations.Add(iSquareRight);

                      else if (CheckForAnEnemyPiece(iSquareRight) == null && CheckForAFriendlyPiece(iSquareRight) == null)
                          allowedDestinations.Add(iSquareRight);
                  }
              }*/

        Vector2Int iSquareLeft = new Vector2Int(boardCoords.x - 1, boardCoords.y);
        DetermineAllowedDestinations(iSquareLeft);
        /*     if (iSquareLeft.x >= 0 && iSquareLeft.x <= 7 && iSquareLeft.y >= 0 && iSquareLeft.y <= 7) //Have to limit these to not get an out of reach exception 
             {
                 if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareLeft))
                 {
                     if (CheckForAnEnemyPiece(iSquareLeft) != null)
                         allowedDestinations.Add(iSquareLeft);
                     else if (CheckForAnEnemyPiece(iSquareLeft) == null && CheckForAFriendlyPiece(iSquareLeft) == null)
                         allowedDestinations.Add(iSquareLeft);
                 }
             }*/
        #endregion

        #region DIAGONALS
        Vector2Int iSquareUpRight = new Vector2Int(boardCoords.x + 1, boardCoords.y + 1);
        DetermineAllowedDestinations(iSquareUpRight);
/*        if (iSquareUpRight.x >= 0 && iSquareUpRight.x <= 7 && iSquareUpRight.y >= 0 && iSquareUpRight.y <= 7) //Have to limit these to not get an out of reach exception 
        {
            if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareUpRight))
            {
                if (CheckForAnEnemyPiece(iSquareUpRight) != null)
                    allowedDestinations.Add(iSquareUpRight);
                else if (CheckForAnEnemyPiece(iSquareUpRight) == null && CheckForAFriendlyPiece(iSquareUpRight) == null)
                    allowedDestinations.Add(iSquareUpRight);
            }
        }*/
        Vector2Int iSquareUpLeft = new Vector2Int(boardCoords.x - 1, boardCoords.y + 1);
        DetermineAllowedDestinations(iSquareUpLeft);

        /*        if (iSquareUpLeft.x >= 0 && iSquareUpLeft.x <= 7 && iSquareUpLeft.y >= 0 && iSquareUpLeft.y <= 7) //Have to limit these to not get an out of reach exception 
                {
                    if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareUpLeft))
                    {
                        if (CheckForAnEnemyPiece(iSquareUpLeft) != null)
                            allowedDestinations.Add(iSquareUpLeft);
                        else if (CheckForAnEnemyPiece(iSquareUpLeft) == null && CheckForAFriendlyPiece(iSquareUpLeft) == null)
                            allowedDestinations.Add(iSquareUpLeft);
                    }
                }
        */
        Vector2Int iSquareDownRight = new Vector2Int(boardCoords.x + 1, boardCoords.y - 1);
        DetermineAllowedDestinations(iSquareDownRight);
/*        if (iSquareDownRight.x >= 0 && iSquareDownRight.x <= 7 && iSquareDownRight.y >= 0 && iSquareDownRight.y <= 7) //Have to limit these to not get an out of reach exception 
        {
            if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareDownRight))
            {
                if (CheckForAnEnemyPiece(iSquareDownRight) != null)
                    allowedDestinations.Add(iSquareDownRight);
                else if (CheckForAnEnemyPiece(iSquareDownRight) == null && CheckForAFriendlyPiece(iSquareDownRight) == null)
                    allowedDestinations.Add(iSquareDownRight);
            }
        }*/
        Vector2Int iSquareDownLeft = new Vector2Int(boardCoords.x - 1, boardCoords.y - 1);
        DetermineAllowedDestinations(iSquareDownLeft);
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