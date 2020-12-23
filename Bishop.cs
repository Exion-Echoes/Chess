using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    public override void DeterminePossibleActions()
    {
        allowedDestinations.Clear();

        //Check along the 4 paths around the bishop and stop when it reaches the end of the board or a piece (before friendly and on top of enemy)
        int travelDistanceUpRight = 7 - boardCoords.y;
        int travelDistanceUpLeft = boardCoords.y;
        int travelDistanceDownRight = 7 - boardCoords.x;
        int travelDistanceDownLeft = boardCoords.x;

        #region DIAGONAL ADDITIONS TO ROOK LOGIC
        for (int i = 1; i <= travelDistanceUpRight; i++)
        {
            Vector2Int iSquareUpRight = new Vector2Int(boardCoords.x + i, boardCoords.y + i);
            if (iSquareUpRight.x >= 0 && iSquareUpRight.x <= 7 && iSquareUpRight.y >= 0 && iSquareUpRight.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareUpRight))
                {
                    if (CheckForAnEnemyPiece(iSquareUpRight) != null)
                    {
                        allowedDestinations.Add(iSquareUpRight);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareUpRight) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareUpRight) == null && CheckForAFriendlyPiece(iSquareUpRight) == null)
                        allowedDestinations.Add(iSquareUpRight);
                }
            }
        }
        for (int i = 1; i <= travelDistanceUpLeft; i++)
        {
            Vector2Int iSquareUpLeft = new Vector2Int(boardCoords.x - i, boardCoords.y + i);
            if (iSquareUpLeft.x >= 0 && iSquareUpLeft.x <= 7 && iSquareUpLeft.y >= 0 && iSquareUpLeft.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareUpLeft))
                {
                    if (CheckForAnEnemyPiece(iSquareUpLeft) != null)
                    {
                        allowedDestinations.Add(iSquareUpLeft);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareUpLeft) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareUpLeft) == null && CheckForAFriendlyPiece(iSquareUpLeft) == null)
                        allowedDestinations.Add(iSquareUpLeft);
                }
            }
        }
        for (int i = 1; i <= travelDistanceDownRight; i++)
        {
            Vector2Int iSquareDownRight = new Vector2Int(boardCoords.x + i, boardCoords.y - i);
            if (iSquareDownRight.x >= 0 && iSquareDownRight.x <= 7 && iSquareDownRight.y >= 0 && iSquareDownRight.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareDownRight))
                {
                    if (CheckForAnEnemyPiece(iSquareDownRight) != null)
                    {
                        allowedDestinations.Add(iSquareDownRight);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareDownRight) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareDownRight) == null && CheckForAFriendlyPiece(iSquareDownRight) == null)
                        allowedDestinations.Add(iSquareDownRight);
                }
            }
        }
        for (int i = 1; i <= travelDistanceDownLeft; i++)
        {
            Vector2Int iSquareDownLeft = new Vector2Int(boardCoords.x - i, boardCoords.y - i);
            if (iSquareDownLeft.x >= 0 && iSquareDownLeft.x <= 7 && iSquareDownLeft.y >= 0 && iSquareDownLeft.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareDownLeft))
                {
                    if (CheckForAnEnemyPiece(iSquareDownLeft) != null)
                    {
                        allowedDestinations.Add(iSquareDownLeft);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareDownLeft) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareDownLeft) == null && CheckForAFriendlyPiece(iSquareDownLeft) == null)
                        allowedDestinations.Add(iSquareDownLeft);
                }
            }
        }
        #endregion
    }
}
