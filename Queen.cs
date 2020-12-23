using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    public override void DeterminePossibleActions()
    {
        allowedDestinations.Clear();

        //Check along the 8 paths around the queen and stop when it reaches the end of the board or a piece (before friendly and on top of enemy)

        int travelDistanceUp = 7 - boardCoords.y;
        int travelDistanceDown = boardCoords.y;
        int travelDistanceRight = 7 - boardCoords.x;
        int travelDistanceLeft = boardCoords.x;
        
        //Diagonal additions to Rook's logic
        //Since it's a square grid, the diagonal squares to consider can be determined by looking at the same coordinates as before
        int travelDistanceUpRight = 7 - boardCoords.y;
        int travelDistanceUpLeft = boardCoords.y;
        int travelDistanceDownRight = 7 - boardCoords.x;
        int travelDistanceDownLeft = boardCoords.x;

        #region COPY FROM ROOK LOGIC
        for (int i = 1; i <= travelDistanceUp; i++)
        {
            Vector2Int iSquareUp = new Vector2Int(boardCoords.x, boardCoords.y + i);
            if (iSquareUp.x >= 0 && iSquareUp.x <= 7 && iSquareUp.y >= 0 && iSquareUp.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareUp))
                {
                    if (CheckForAnEnemyPiece(iSquareUp) != null)
                    {
                        allowedDestinations.Add(iSquareUp);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareUp) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareUp) == null && CheckForAFriendlyPiece(iSquareUp) == null)
                        allowedDestinations.Add(iSquareUp);
                }
            }
        }

        for (int i = 1; i <= travelDistanceDown; i++)
        {
            Vector2Int iSquareDown = new Vector2Int(boardCoords.x, boardCoords.y - i);
            if (iSquareDown.x >= 0 && iSquareDown.x <= 7 && iSquareDown.y >= 0 && iSquareDown.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                //                Debug.Log("BEFORE DOUBLE IF " + i + ", " + board.boardArray[boardCoords.x, boardCoords.y - i] + ", " + CheckForAnEnemyPiece(iSquareDown) + ", " + !WillMovingPiecePutKingInCheck(iSquareDown));
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareDown))
                {
                    //                    Debug.Log(i + ", " + board.boardArray[boardCoords.x, boardCoords.y - i] + ", " + CheckForAnEnemyPiece(iSquareDown) + ": enemy, friend : " + CheckForAFriendlyPiece(iSquareDown) + ", " + !WillMovingPiecePutKingInCheck(iSquareDown));

                    if (CheckForAnEnemyPiece(iSquareDown) != null)
                    {
                        allowedDestinations.Add(iSquareDown);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareDown) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareDown) == null && CheckForAFriendlyPiece(iSquareDown) == null)
                        allowedDestinations.Add(iSquareDown);
                    //                    Debug.Log(i + ", " + board.boardArray[boardCoords.x, boardCoords.y - i] + ", " + CheckForAnEnemyPiece(iSquareDown) + ", " + !WillMovingPiecePutKingInCheck(iSquareDown));
                }
            }
        }

        for (int i = 1; i <= travelDistanceRight; i++)
        {
            Vector2Int iSquareRight = new Vector2Int(boardCoords.x + i, boardCoords.y);
            if (iSquareRight.x >= 0 && iSquareRight.x <= 7 && iSquareRight.y >= 0 && iSquareRight.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareRight))
                {
                    if (CheckForAnEnemyPiece(iSquareRight) != null)
                    {
                        allowedDestinations.Add(iSquareRight);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareRight) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareRight) == null && CheckForAFriendlyPiece(iSquareRight) == null)
                        allowedDestinations.Add(iSquareRight);
                }
            }
        }

        for (int i = 1; i <= travelDistanceLeft; i++)
        {
            Vector2Int iSquareLeft = new Vector2Int(boardCoords.x - i, boardCoords.y);
            if (iSquareLeft.x >= 0 && iSquareLeft.x <= 7 && iSquareLeft.y >= 0 && iSquareLeft.y <= 7) //Have to limit these to not get an out of reach exception 
            {
                if (!WillMovingPiecePutKingInCheck(boardCoords, iSquareLeft))
                {
                    if (CheckForAnEnemyPiece(iSquareLeft) != null)
                    {
                        allowedDestinations.Add(iSquareLeft);
                        break; //Rook can eat this piece, but can't move farther
                    }

                    else if (CheckForAFriendlyPiece(iSquareLeft) != null)
                        break; //Rook has to stop before this piece, so no other allowed destination is added

                    else if (CheckForAnEnemyPiece(iSquareLeft) == null && CheckForAFriendlyPiece(iSquareLeft) == null)
                        allowedDestinations.Add(iSquareLeft);
                }
            }
        }
        #endregion

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
