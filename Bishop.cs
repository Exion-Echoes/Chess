using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        //Possible moves are such that m x and y movements gives 1 when one component is 0 (hori/vert) and 2 when both components are equal (diagonal)
        //**Replace with something else since bishops have line movement that's blocked at the first non empty tile
        bool moveAllowed = InTheList(PossibleMoves(), endTile);
//        Debug.Log(moveAllowed + ", " + endTile.pos);

        if (moveAllowed)
        {
            if (!MovingChecksOwnKing(startTile, endTile))
                return true;
            return true;
        }

        //
        return false;
    }

    //GET BISHOP WORKING, THEN GET BOARD STATE WORKING, AND GET MOVINGHCECKSOWNKING WORKING (WANT BISHOP TO MOVE ALONG THE DIAGONAL IT'S DEFENDING)


    public override List<Tile> PossibleMoves() //Any allowed movement must be part of this list
    {
        List<Tile> moves = new List<Tile>();

        int y = pos.y;
        for (int x = pos.x - 1; x >= 0; x--) //Upper left diagonal
        {
            if (y <= 7)
            {
                y++;
                if (IsAnAlly(board.TileAt(new Vector2Int(x, y))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, y)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, y))))
                    break;
            }
        }
        y = pos.y;
        for (int x = pos.x - 1; x >= 0; x--) //Down left diagonal
        {
            if (y >= 0)
            {
                y--;
                if (IsAnAlly(board.TileAt(new Vector2Int(x, y))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, y)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, y))))
                    break;
            }
        }
        y = pos.y;
        for (int x = pos.x + 1; x <= 7; x++) //Upper right diagonal
        {
            if (y <= 7)
            {
                y++;
//                Debug.Log(x + ", " + y + ", " + board.TileAt(new Vector2Int(x, y)).piece);
                if (IsAnAlly(board.TileAt(new Vector2Int(x, y))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, y)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, y))))
                    break;
            }
        }
        y = pos.y;
        for (int x = pos.x + 1; x <= 7; x++) //Down right diagonal
        {
            if (y >= 0)
            {
                y--;
                if (IsAnAlly(board.TileAt(new Vector2Int(x, y))))
                    break;

                moves.Add(board.TileAt(new Vector2Int(x, y)));
                if (IsAnEnemy(board.TileAt(new Vector2Int(x, y))))
                    break;
            }
        }

        return moves;
    }


    /*
    public override void UpdateAllowedDestinations()
    {
        allowedDestinations.Clear();
        positionsDefended.Clear();
        linesOfAttack.Clear();

        void DestinationFunction(Vector2Int testDirection, int travelDistance)
        {
            beginLinesOfAttack = false;
            for (int i = 1; i <= travelDistance; i++)
            {
                Vector2Int testBoardCoords = boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
                if (!IsThisOOB(testBoardCoords) && !IsOwnKingChecked(testBoardCoords))
                {
                    if (!beginLinesOfAttack)
                    {
                        if (IsThereAnEnemy(testBoardCoords))
                        {
                            allowedDestinations.Add(testBoardCoords);
                            break;
//                            beginLinesOfAttack = true; //Can eat this piece, but can't move farther
                        }

                        else if (IsThereAnAlly(testBoardCoords))
                        {
                            positionsDefended.Add(testBoardCoords);
                            break;
//                            beginLinesOfAttack = true; //Has to stop before this piece, so no further allowed destination is added
                        }

                        else if (!IsThereAnEnemy(testBoardCoords) && !IsThereAnAlly(testBoardCoords))
                            allowedDestinations.Add(testBoardCoords);
                    }
                    else //Carry on destination calculations until we hit another piece
                    {
                        if (!IsThereAnAlly(testBoardCoords) && !IsThereAnEnemy(testBoardCoords))
                            linesOfAttack.Add(testBoardCoords);

                        else if (IsThereAnEnemy(testBoardCoords))
                        {
                            linesOfAttack.Add(testBoardCoords);
                            break;
                        }

                        else if (IsThereAnAlly(testBoardCoords))
                            break;
                    }
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

    */
}