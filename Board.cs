using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public delegate void GameStateDelegate();
    public GameStateDelegate gameState;

    public delegate void NotifyPieceMoved(Piece p, Tile s, Tile e);
    public NotifyPieceMoved moveNotification;

    private List<string> trackedMoves = new List<string>();

    public Tile[] state = new Tile[64];
    public Piece grabbedPiece; //Piece being operated on
    public List<Piece> eatenPieces = new List<Piece>();
    List<SpriteRenderer> eatenSprites = new List<SpriteRenderer>();
    public Tile startTile, wKTile, bKTile; //King tiles need to be updated (Pieces wouldn't have to, but I would still need to keep track of their pos)
    public Tile lWRookTile, rWRookTile, lBRookTile, rBRookTile;
    public Tile enPassantTile; //Needs to be reset as soon as a move is made after this has been turned on
    public SpriteRenderer[] promoSprites;
    public bool turnSwapper;
    int moveCount;

    //Initiate two players and give them human or computer, depending on playerS choice (start with human = white and computer = black)
    Player[] p = new Player[2];
    DepthSearching depthSearch;

    public void Awake()
    {
        state = InitiateBoardState();
        InitiateMoveNotificationDelegate();
        turnSwapper = false;

        promoSprites = InitiatePawnPromotionObjects();

        p[0] = new GameObject().AddComponent<Human>();
//        p[0] = new GameObject().AddComponent<Computer>();
        p[0].isWhite = true;
        p[1] = new GameObject().AddComponent<Computer>();
//        p[1] = new GameObject().AddComponent<Human>();
        gameState = p[0].PickAPiece;

        //ply calculations
//        depthSearch = gameObject.AddComponent<DepthSearching>();
//        gameState = depthSearch.DepthSearch;
    }

    private void Start()
    {
        //
    }

    public void Update()
    {
        gameState();
    }

    void GameOver()
    {
        //Game state to display game over screen after a checkmate occurred or a stalemate

        //Need to let player navigate menu and click on options
        //A generalized mouse-hovering-button-clicking script might be nice here
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Start with option to reset board (call state = InitiateBoardState(), with possibility of inverting pieces?)
        if (Input.GetMouseButtonDown(0)) //Left click
        {
            Debug.Log("Reset the game");

            ResetBoard();

            //MAYBE INSTEAD OF REINITILIAZING, I CAN UNDO EVERY MOVE UP TO THE FIRST ONE

            gameState = p[0].PickAPiece;
        }
    }

    #region BOARD OPERATIONS
    public Tile TileAt(Vector2Int pos) //Look at current board and return tile
    {
        if (pos.x >= 0 && pos.x <= 7 && pos.y >= 0 && pos.y <= 7)
            return state[pos.x + 8 * pos.y];
        return null; //Null is used for OOB cases
    }

    public Vector3 UnityUnits(Vector2Int pos, float z = 0)
    {
        return new Vector3(-105 + pos.x * 30, -105 + pos.y * 30, -1 + z);
    }

    public Vector2Int BoardUnits(Vector3 pos)
    {
        if (pos.x >= -120 && pos.x <= 120 && pos.y >= -120 && pos.y <= 120)
            return new Vector2Int((int)(120 + pos.x) / 30, (int)(120 + pos.y) / 30);
        return new Vector2Int(-1, -1);
    }

    public Player SwitchPlayer (Player player) //Used to swap turns
    {
        if (player == p[0])
            return p[1];
        return p[0];
    }
    #endregion

    #region BOARD INITIATING/RESETING FUNCTIONS
    Tile[] InitiateBoardState()
    {
        Tile[] boardState = new Tile[64];
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        Vector2Int piecePos;
        piecePos = new Vector2Int(0, 0);
        lWRookTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[6], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(1, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[7], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(2, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(3, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Queen>(), pieceSprites[9], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(5, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(6, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[7], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(7, 0);
        rWRookTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[6], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(4, 0);
        wKTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[10], UnityUnits(piecePos), true);
        for (int i = 0; i < 8; i++)
        {
            piecePos = new Vector2Int(i, 1);
            boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Pawn>(), pieceSprites[11], UnityUnits(piecePos), true);
        }
        piecePos = new Vector2Int(0, 7);
        lBRookTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[0], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(1, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[1], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(2, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(3, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Queen>(), pieceSprites[3], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(5, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(6, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[1], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(7, 7);
        rBRookTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[0], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(4, 7);
        bKTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[4], UnityUnits(piecePos), false);
        for (int i = 0; i < 8; i++)
        {
            piecePos = new Vector2Int(i, 6);
            boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Pawn>(), pieceSprites[5], UnityUnits(piecePos), false);
        }
        for (int i = 16; i < 48; i++)
        {
            piecePos = new Vector2Int(i % 8, i / 8);
            boardState[i] = new Tile(piecePos, null, null, UnityUnits(piecePos), false);
        }

        for (int i = 0; i < 64; i++) //Can be determined as a function of the tile position (to be changed later)
            boardState[i].id = i;
        return boardState;
    }

    void InitiateMoveNotificationDelegate()
    {
        moveNotification = null; //Make sure the delegate is empty

        moveNotification += TrackMoves;
        moveNotification += lWRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled; //Castling isn't allowed after rook moves
        moveNotification += rWRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled;
        moveNotification += lBRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled;
        moveNotification += rBRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled;
        moveNotification += wKTile.piece.GetComponent<King>().CheckIfMoved;
        moveNotification += bKTile.piece.GetComponent<King>().CheckIfMoved;
        moveNotification += wKTile.piece.GetComponent<King>().UpdateKingTile;
        moveNotification += bKTile.piece.GetComponent<King>().UpdateKingTile;
        for (int i = 0; i < 64; i++)
        {
            if (state[i].piece != null && state[i].piece.GetComponent<Pawn>() != null)
                moveNotification += state[i].piece.GetComponent<Pawn>().EnPassantCheck;
        }
    }

    SpriteRenderer[] InitiatePawnPromotionObjects() //Produce pawn promotion board and 4 pieces (with possibility of highlighting the one hovered over by the mouse cursor)
    {
        SpriteRenderer[] promotionObjects = new SpriteRenderer[9];
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");
        for (int i = 0; i < 9; i++)
        {
            promotionObjects[i] = new GameObject().AddComponent<SpriteRenderer>();
            promotionObjects[i].transform.position = new Vector3(0, 0, 1);
        }
        promotionObjects[0].sprite = Resources.Load<Sprite>("PromoBackground");
        promotionObjects[1].sprite = pieceSprites[9]; //Queen
        promotionObjects[2].sprite = pieceSprites[6]; //Rook
        promotionObjects[3].sprite = pieceSprites[7]; //Knight
        promotionObjects[4].sprite = pieceSprites[8]; //Bishop
        promotionObjects[5].sprite = pieceSprites[3]; //Queen
        promotionObjects[6].sprite = pieceSprites[0]; //Rook
        promotionObjects[7].sprite = pieceSprites[1]; //Knight
        promotionObjects[8].sprite = pieceSprites[2]; //Bishop

        return promotionObjects;
    }

    void ResetBoard()
    {
        Piece[] pieces = FindObjectsOfType<Piece>();
        for (int i = 0; i < pieces.Length; i++) //Destroy all pieces still on the board
            Destroy(pieces[i].gameObject);

        state = InitiateBoardState();
        InitiateMoveNotificationDelegate();
        turnSwapper = false;
        eatenPieces.Clear();
        for (int i = 0; i < eatenSprites.Count; i++) //Destroy displayed eaten pieces
            Destroy(eatenSprites[i].gameObject);
        eatenSprites.Clear();
        moveCount = 0;
    }
    #endregion

    public void HandlePawnPromotionsMenuDisplay(Piece p, bool reveal) //Reveal(true) or hide(false) pawn promotion options
    {
        if (reveal)
        {
            promoSprites[0].transform.position = UnityUnits(p.pos, -1f); //grabbedPiece (p) should have moved at this point, so p.pos should be accurate
            (p.isWhite ? promoSprites[1] : promoSprites[5]).transform.position = UnityUnits(p.pos, -3f) - new Vector3(45, 0, 0);
            (p.isWhite ? promoSprites[2] : promoSprites[6]).transform.position = UnityUnits(p.pos, -3f) - new Vector3(15, 0, 0);
            (p.isWhite ? promoSprites[3] : promoSprites[7]).transform.position = UnityUnits(p.pos, -3f) + new Vector3(15, 0, 0);
            (p.isWhite ? promoSprites[4] : promoSprites[8]).transform.position = UnityUnits(p.pos, -3f) + new Vector3(45, 0, 0);
        }
        else
        {
            for (int i = 0; i < 9; i++)
                promoSprites[i].transform.position = new Vector3(0, 0, 1);
        }
    }

    public void DistributeEatenPieces(Tile tile)
    {
        eatenPieces.Add(tile.piece);
        eatenSprites.Add(tile.piece.sr);
        Destroy(tile.piece);
        tile.piece = null;

        int wCount = 0;
        int bCount = 0;
        for (int i = 0; i < eatenPieces.Count; i++)
        {
            if (eatenPieces[i].isWhite)
                wCount++;
            else
                bCount++;
        }
        eatenPieces[eatenPieces.Count-1].transform.position = new Vector3((eatenPieces[eatenPieces.Count - 1].isWhite ? 150 : 180), 120 - (eatenPieces[eatenPieces.Count - 1].isWhite ? wCount : bCount) * 15, -1 - 0.01f * (eatenPieces[eatenPieces.Count - 1].isWhite ? wCount : bCount));
    }

    public void IsItGameOver()
    {
        bool IsEnemyKingChecked()
        {
            //Verify that king is attacked by grabbedPiece
            List<Tile> pieceAttacks = new List<Tile>();
            if (grabbedPiece.isPawn == null)
                pieceAttacks = grabbedPiece.PossibleMoves();
            else
                pieceAttacks = grabbedPiece.isPawn.Attacks();
            for (int i = 0; i < pieceAttacks.Count; i++)
            {
                if (pieceAttacks[i] == (grabbedPiece.isWhite ? bKTile : wKTile))
                    return true;
            }
            return false;
        }

        bool EnemyCanMove() //Look for possible moves that protect enemy king - return false as soon as one is found, otherwise it has to be checkmate
        {
            for (int i = 0; i < 64; i++)
            {
                if (state[i].piece != null && state[i].piece.isWhite != grabbedPiece.isWhite)
                {
                    List<Tile> possibleMoves = state[i].piece.PossibleMoves();
                    if (possibleMoves != null)
                    {
                        for (int j = 0; j < possibleMoves.Count; j++)
                        {
                            if (state[i].piece.CanMove(state[i], possibleMoves[j]))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        bool HaveThreeMovesBeenRepeated()
        {
            //**
            //**
            //**
            //**
            //Refer to TrackMoves(p, t, t) method
            //**
            //**
            //**
            //**
            return false;
        }

        bool HasADeadPositionBeenReached()
        {
            //Dead positions:
            //kings only
            //king vs king + bishop
            //king vs king + knight
            //king + bishop vs king + bishop where both bishops are of the same tile color

            //Would be faster to just keep two arrays stored throughout the game which get updated by the eating function
            List<Piece> p0Pieces = new List<Piece>();
            List<Piece> p1Pieces = new List<Piece>();
            p0Pieces.Add(p[0].isWhite ? wKTile.piece : bKTile.piece);
            p1Pieces.Add(p[1].isWhite ? wKTile.piece : bKTile.piece);
            for (int i = 0; i < 64; i++)
            {
                if(state[i].piece != null)
                {
                    if (state[i].piece.isWhite == p[0].isWhite && state[i].piece.isKing == null)
                        p0Pieces.Add(state[i].piece);
                    else if(state[i].piece.isWhite == p[1].isWhite && state[i].piece.isKing == null)
                        p1Pieces.Add(state[i].piece);
                }
            }

            if (p0Pieces.Count == 1 && p1Pieces.Count == 1) //First piece in the list is always the king, so a count of 1 means the kings are the only pieces left on the board
                return true;

            //One knight on each side - apparently this isn't a dead position
            else if (p0Pieces.Count == 2 && p0Pieces[1].isKnight != null && p1Pieces.Count == 2 && p1Pieces[0].isKnight != null)
                return true;

            //One bishop or one knight left on either side of the board
            else if(((p0Pieces.Count == 2 && (p0Pieces[1].isBishop != null || p0Pieces[1].isKnight != null)) && p1Pieces.Count == 1) || ((p1Pieces.Count == 2 && (p1Pieces[1].isBishop != null || p1Pieces[1].isKnight != null)) && p0Pieces.Count == 1))
                return true;

            //There are other situations where a dead position may be reached, such as the case where all pawns are blocking each other to the point that none can move
            //and they block the kings from crossing, and there is no piece that can ever target any of the enemy pawns
            //E.G. black pawns on black tiles blocking white pawns on white tiles, with white bishop on white tile (that can't reach white pawns) and black bishop on black tile (that can't reach white pawns)
            //For this I think I need to verify that the possible moves come from bishops, and that the bishop can never hit any of the pawns
            //this is a highly complicated scenario, that even chess.com doesn't do well, so maybe I should avoid trying to figure it out

            return false;
        } //Find whether state is unwinnable

        bool hasADeadPositionBeenReached = HasADeadPositionBeenReached();
        bool haveThreeMovesBeenRepeated = HaveThreeMovesBeenRepeated();
        bool moveCountCapReached = moveCount >= 500; //STANDARDS ARE 50, BUT FOR COMPUTER VS COMPUTER SHENANIGANS, IT'S MORE ENTERTAINING TO HAVE A HIGH CAP
        bool enemyCanMove = EnemyCanMove(); //Look for checkmate/stalemate
        bool isEnemyKingChecked = IsEnemyKingChecked();

        if(haveThreeMovesBeenRepeated || hasADeadPositionBeenReached || moveCountCapReached)
        {
            if (haveThreeMovesBeenRepeated)
                Debug.Log("Stalemate - 3 moves repeated");
            else if (hasADeadPositionBeenReached)
                Debug.Log("Stalemate - game state cannot be won");
            else if (moveCountCapReached)
                Debug.Log("Stalemate - move count exceeded " + moveCount.ToString());
            Debug.Log("moveCount: " + moveCount);
            gameState = GameOver;
            return;
        }

        if(!enemyCanMove)
        {
            if (!isEnemyKingChecked)
            {
                Debug.Log("Stalemate - Enemy can't move");
                gameState = GameOver;
            }
            else
            {
                Debug.Log("Checkmate");
                gameState = GameOver;
            }
        }
    }

    void TrackMoves(Piece p, Tile s, Tile e) //
    {
        #region CONVERT INT TO STRING
        string Convert(int i)
        {
            switch (i)
            {
                case (0):
                    return "a";
                case (1):
                    return "b";
                case (2):
                    return "c";
                case (3):
                    return "d";
                case (4):
                    return "e";
                case (5):
                    return "f";
                case (6):
                    return "g";
                case (7):
                    return "h";
                default:
                    return null;
            }
        }
        #endregion

        //        trackedMoves.Add((p.isWhite ? "w_" : "b_") + p.IdentifyThis() + Convert(e.pos.x) + (e.pos.y + 1).ToString());

        string special = "";
        if (e.piece != null) //It means the piece ate the one at this tile
            special = " x ";
        //        if (p.isPawn != null &&) //Need notifier for pawn promotion
        //          special = "p=P";
        if (p.pawn && p.isPawn == null)
            Debug.Log("Pawn promotion occurred");
  //maybe keep pawn script so it can be detected, or maybe ispawn stays true?
    //if castling
    //O-O
    //special
        //if (castling)

        //NEED PAWN PROMOTION AND CASTLING SPECIAL

        string addition = (p.isWhite ? "w " : "b ") + p.IdentifyThis() + special + Convert(e.pos.x) + (e.pos.y + 1).ToString();
        Debug.Log(addition);

        moveCount++;
    }
}
