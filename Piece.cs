using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool canBeMoved; //This will be determined sequentially whenever player tries to pick up a piece
    public bool isWhite; //If false, then piece is black
    public bool beingCarried; //State of piece when it's being held by player
    public Vector2Int boardCoords; //[0,0] = A1, [0,1] = A2, etc.
    public Vector2Int pastBoardCoords;
    public List<Vector2Int> allowedDestinations = new List<Vector2Int>();
    public List<Vector2Int> positionsDefended = new List<Vector2Int>(); //Friendly pieces this piece is attacking
    public bool isOnBoard;

    public Board board;
    [HideInInspector]
    public King king;
    [HideInInspector]
    public King enemyKing;

    [HideInInspector]
    public Pawn isAPawn;
    [HideInInspector]
    public Rook isARook;
    [HideInInspector]
    public Queen isAQueen;
    [HideInInspector]
    public Bishop isABishop;
    [HideInInspector]
    public Knight isAKnight;
    [HideInInspector]
    public King isAKing;

    //TEST

    //
    //

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();

        isAPawn = GetComponent<Pawn>();
        isAKing = GetComponent<King>();

        //Might not need these ones below - in which case i can delete them
        isARook = GetComponent<Rook>();
        isAKnight = GetComponent<Knight>();
        isABishop = GetComponent<Bishop>();
        isAQueen = GetComponent<Queen>();
    }

    public virtual void Start()
    {
        king = isWhite ? board.wPieces[0].isAKing : board.bPieces[0].isAKing;
        enemyKing = isWhite ? board.bPieces[0].isAKing : board.wPieces[0].isAKing;
    }

    public virtual void InitialAllowedDestinations()
    {
        //
    }

    public bool IsThereAnEnemy(Vector2Int pos)
    {
        Piece piece = board.GetBoardPiece(pos);
        if (piece != null && piece.isWhite != isWhite)
            return true;
        return false;
    }

    public bool IsThereAnAlly(Vector2Int pos)
    {
        Piece piece = board.GetBoardPiece(pos);
        if (piece != null && piece.isWhite == isWhite)
            return true;
        return false;
    }

    public bool IsThisOOB(Vector2Int coords) //Are given coordinates out of bounds, i.e. not within {[0,7] , [0,7]}?
    {
        if (coords.x < 0 || coords.x > 7 || coords.y < 0 || coords.y > 7)
            return true;
        return false;
    }

    public bool IsOwnKingChecked(Vector2Int testPos) //Determines whether King is checked after testPos is emptied
    {
        Vector2Int kingPos = board.wPieces[0].boardCoords; //Check in this direction for potential checks from the enemy

        int travelDistance = 0;
        Vector2Int testDirection = Vector2Int.zero;

        //Check if revealing the king would check it
        bool EnemyInTheWay (bool checkQueen = false, bool checkBishop = false, bool checkRook = false)
        {
            bool IsAQueen(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isAQueen != null)
                    return true;
                return false;
            }
            bool IsARook(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isARook != null)
                    return true;
                return false;
            }
            bool IsABishop(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isABishop != null)
                    return true;
                return false;
            }

            for (int i = 1; i <= travelDistance; i++)
            {
                Vector2Int testBoardCoords = boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
                Piece piece = board.GetBoardPiece(testBoardCoords);
                if (IsThereAnEnemy(testBoardCoords) && (IsAQueen(checkQueen, piece) || IsARook(checkRook, piece) || IsABishop(checkBishop, piece)))
                    return true;

                else if (IsThereAnAlly(testBoardCoords)) //This lets the function exit the for loop earlier sometimes, but otherwise isn't necessary
                    return false;
            }
            return false;
        }

        if (Mathf.Abs(boardCoords.x - kingPos.x) == Mathf.Abs(boardCoords.y - kingPos.y) && boardCoords.x != kingPos.x && boardCoords.y != kingPos.y)
        {
            travelDistance = Mathf.Abs(boardCoords.x - kingPos.x);
            testDirection = new Vector2Int((int) Mathf.Sign(boardCoords.x - kingPos.x), (int) Mathf.Sign(boardCoords.y - kingPos.y));
            return EnemyInTheWay(true) || EnemyInTheWay(false, true); //If a Queen or a Bishop attacks, return true
        }
        else if (boardCoords.x == kingPos.x)
        {
            travelDistance = Mathf.Abs(boardCoords.y - kingPos.y);
            testDirection = new Vector2Int(0, (int)Mathf.Sign(boardCoords.y - kingPos.y));
            return EnemyInTheWay(true) || EnemyInTheWay(false, false, true); //If a Queen or a Rook attacks, return true
            //Vertical/Horizontal check - rooks and queen
        }
        else if (boardCoords.y == kingPos.y)
        {
            travelDistance = Mathf.Abs(boardCoords.x - kingPos.x);
            testDirection = new Vector2Int((int)Mathf.Sign(boardCoords.x - kingPos.x), 0);
            return EnemyInTheWay(true) || EnemyInTheWay(false, false, true); //If a Queen or a Rook attacks, return true
        }


        //Check if moving to testPos stops a check
        //
        //
        //

        if (Mathf.Abs(boardCoords.x - kingPos.x) == Mathf.Abs(boardCoords.y - kingPos.y) && boardCoords.x != kingPos.x && boardCoords.y != kingPos.y)
        {
            //Diagonal check - bishops and queen
            //IsThereAnAlly() and IsThereAnEnemy() at each spot
        }
        else if (boardCoords.x == kingPos.x || boardCoords.y == kingPos.y)
        {
            //Vertical/Horizontal check - rooks and queen
        }

        //Check if opening up pastPos would make the king checked, in which case return true

        //Check if moving to pos would stop the king from being checked, in which case return false

        //MAKE KING MOVEMENT DETERMINATIONS
        //CONSIDER THE CHECK CASE DESCRIBED BELOW
        //ADD ISOWNKINGCHECKED FUNCTION SO IT WORKS PROPERLY

        //The case where the king is the piece that checks if it moves into a checked position is resolved in the King script
        //...Every other piece's allowed destination is determined before the king's, so the allowed destinations may be referenced along with the king's test positions
        return false;
    }

    public virtual void UpdateAllowedDestinations()
    {
        //Check for allied / enemy pieces and check if moving at the test position would put own king in check
    }

    public virtual void DeterminePossibleActions()
    {
        //In addition to standard movement, this function forbids movement when it leads to own king being checked
    }

    public bool CanPieceMoveAtBoardCoords(Vector2Int testBoardCoords)
    {
        for (int i = 0; i < allowedDestinations.Count; i++)
        {
            if (allowedDestinations[i] == testBoardCoords)
                return true;
//            {
                //                Debug.Log(i + ", " + allowedDestinations[i] + ", " + testBoardCoords);
                //                return true;
  //          }
        }
        return false;
    }

    #region FUNCTIONS TO HELP DETERMINE ALLOWED DESTINATIONS
    public Piece CheckForAnEnemyPiece(Vector2Int testBoardCoords)
    {
        if (testBoardCoords.x >= 0 && testBoardCoords.x <= 7 && testBoardCoords.y >= 0 && testBoardCoords.y <= 7) //Have to limit these to not get an out of reach exception
        {
            if (board.boardArray[testBoardCoords.x, testBoardCoords.y] != null && board.boardArray[testBoardCoords.x, testBoardCoords.y].isWhite != this.isWhite)
                return board.boardArray[testBoardCoords.x, testBoardCoords.y];
        }
        return null;
    }

    public Piece CheckForAFriendlyPiece(Vector2Int testBoardCoords)
    {
        if (testBoardCoords.x >= 0 && testBoardCoords.x <= 7 && testBoardCoords.y >= 0 && testBoardCoords.y <= 7) //Have to limit these to not get an out of reach exception
        {
            if (board.boardArray[testBoardCoords.x, testBoardCoords.y] != null && board.boardArray[testBoardCoords.x, testBoardCoords.y].isWhite == this.isWhite)
                return board.boardArray[testBoardCoords.x, testBoardCoords.y];
        }
        return null;
    }

    public bool WillMovingPiecePutOwnKingInCheck(Vector2Int oldBoardCoords, Vector2Int testBoardCoords)
    {
        //Define a new board array with the this piece at the test position - It might be simpler to buffer this later on, than to define a 8x8 array each call
        //        Piece[,] testBoard = board.boardArray;

        //        testBoard[testBoardCoords.x, testBoardCoords.y] = this;
        //        testBoard[oldBoardCoords.x, oldBoardCoords.y] = null;

        //        List<Piece> enemyPieces = new List<Piece>(); //Might want to define this when the game is first ran, and then update them whenever a piece gets eaten
        /*
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
        //                if (testBoard[i, j] != null && testBoard[i, j].isWhite != isWhite)
        //                    enemyPieces.Add(testBoard[i, j]);
                    }
                }

                for(int i = 0; i < enemyPieces.Count; i++)
                {
        //            enemyPieces[i].DeterminePossibleActions();
        //This won't work, I still need to check possible actions after
                    for(int j = 0; j < enemyPieces[i].allowedDestinations.Count; j++)
                    {
        //                if (enemyPieces[i].allowedDestinations[j] == king.boardCoords)
        //                    return true; //Moving this piece will check own king
                    }
                }
        */
        //THESE ARE BIG OPERATIONS - CAN'T HAVE THIS IN FINAL VERSION - might just want to check pieces around king and long range pieces (bishop queen rook)

        //HAVE TO RESOLVE STACK OVERFLOW ISSUE - BECAUSE THIS FUNCTION's CALLED IN DETERMINE POSSIBLE ACTIONS, IT KEEPS LOOPING
        //HAVE TO FIND A WAY TO DETERMIUNE POSSIBLE ACTIONS BETTER


        //MAYBE I CAN PUT WILL MOVING PIECE AT THE VERY END, TO ERASE ANY ALLOWED DESTINATIONS THAT SHOULDN'T EXIST

        //*****THIS CAN BE SIMPLIFIED ACTUALLY - IT'S IMPOSSIBLE FOR A PAWN TO CHECK A CHECK AFTER ANOTHER PIECE MOVED, SAME FOR KNIGHTS
        //I ONLY HAVE TO CHECK IF ENEMY ROOKS, QUEEN, AND BISHOP CHECK OWN KING WHEN TRYING TO MOVE A PIECE
        //****CHECK IN 8 DIRECTIONS AROUND KING UNTIL A PIECE IS MET WITH CHECKFORFRIENDLY/ENEMY FUNCTIONS
        //****!!!!!!!!!!!!

//        Vector2Int kingPos = king.boardCoords;
        //Look for pieces in the direction of the space that just opened up
        //        float slope = (float)(kingPos.x - oldBoardCoords.x) / Mathf.Clamp((float)(kingPos.y - oldBoardCoords.y),1,8);
        //        bool kingMayBeCheckedFromSpace = Mathf.Abs((int)slope) == 0 || Mathf.Abs(slope) == 1;
        //If the value of the slope isn't 1 or 0, then

//        bool kingMayBeCheckedFromSpace = false;

        //Check horizontal
        if(king.boardCoords.y == oldBoardCoords.y)
        {
            int direction = (int) Mathf.Sign(oldBoardCoords.x - king.boardCoords.x);
            if(direction > 0) //Check right of king for friendly and enemy pieces
            {
                for(int i = king.boardCoords.x; i < 8; i++)
                {
                    Piece friend = CheckForAFriendlyPiece(new Vector2Int(i + 1, king.boardCoords.y));
                    Piece enemy = CheckForAnEnemyPiece(new Vector2Int(i + 1, king.boardCoords.y));
                    Debug.Log(i + ", " + friend + ", " + enemy);
                    if (friend != null && friend != this)
                        break; //There can be no checks from this direction, so move on to the other directions
                    else if (enemy != null && (enemy.isARook != null || enemy.isAQueen != null))
                        return true; //This movement cannot be allowed as it would put own king in check
                }
            }
        }
        else if (king.boardCoords.x == oldBoardCoords.y)
        {
            //
        }
        else
        {
            //Verify that it's a proper diagonal (45 degree angle vector from king to empty space)
        }

        //If moving the piece stops check on own king, then this returns false (this is also the only allowed movement during a check - if no such movements exist, then it's checkmate)

        //need to allow king to move out of check as well, this is currently forbidden by the code

        //If 

        //THIS IS SPECIFICALLY FOR THIS PIECE'S KING

        //Look for checks on the king of the same color as this piece
        //Check if simply moving the piece puts the king in check (no need to check anything else here)
        //The king will be in check from a piece along the path opened up by the piece being moved at testBoardCoords

        //Trace a line along the path opened up the piece and check board.boardArray for any pieces there, and check if they are attacking the king
        //In addition, if king is ALREADY in check, there are some potential moves that may block the check
        //summary: 
        //  1) function will stop pieces from moving and causing a check to their own king (will happen by default, if king is already in check and move doesn't fix that)
        //  2) function will allow blocking checks of their own king

        //if(king not checked, but checked after piece, return true)
        //if(king checked, and still checked after piece, return true)

        //        if(kingOfThisPiece.isChecked)

        return false;
    }

    public void DidMovingPutOpposingKingInCheck()
    {
        DeterminePossibleActions();
        for(int i = 0; i < allowedDestinations.Count; i++)
        {
            //If enemy king is an allowed destination after movement, that's a check
            if(allowedDestinations[i] == enemyKing.boardCoords)
            {
                enemyKing.isChecked = true;
                break;
            }
        }
    }
    #endregion

    public virtual void DropOnBoard(Vector2Int dropPos)
    {
        bool canPieceMoveAtBoardCoords = CanPieceMoveAtBoardCoords(dropPos);
        Piece pieceAtPos = board.PieceOnBoard(dropPos);
        bool enPassantAllowed = isAPawn != null && isAPawn.enPassantPawn != null;

        //Check if piece can't be dropped at the given position: (1) where it can't move, (2) outside the board, (3) and where it started
        if (!canPieceMoveAtBoardCoords || dropPos.x < 0 || dropPos.x > 7 || dropPos.y < 0 || dropPos.y > 7 || dropPos == boardCoords)
            transform.position = board.UnityBoardCoordinates(boardCoords);

        //Eat piece if there's an enemy piece in the pos
        else if (canPieceMoveAtBoardCoords && (pieceAtPos != null || enPassantAllowed)) //If piece can move on the piece, then the latter must be an enemy's// && pieceAtPos.isWhite != isWhite)
        {
            if (pieceAtPos != null)
                board.EatPiece(pieceAtPos);
            else if (enPassantAllowed && dropPos == allowedDestinations[allowedDestinations.Count - 1]) //2nd && prevents enPassant eating while not moving diagonally
                board.EatPiece(isAPawn.enPassantPawn);

            board.UpdatePieceOnBoard(dropPos);
            board.ResetEnPassantPawns(); //Must be called after a succesful action
        }

        //If no piece is eaten and movement is allowed
        else
        {
            //After a succesful action (which it will be, when making it here in the function), reset flags on pawns that could move en-passant, if there were any
            board.ResetEnPassantPawns();

            //If this piece is a pawn, have to check if we need to activate en passant flag or promotion flag
            if (isAPawn != null)
            {
                //If this pawn was on the starting line and doubled moved, turn on a flag in neighbouring enemy pawns to allow for eating en passant
                if (isAPawn.onStartingLine && allowedDestinations.Count > 1 && dropPos == allowedDestinations[1])
                    isAPawn.EnPassantFlags(dropPos);

                //If pawn is dropped at the final line
                else if (dropPos.y == 0 || dropPos.y == 7) //Only white and black pawns can be at pos.y = 7 and pos.y = 0 respectively
                    isAPawn.BeginPawnPromotion();
            }

            board.UpdatePieceOnBoard(dropPos);
        }
    }
}