using System.Collections;
using System.Collections.Generic;
using UnityEngine;
////Features to implement:
/// Sprites for pieces and board
/// Mouse script: 
///     Mouse location is refreshed whenever it moves
///     Call board script highlight function when Mouse hovers over a new square
///     Can pick up moveable pieces
/// Piece script:
///     Rule function that gives a list of tiles that have to be tested when trying to move
///     Keep track of squares that the piece went over when moving
/// Board script:
///     Function to highlight/unhighlight squares of the board
///     Function to remove piece from the board (and set it aside)
///     (DONE) Function to generate pieces and to load them and place them on board
public class Board : MonoBehaviour
{
    //Board is 120 x 120 and situated at (0, 0) - squares are 30 x 30

    public Piece[,] boardArray = new Piece[8, 8]; //Used to manage piece-piece interactions
    public Piece movingPiece;
    public List<Piece> eatenWhitePieces = new List<Piece>(); //REPLACE THIS BY BOOL - isOnBoard
    public List<Piece> eatenBlackPieces = new List<Piece>();
    public King wKing, bKing;
    public List<Pawn> pawnsThatMayEatEnPassant; //List is reset after each move

    //Board highlights of which piece last moved and where a picked piece can move
    GameObject[] destinationTraces = new GameObject[28];
    GameObject[] travelTraces = new GameObject[28];
    Vector3 mousePos;

    private void Awake()
    {
        InitiateBoardArray();
        InitiateMoveTraces();

        King[] kings = FindObjectsOfType<King>();
        for (int i = 0; i < 2; i++)
        {
            if (kings[i].isWhite)
                wKing = kings[i];
            else
                bKing = kings[i];
        }
    }

    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        CheckForMouseClick();

