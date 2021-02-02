using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public bool moved;
    public bool isChecked; //Limits movements this turn

    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        //Possible moves are such that adding x and y movements gives 1 when one component is 0 (hori/vert) and 2 when both components are equal (diagonal)
        Vector2Int move = new Vector2Int(Mathf.Abs(startTile.pos.x - endTile.pos.x), Mathf.Abs(startTile.pos.y - endTile.pos.y));
        bool moveAllowed = move.x + move.y == 1 || move.x * move.y == 1;// ((posMove.x == 0 || posMove.y == 0) && (posMove.x + posMove.y == 1)) || ((posMove.x == posMove.y) && (posMove.x + posMove.y == 2));
        if(moveAllowed)
        {
            if (KingAttackedAt(endTile))
                return false;
            return true;
        }

        return PerformCastling(endTile);
    }

    bool KingAttackedAt(Tile tile)
    {
        //If tile already contains a piece, imagine it was this king instead
        //Update board state to see if any enemy attacks at the tile's location
        //If an attack exists, return true, which forbids the king moving there
        //Attacks consist of every PossibleMove() of opponent pieces, minus pawns, from whom we only need diagonal moves
        //Otherwise return false

        return false;
    }

    bool PerformCastling(Tile tile)
    {
        //tile will be one left of right rook or two right of left rook
        //Ensure there are empty tiles on the way there
        //Ensure there are no enemies attacking the empty tiles
        //Ensure king and chosen rook haven't moved all game
        int distToRookTile = tile.pos.x + 1 - pos.x; //Castling would be triggered by moving king two tiles right or two tiles left
//        if (!moved && (tile.pos.x + 1 - pos.x == 3 || distToRookTile == 4) && tile.piece)

            return false;
    }

    public override List<Tile> PossibleMoves()
    {
        return null;
        return new List<Tile> {
            board.TileAt(pos + new Vector2Int(1, 1)), board.TileAt(pos + new Vector2Int(1, 0)), board.TileAt(pos + new Vector2Int(1, -1)), board.TileAt(pos + new Vector2Int(0, -1)),
            board.TileAt(pos + new Vector2Int(-1, -1)), board.TileAt(pos + new Vector2Int(-1, 0)), board.TileAt(pos + new Vector2Int(-1, 1)), board.TileAt(pos + new Vector2Int(0, 1)) };
    }



    /*
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

    */
}