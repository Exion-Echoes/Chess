using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public bool isChecked;
    public Rook[] rooks = new Rook[2]; //Used for determining if castling is possible

    public override void Start()
    {
        base.Start();

        int j = 0;
        for (int i = 0; i < (isWhite ? board.wPieces.Count : board.bPieces.Count); i++)
        {
            if ((isWhite ? board.wPieces[i].GetComponent<Rook>() != null : board.bPieces[i].GetComponent<Rook>() != null))
            {
                rooks[j] = (isWhite ? board.wPieces[i].GetComponent<Rook>() : board.bPieces[i].GetComponent<Rook>());
                j++;
            }
        }
    }

    public override void UpdateAllowedDestinations()
    {
        allowedDestinations.Clear();
        positionsDefended.Clear();

        //**INSTEAD OF CHECKING IF POSITIONDEFEND BY OTHER KING, I CAN CHECK 8 LOCATIONS AROUND ENEMY KING - KINGS CAN NEVER BE NEAR EACH OTHER

        bool WillKingBecomeChecked(Vector2Int testPos)
        {
            //Compare test Pos with enemy allowed movements
            //Have to look at enemy allowed destinations that are ATTACKS (every non-pawn can have their allowed destinations scanned through, pawns only their diagonals)

            List<Vector2Int> allowedDestinations = new List<Vector2Int>();

            for (int i = 0; i < (isWhite ? board.bPieces.Count : board.wPieces.Count); i++)
            {
                if (isWhite ? !board.bPieces[i].isAPawn : !board.wPieces[i].isAPawn) //If not a pawn, check all allowed destinations
                {
                    for (int j = 0; j < (isWhite ? board.bPieces[i].allowedDestinations.Count : board.wPieces[i].allowedDestinations.Count); j++)
                    {
                        if (testPos == (isWhite ? board.bPieces[i].allowedDestinations[j] : board.wPieces[i].allowedDestinations[j]))
                            return true;
                    }
                }
                else //If it's a pawn, check for their diagonal attacks
                {
                    Vector2Int rightDiagonal = isWhite ? board.bPieces[i].boardCoords + new Vector2Int(1, -1) : board.wPieces[i].boardCoords + new Vector2Int(1, 1);
                    Vector2Int leftDiagonal = isWhite ? board.bPieces[i].boardCoords + new Vector2Int(-1, -1) : board.wPieces[i].boardCoords + new Vector2Int(-1, 1);
                    if (testPos == rightDiagonal || testPos == leftDiagonal)
                        return true;
                }
            }
            return false;
        }

        //**DEFINE POTENTIAL ATTACKING LINES, E.G. RIGHT NOW QUEEN FORBIDS KING FROM MOVING TOWARDS, BUT NOT BACKWARDS (BECAUSE THAT'S NOT CONSIDERED IN THE CODE)
        //**ATTACKING LINES COULD START FROM THE FIRST ALLIED PAWN AND END AT THE NEXT PAWN (ALLY OR ENEMY)


        //**
        //**ALSO NEEDS TO VERIFY THAT CASTLING IS POSSIBLE
        //**Maybe I can initialize allied rooks and add a bool "moved" for them, which must be off for castling to be available
        //**

        bool IsCheckedAfterEating(Vector2Int testPos) //Check if King would be checked after eating the piece at testPos
        {
            for (int i = 0; i < (isWhite ? board.bPieces.Count : board.wPieces.Count); i++)
            {
                for (int j = 0; j < (isWhite ? board.bPieces[i].positionsDefended.Count : board.wPieces[i].positionsDefended.Count); j++)
                {
                    if (testPos == (isWhite ? board.bPieces[i].positionsDefended[j] : board.wPieces[i].positionsDefended[j]))
                        return true;
                }
            }
            return false;
        }
        
        bool IsEnemyKingTooClose(Vector2Int testPos) //Check 8 positions around enemy king and forbid moving there
        {
            if ((int)Mathf.Abs(testPos.x - enemyKing.boardCoords.x) <= 1 && (int)Mathf.Abs(testPos.y - enemyKing.boardCoords.y) <= 1)
                return true;
            return false;
        }

        void DestinationFunction(Vector2Int testDirection, int travelDistance)
        {
            Vector2Int testBoardCoords = boardCoords + new Vector2Int(testDirection.x * 1, testDirection.y * 1);
            if (!IsThisOOB(testBoardCoords) && !IsThereAnAlly(testBoardCoords) && !IsEnemyKingTooClose(testBoardCoords))
            {
                if (IsThereAnEnemy(testBoardCoords) && !IsCheckedAfterEating(testBoardCoords))
                    allowedDestinations.Add(testBoardCoords);

                else if (!WillKingBecomeChecked(testBoardCoords) && !IsThereAnEnemy(testBoardCoords))
                    allowedDestinations.Add(testBoardCoords);
            }
        }

        DestinationFunction(new Vector2Int(0, 1), 1);
        DestinationFunction(new Vector2Int(0, -1), 1);
        DestinationFunction(new Vector2Int(1, 0), 1);
        DestinationFunction(new Vector2Int(-1, 0), 1);
        DestinationFunction(new Vector2Int(1, 1), 1);
        DestinationFunction(new Vector2Int(-1, 1), 1);
        DestinationFunction(new Vector2Int(1, -1), 1);
        DestinationFunction(new Vector2Int(-1, -1), 1);
    }

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
            if (!WillMovingPiecePutOwnKingInCheck(boardCoords, testBoardCoords))
            {
                if (CheckForAnEnemyPiece(testBoardCoords) != null)
                    allowedDestinations.Add(testBoardCoords);
                else if (CheckForAnEnemyPiece(testBoardCoords) == null && CheckForAFriendlyPiece(testBoardCoords) == null)
                    allowedDestinations.Add(testBoardCoords);
            }
        }
    }

    public bool KingIsCheckedIfPieceMovesAt(Piece piece, Vector2Int pos)
    {
        Vector2Int pastPos = piece.boardCoords;

        if (boardCoords.x - pastPos.x == boardCoords.y - pastPos.y && boardCoords.x != pastPos.x && boardCoords.y != pastPos.y)
        {
            //Diagonal check - bishops and queen
            //IsThereAnAlly() and IsThereAnEnemy() at each spot
        }
        else if (boardCoords.x == pastPos.x || boardCoords.y == pastPos.y)
        {
            //Vertical/Horizontal check - rooks and queen
        }

        //Check if opening up pastPos would make the king checked, in which case return true

        //Check if moving to pos would stop the king from being checked, in which case return false

        //Look at piece
        return false;
    }

    //If isChecked, and another move happens, isChecked must be turned back to false automatically. If no moves are possible when isChecked, then gameState = Checkmate

}