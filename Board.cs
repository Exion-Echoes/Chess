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
    //Board is 120 x 120 and situated at (0, 0)

    public Piece[,] boardArray = new Piece[8, 8]; //Used to manage piece-piece interactions
    public Piece movingPiece;
    
    //Board highlights of which piece last moved and where a picked piece can move
    GameObject[] destinationTraces = new GameObject[9];
    GameObject[] travelTraces = new GameObject[8];
    Vector3 mousePos;

    private void Awake()
    {
        InitiateBoardArray();
        InitiateMoveTraces();
    }

    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        CheckForMouseClick();

        CarryAPiece();
    }

    void CheckForMouseClick()
    {
        Vector2Int locationOnBoard = BoardCoordinates(mousePos);
//        Debug.Log(locationOnBoard);
        Piece piece = null;

        if (locationOnBoard.x >= 0 && locationOnBoard.x <= 7 && locationOnBoard.y >= 0 && locationOnBoard.y <= 7) //Have to limit these to not get an out of reach exception
            piece = boardArray[locationOnBoard.x, locationOnBoard.y];

        if (piece != null)
        {
            int ayylmao = 0;
            for(int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    if (boardArray[i, j] != null)
                        ayylmao++;
                }

            }
//            Debug.Log("ayy lmao: " + ayylmao);
//            Debug.Log(piece);
//            piece.Rule();

            if (Input.GetMouseButtonDown(0)) //Left click
            {
                Debug.Log("Grabbed " + piece);
                movingPiece = piece;
                piece.beingCarried = true;
            }
        }
    }

    void CarryAPiece()
    {
        if (movingPiece != null)
        {
            movingPiece.transform.position = new Vector3(mousePos.x, mousePos.y, -1); //Make the piece move along the cursor

            if(Input.GetMouseButtonUp(0)) //Drop piece onto board if it's in an allowed place, otherwise reset to original position
            {
                Debug.Log("LET GO of " + movingPiece);
                Vector2Int testBoardCoords = BoardCoordinates (mousePos); //Where piece is dropped
                Debug.Log(testBoardCoords);

                Piece testPiece = null;
                if (testBoardCoords.x >= 0 && testBoardCoords.x <= 7 && testBoardCoords.y >= 0 && testBoardCoords.y <= 7)
                    testPiece = boardArray[testBoardCoords.x, testBoardCoords.y];

                //Need a test for whether the move is allowed

                bool resetPiece = false;
                if (testBoardCoords.x < 0 || testBoardCoords.x > 7 || testBoardCoords.y < 0 || testBoardCoords.y > 7) //Check if piece is dropped out of bounds
                    resetPiece = true;

                if (testBoardCoords == movingPiece.boardCoords) //Check if piece is dropped in the same square it started
                    resetPiece = true;

                if (resetPiece) //if movement is not allowed - NOT SUFFICIENT, JUST TEST
                {
                    movingPiece.transform.position = UnityBoardCoordinates(movingPiece.boardCoords); //Reset piece to original position
                }

                else if (testPiece == null) //if movement is allowed - NOT SUFFICIENT, JUST TEST
                {
                    boardArray[movingPiece.boardCoords.x, movingPiece.boardCoords.y] = null; //Remove old piece from board array
                    movingPiece.transform.position = UnityBoardCoordinates(testBoardCoords); //Snap new piece to board
                    movingPiece.boardCoords = testBoardCoords;
                    boardArray[testBoardCoords.x, testBoardCoords.y] = movingPiece; //Add new piece to board array
                }

                movingPiece.beingCarried = false;
                movingPiece = null;
            }
        }
    }

    public void DisplayTraces(Vector2Int[] destinationTraces, Vector2Int[] travelTraces)
    {
        for (int i = 0; i < destinationTraces.Length; i++)
            this.destinationTraces[i].transform.position = UnityBoardCoordinates(destinationTraces[i]);
    }

    public Vector2Int BoardCoordinates(Vector3 pos) //Translates Unity units into board coordinates
    {
        int i = (int)(pos.x + 120) / 30;
        int j = (int)(pos.y + 120) / 30;

        return new Vector2Int(i, j);
    }
    Vector3 UnityBoardCoordinates(Vector2Int boardCoords) //Translates board coords (A1, A2, etc) to Unity units
    {
        return new Vector3(-105 + boardCoords.x * 30, -105 + boardCoords.y * 30, -1);
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
            destinationTraces[i].transform.localScale = new Vector3(30, 30, 1);
            destinationTraces[i].transform.position = new Vector3(0, 0, 1);
        }
        for (int i = 0; i < travelTraces.Length; i++)
        {
            travelTraces[i] = new GameObject();
            travelTraces[i].AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("TravelTrace");
            travelTraces[i].name = "TravelTrace";
            travelTraces[i].transform.localScale = new Vector3(30, 30, 1);
            travelTraces[i].transform.position = new Vector3(0, 0, 1);
        }
    }
    #endregion

}