        CarryAPiece();
    }

    void CheckForMouseClick()
    {
        Vector2Int mousedOverBoardCoords = BoardCoordinates(mousePos);
//        Debug.Log(mousedOverBoardCoords);
        Piece piece = null;

        if (mousedOverBoardCoords.x >= 0 && mousedOverBoardCoords.x <= 7 && mousedOverBoardCoords.y >= 0 && mousedOverBoardCoords.y <= 7) //Have to limit these to not get an out of reach exception
            piece = boardArray[mousedOverBoardCoords.x, mousedOverBoardCoords.y];

        if (piece != null)
        {
            piece.DeterminePossibleActions();

            if (Input.GetMouseButtonDown(0)) //Left click
            {
                Debug.Log("Grabbed " + piece);
                movingPiece = piece;
                piece.beingCarried = true;
                DisplayTraces(piece);
            }
        }
    }

    void CarryAPiece()
    {
        if (movingPiece != null)
        {
            movingPiece.transform.position = new Vector3(mousePos.x, mousePos.y, -2); //Make the piece move along the cursor

            if(Input.GetMouseButtonUp(0)) //Drop piece onto board if it's in an allowed place, otherwise reset to original position
            {
                movingPiece.DropOnBoard(BoardCoordinates(mousePos));

//                CleanUpBoardDisplay();
                movingPiece.beingCarried = false;
                movingPiece.transform.position = new Vector3(movingPiece.transform.position.x, movingPiece.transform.position.y, -1);
                movingPiece.CheckIfMovingPutOpposingKingOnCheck();
                HideTraces(movingPiece);
                movingPiece = null;
            }
        }
    }

    //    public void DisplayTraces(Vector2Int[] destinationTraces, Vector2Int[] travelTraces)
    void DisplayTraces(Piece piece)
    {
        for (int i = 0; i < piece.allowedDestinations.Count; i++)
            travelTraces[i].transform.position = UnityBoardCoordinates(piece.allowedDestinations[i], .5f);

        destinationTraces[0].transform.position = UnityBoardCoordinates(piece.boardCoords, .5f);
    }

    void HideTraces(Piece piece)
    {
        for (int i = 0; i < piece.allowedDestinations.Count; i++)
            travelTraces[i].transform.position = new Vector3(0, 0, 1);

        destinationTraces[0].transform.position = new Vector3(0, 0, 1);
    }

    public Vector2Int BoardCoordinates(Vector3 pos) //Translates Unity units into board coordinates
    {
        //**if statements guarantee that clicks are only recognized when inside the board (would have to change "120" if board size changes)
        int i = -1;
        int j = -1;
        if(pos.x + 120 >= 0)
            i = (int)(pos.x + 120) / 30;
        if(pos.y + 120 >= 0)
            j = (int)(pos.y + 120) / 30;

        return new Vector2Int(i, j);
    }

    public Vector3 UnityBoardCoordinates(Vector2Int boardCoords, float z = 0) //Translates board coords (A1, A2, etc) to Unity units
    {
        return new Vector3(-105 + boardCoords.x * 30, -105 + boardCoords.y * 30, -1 + z);
    }

    #region INITIATION FUNCTIONS
    void DefinePiece<T>(Vector2Int boardCoords, bool isWhite, string name, Sprite sprite) where T : Piece
    {
        boardArray[boardCoords.x, boardCoords.y] = new GameObject().AddComponent<T>();
        boardArray[boardCoords.x, boardCoords.y].isWhite = isWhite;
        boardArray[boardCoords.x, boardCoords.y].transform.position = UnityBoardCoordinates(boardCoords);
        boardArray[boardCoords.x, boardCoords.y].name = name;
        boardArray[boardCoords.x, boardCoords.y].gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
        boardArray[boardCoords.x, boardCoords.y].boardCoords = boardCoords;
    }

    void InitiateBoardArray()
    {
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        //Place Pawns
        for (int i = 0; i < 8; i++)
        {
            DefinePiece<Pawn>(new Vector2Int(i, 1), true, "wPawn_" + i.ToString(), pieceSprites[11]);
            DefinePiece<Pawn>(new Vector2Int(i, 6), false, "bPawn_" + i.ToString(), pieceSprites[5]);
        }

        //Place Rooks
        for (int i = 0; i < 2; i++)
        {
            DefinePiece<Rook>(new Vector2Int(7 * i, 0), true, "wRook_" + i.ToString(), pieceSprites[6]);
            DefinePiece<Rook>(new Vector2Int(7 * i, 7), false, "bRook_" + i.ToString(), pieceSprites[0]);
        }

        //Place Knights
        for (int i = 0; i < 2; i++)
        {
            DefinePiece<Knight>(new Vector2Int(1 + 5 * i, 0), true, "wKnight_" + i.ToString(), pieceSprites[7]);
            DefinePiece<Knight>(new Vector2Int(1 + 5 * i, 7), false, "bKnight_" + i.ToString(), pieceSprites[1]);
        }

        //Place Bishops
        for (int i = 0; i < 2; i++)
        {
            DefinePiece<Bishop>(new Vector2Int(2 + 3 * i, 0), true, "wBishop_" + i.ToString(), pieceSprites[8]);
            DefinePiece<Bishop>(new Vector2Int(2 + 3 * i, 7), false, "bBishop_" + i.ToString(), pieceSprites[2]);
        }

        //Place Queens
        DefinePiece<Queen>(new Vector2Int(3, 0), true, "wQueen", pieceSprites[9]);
        DefinePiece<Queen>(new Vector2Int(3, 7), false, "bQueen", pieceSprites[3]);
        //Place Kings
        DefinePiece<King>(new Vector2Int(4, 0), true, "wKing", pieceSprites[10]);
        DefinePiece<King>(new Vector2Int(4, 7), false, "bKing", pieceSprites[4]);
    }

    void InitiateMoveTraces()
    {
        for (int i = 0; i < destinationTraces.Length; i++)
        {
            destinationTraces[i] = new GameObject();
            destinationTraces[i].AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("DestinationTrace");
            destinationTraces[i].name = "DestinationTrace";
            destinationTraces[i].transform.localScale = new Vector3(28, 28, 1);
            destinationTraces[i].transform.position = new Vector3(0, 0, 1);
        }
        for (int i = 0; i < travelTraces.Length; i++)
        {
            travelTraces[i] = new GameObject();
            travelTraces[i].AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("TravelTrace");
            travelTraces[i].name = "TravelTrace";
            travelTraces[i].transform.localScale = new Vector3(28, 28, 1);
            travelTraces[i].transform.position = new Vector3(0, 0, 1);
        }
    }
    #endregion

    public void EatPiece(Piece piece)
    {
        //**Would be nice to organize the eaten lists by value, and might need to manipulate z a bit to stack them nicely
        if (piece.isWhite)
        {
            piece.transform.position = new Vector3(-200, 100 - eatenWhitePieces.Count * 10);
            eatenWhitePieces.Add(piece);
        }
        else
        {
            piece.transform.position = new Vector3(200, 100 - eatenBlackPieces.Count * 10);
            eatenBlackPieces.Add(piece);
        }

        boardArray[piece.boardCoords.x, piece.boardCoords.y] = null;
    }

    public Piece PieceOnBoard(Vector2Int pos)
    {
        if(pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && pos.y <= 7)
            return boardArray[pos.x, pos.y];
        return null;
    }

    public void ResetEnPassantPawns()
    {
        for (int i = 0; i < pawnsThatMayEatEnPassant.Count; i++)
            pawnsThatMayEatEnPassant[i].enPassantPawn = null;
        pawnsThatMayEatEnPassant.Clear();
    }

    public void UpdatePieceOnBoard(Vector2Int testBoardCoords)
    {
        boardArray[movingPiece.boardCoords.x, movingPiece.boardCoords.y] = null; //Remove old piece from board array
        movingPiece.transform.position = UnityBoardCoordinates(testBoardCoords); //Snap new piece to board
        movingPiece.pastBoardCoords = movingPiece.boardCoords;
        movingPiece.boardCoords = testBoardCoords;
        boardArray[testBoardCoords.x, testBoardCoords.y] = movingPiece; //Add new piece to board array
    }
}