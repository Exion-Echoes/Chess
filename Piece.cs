using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool canBeMoved; //This will be determined sequentially whenever player tries to pick up a piece
    public bool isWhite; //If false, then piece is black
    public bool beingCarried; //State of piece when it's being held by player
    public Vector2Int boardCoords; //[0,0] = A1, [0,1] = A2, etc.
    public List<Vector2Int> allowedDestinations = new List<Vector2Int>();

    public Board board;

    public Pawn pawn;
    public Rook rook;
    public Queen queen;
    public Bishop bishop;
    public Knight knight;
    public King king;

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();

        pawn = GetComponent<Pawn>();
        rook = GetComponent<Rook>();
        knight = GetComponent<Knight>();
        bishop = GetComponent<Bishop>();
        queen = GetComponent<Queen>();
        king = GetComponent<King>();
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
                Debug.Log(i + ", " + allowedDestinations[i] + ", " + testBoardCoords);
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
        //Look for checks on the king of the same color as this piece
        //Check if simply moving the piece puts the king in check (no need to check anything else here)
        //The king will be in check from a piece along the path opened up by the piece being moved at testBoardCoords

        //Trace a line along the path opened up the piece and check board.boardArray for any pieces there, and check if they are attacking the king
        //In addition, if king is ALREADY in check, there are some potential moves that may block the check
        //summary: 
        //  1) function will stop pieces from moving and causing a check to their own king (will happen by default, if king is already in check and move doesn't fix that)
        //  2) function will allow blocking checks of their own king

        return false;
    }
    #endregion
}