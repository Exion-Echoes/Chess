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

//    public Piece[] pieces = new Piece[32];
    public Piece[,] boardArray = new Piece[8, 8]; //Used to manage piece-piece interactions
    public Piece pieceBeingMoved;
    
    //Board highlights of which piece last moved and where a picked piece can move
    GameObject[] destinationTraces = new GameObject[9];
    GameObject[] travelTraces = new GameObject[8];
    Vector3 mousePos;

    private void Awake()
    {
        InitiateBoardArray(); //Gather all pieces and place them in the list
        InitiateMoveTraces();

        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
//                Debug.Log((i + 1) + ", " + (j + 1) + ", " + boardArray[i, j]);
            }
        }
    }

    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        MouseHover();

        CarryAPiece();
    }

    void MouseHover()
    {
        Vector2Int locationOnBoard = BoardCoordinates(mousePos);
        //        Debug.Log(locationOnBoard);
        Piece piece = null;

        if (locationOnBoard.x >= 1 && locationOnBoard.x <= 8 && locationOnBoard.y >= 1 && locationOnBoard.y <= 8)
            piece = boardArray[locationOnBoard.x - 1, locationOnBoard.y - 1];

        if (piece != null)
        {
//            Debug.Log(piece);
//            piece.Rule();

            if (Input.GetMouseButtonDown(0)) //Left click
            {
                Debug.Log("Grabbed " + piece);
                pieceBeingMoved = piece;
                piece.beingCarried = true;
            }
        }
    }

    void CarryAPiece()
    {
        if (pieceBeingMoved != null)
        {
            pieceBeingMoved.transform.position = new Vector3(mousePos.x, mousePos.y, -1); //Make the piece move in the view screen

            if(Input.GetMouseButtonUp(0)) //Drop piece onto board if it's in an allowed place, otherwise reset to original position
            {
                Debug.Log("LET GO of " + pieceBeingMoved);
                Vector2Int boardSquareToTest = BoardCoordinates (mousePos);
                Debug.Log(boardSquareToTest);
                
                Piece pieceAtTestedSquare = boardArray[boardSquareToTest.x, boardSquareToTest.y];
                if (pieceAtTestedSquare != null)
                {
                    //if can't move there, return piece to original position
                    pieceBeingMoved.transform.position = UnityBoardCoordinates(pieceBeingMoved.posOnBoard);
                    //No need to update board array
                }
                else
                {
                    Debug.Log(boardArray[pieceBeingMoved.posOnBoard.x - 1, pieceBeingMoved.posOnBoard.y - 1]);
                    boardArray[boardSquareToTest.x - 1, boardSquareToTest.y - 1] = pieceBeingMoved;
                    boardArray[pieceBeingMoved.posOnBoard.x - 1, pieceBeingMoved.posOnBoard.y - 1] = null;
                    Debug.Log(boardArray[pieceBeingMoved.posOnBoard.x - 1, pieceBeingMoved.posOnBoard.y - 1]);
                    pieceBeingMoved.posOnBoard = boardSquareToTest;
                    pieceBeingMoved.transform.position = UnityBoardCoordinates(boardSquareToTest);
                }

                pieceBeingMoved.beingCarried = false;
                pieceBeingMoved = null;
            }
        }
    }

    public void DisplayTraces(Vector2Int[] destinationTraces, Vector2Int[] travelTraces)
    {
        for (int i = 0; i < destinationTraces.Length; i++)
            this.destinationTraces[i].transform.position = UnityBoardCoordinates(destinationTraces[i]);
    }

    public Vector2Int BoardCoordinates(Vector3 pos)
    {
//        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition); use this later to allign mouse cursor with board pieces

        //Board coordinates: i,j = [1,8]
        int i = (int)(pos.x + 120) / 30 + 1;
        int j = (int)(pos.y + 120) / 30 + 1;

        return new Vector2Int(i, j);
    }

    #region INITIATION FUNCTIONS
    void DefinePiece(ref Piece piece, bool isWhite, Vector3 position, string name, Sprite sprite, Vector2Int boardCoord)
    {
        piece.isWhite = isWhite;
        piece.posOnBoard = BoardCoordinates(position);
        piece.transform.position = position;
        piece.name = name;
        piece.gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
        boardArray[piece.posOnBoard.x - 1, piece.posOnBoard.y - 1] = piece;
    }

    void DefinePieceTEST<T>(Vector2Int boardCoords, bool isWhite, string name, Sprite sprite) where T : Piece
    {
        boardArray[boardCoords.x, boardCoords.y] = new GameObject().AddComponent<T>();
        boardArray[boardCoords.x, boardCoords.y].isWhite = isWhite;
        boardArray[boardCoords.x, boardCoords.y].transform.position = UnityBoardCoordinates(boardCoords);
        boardArray[boardCoords.x, boardCoords.y].name = name;
        boardArray[boardCoords.x, boardCoords.y].gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
    }

    void InitiateBoardArray()
    {
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        //Place Pawns
        for (int i = 0; i < 8; i++)
        {
            DefinePieceTEST<Pawn>(new Vector2Int(i, 1), true, "wPawn_" + i.ToString(), pieceSprites[11]);
            DefinePieceTEST<Pawn>(new Vector2Int(i, 6), false, "bPawn_" + i.ToString(), pieceSprites[5]);
            //          pieces[i] = new GameObject().AddComponent<Pawn>();
            //            DefinePiece(ref pieces[i], true, UnityBoardCoordinates(i + 1, 2), "wPawn_" + i.ToString(), pieceSprites[11], new Vector2Int(1 + i, 2));
            //            boardArray[i, 1] = new GameObject().AddComponent<Pawn>();

            //boardArray[i, 6] = new GameObject().AddComponent<Pawn>();

            //      pieces[i + 8] = new GameObject().AddComponent<Pawn>();
            //        DefinePiece(ref pieces[8 + i], false, UnityBoardCoordinates(i + 1, 7), "bPawn_" + i.ToString(), pieceSprites[5], new Vector2Int(1 + i, 7));
        }

        //Place Rooks
        for (int i = 0; i < 2; i++)
        {
//            pieces[16 + i] = new GameObject().AddComponent<Rook>();
//            DefinePiece(ref pieces[16 + i], true, UnityBoardCoordinates(1 + 7 * i, 1), "wRook_" + i.ToString(), pieceSprites[6], new Vector2Int(1 + 7 * i, 1));
//            pieces[18 + i] = new GameObject().AddComponent<Rook>();
//            DefinePiece(ref pieces[18 + i], false, UnityBoardCoordinates(1 + 7 * i, 8), "bRook_" + i.ToString(), pieceSprites[0], new Vector2Int(1 + 7 * i, 8));
        }

        //Place Knights
        for (int i = 0; i < 2; i++)
        {
//            pieces[20 + i] = new GameObject().AddComponent<Knight>();
//            DefinePiece(ref pieces[20 + i], true, UnityBoardCoordinates(2 + 5 * i, 1), "wKnight_" + i.ToString(), pieceSprites[7], new Vector2Int(2 + 5 * i, 1));
//            pieces[22 + i] = new GameObject().AddComponent<Knight>();
//            DefinePiece(ref pieces[22 + i], false, UnityBoardCoordinates(2 + 5 * i, 8), "bKnight_" + i.ToString(), pieceSprites[1], new Vector2Int(2 + 5 * i, 8));
        }

        //Place Bishops
        for (int i = 0; i < 2; i++)
        {
//            pieces[24 + i] = new GameObject().AddComponent<Bishop>();
//            DefinePiece(ref pieces[24 + i], true, UnityBoardCoordinates(3 + 3 * i, 1), "wBishop_" + i.ToString(), pieceSprites[8], new Vector2Int(3 + 3 * i, 1));
//            pieces[26 + i] = new GameObject().AddComponent<Bishop>();
//            DefinePiece(ref pieces[26 + i], false, UnityBoardCoordinates(3 + 3 * i, 8), "bBishop_" + i.ToString(), pieceSprites[2], new Vector2Int(3 + 3 * i, 8));
        }

        //Place Queens
//        pieces[28] = new GameObject().AddComponent<Queen>();
//        DefinePiece(ref pieces[28], true, UnityBoardCoordinates(4, 1), "wQueen", pieceSprites[9], new Vector2Int(4, 1));
//        pieces[29] = new GameObject().AddComponent<Queen>();
//        DefinePiece(ref pieces[29], false, UnityBoardCoordinates(4, 8), "bQueen", pieceSprites[3], new Vector2Int(4, 8));

        //Place Kings
//        pieces[30] = new GameObject().AddComponent<King>();
//        DefinePiece(ref pieces[30], true, UnityBoardCoordinates(5, 1), "wKing", pieceSprites[10], new Vector2Int(5, 1));
//        pieces[31] = new GameObject().AddComponent<King>();
//        DefinePiece(ref pieces[31], false, UnityBoardCoordinates(5, 8), "bKing", pieceSprites[4], new Vector2Int(5, 8));
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

    Vector3 UnityBoardCoordinates(Vector2Int boardCoords) //Translates board coords (A1, A2, etc) to Unity units
    {
        return new Vector3(-105 + boardCoords.x * 30, -105 + boardCoords.y * 30, -1);
    }
}