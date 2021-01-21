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
    public bool isOnBoard;

    public Board board; //This can be made into a singleton
    [HideInInspector]
    public King king;
    [HideInInspector]
    public King opposingKing;

    [HideInInspector]
    public Pawn isAPawn;
    [HideInInspector]
    public Rook rook;
    [HideInInspector]
    public Queen queen;
    [HideInInspector]
    public Bishop bishop;
    [HideInInspector]
    public Knight knight;
    [HideInInspector]
    public King isAKing;

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();

        isAPawn = GetComponent<Pawn>();
        rook = GetComponent<Rook>();
        knight = GetComponent<Knight>();
        bishop = GetComponent<Bishop>();
        queen = GetComponent<Queen>();
        isAKing = GetComponent<King>();
    }

    public void Start()
    {
        if (isWhite)
        {
            king = board.wKing;
            opposingKing = board.bKing;
        }
        else
        {
            king = board.bKing;
            opposingKing = board.wKing;
        }
    }

    public virtual void DeterminePossibleActions()
    {
        //
    }

    public bool CanPieceMoveAtBoardCoords(Vector2Int testBoardCoords)
    {
        for (int i = 0; i < allowedDestinations.Count; i++)
        {
            if (allowedDestinations[i] == testBoardCoords)
            {
//                Debug.Log(i + ", " + allowedDestinations[i] + ", " + testBoardCoords);
                return true;
            }
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

    public bool WillMovingPiecePutKingInCheck(Vector2Int oldBoardCoords, Vector2Int testBoardCoords)
    {
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

    public virtual void CheckIfMovingPutOpposingKingOnCheck()
    {
        //refer to piece's boardcoords and possible actions to see if a check is happening

        //Need logic that makes a line with disappeared piece to see if any piece is placing king in check (e.g. a rook behind a bishop attacking the king)
        //..
        //..
        //..
        //I think I need a function to check if one piece puts check, and to refer to it when scanning through the opened line
        //e.g. a pawn will check its forward diagonals after movement to see if it puts the king in check
        //e.g. if a bishop moves and reveals a rook in line with the king, i need to call rook.IsCheckingOpposingKing

        CheckIfThisPieceChecksOpposingKing();

        //make a line from king to pastBoardCoords and call that piece's CheckKing()

        //from king's boardcoords to this.pastboardboords, fine

        //Need a bit more info than what I'm using now
        //For instance, I need a check to see if king is not in check anymore
        //Maybe King should hold a list of pieces that are checking it, and everytime movement occurs during a check, the list is checked to see if king'S still in check
        //If more than 1 piece are attacking the king at once, then the king is the only piece that can move

        //TRANSFER THIS TO LAPTOP SO I CAN WORK ON IT AT BUCK
    }

    public virtual void CheckIfThisPieceChecksOpposingKing()
    {
        //Each override will determine whether the opposing king's in attacking range of this piece
    }
    #endregion

    public virtual void DropOnBoard(Vector2Int posOnBoard)
    {
        bool canPieceMoveAtBoardCoords = CanPieceMoveAtBoardCoords(posOnBoard);
        //Check if piece has to be reset to original position

        //Check if piece can't be dropped at the given position:
        //  Where it can't move, outside the board, and where it started
        if (!canPieceMoveAtBoardCoords || posOnBoard.x < 0 || posOnBoard.x > 7 || posOnBoard.y < 0 || posOnBoard.y > 7 || posOnBoard == boardCoords)
        {
            Debug.Log("Piece reset");
            transform.position = board.UnityBoardCoordinates(boardCoords);
            return;
            //Reset piece and call return; to leave this function
        }

        //If movement is allowed and there's a piece in the way, then it must be eaten
        if (movingPiece.CanPieceMoveAtBoardCoords(testBoardCoords) && boardArray[testBoardCoords.x, testBoardCoords.y] != null && boardArray[testBoardCoords.x, testBoardCoords.y].isWhite != movingPiece.isWhite)
            EatPiece(boardArray[testBoardCoords.x, testBoardCoords.y]);


    }
}