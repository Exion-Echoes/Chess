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

    public Board board;
    [HideInInspector]
    public King king;
    [HideInInspector]
    public King opposingKing;

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

    public virtual void DropOnBoard(Vector2Int dropPos)
    {
        bool canPieceMoveAtBoardCoords = CanPieceMoveAtBoardCoords(dropPos);
        Piece pieceAtPos = board.boardArray[dropPos.x, dropPos.y];
        bool enPassantAllowed = isAPawn != null && isAPawn.enPassantPawn != null;

        //Check if piece can't be dropped at the given position: (1) where it can't move, (2) outside the board, (3) and where it started
        if (!canPieceMoveAtBoardCoords || dropPos.x < 0 || dropPos.x > 7 || dropPos.y < 0 || dropPos.y > 7 || dropPos == boardCoords)
            transform.position = board.UnityBoardCoordinates(boardCoords);

        //Eat piece if there's an enemy piece in the pos
        else if (canPieceMoveAtBoardCoords && (pieceAtPos != null || enPassantAllowed)) //If piece can move on the piece, then the latter must be an enemy's// && pieceAtPos.isWhite != isWhite)
        {
            if (pieceAtPos != null)
                board.EatPiece(pieceAtPos);
            else if (enPassantAllowed)
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
                    Debug.Log("Pawn promotion happens now");
            }

            board.UpdatePieceOnBoard(dropPos);
        }
    }
}