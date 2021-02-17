using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Restarting project a second time (l0l):
 * state := playGame;
initialize position
print board
while (state != gameover)
   wait for input of a move by the user
   make move (if legal)
   if (mate or stalemate) {
      state := gameOver;
      break;
   }
   search move with allocated time
   make move
   print move and update board
   if (mate or stalemate)
      state := gameOver;
}
print "thank you for playing the game";
 * 
 */

//WANT TO GET THIS WORKING WITH PROPER TURN ORDER AND LOGIC IN "UGLY" CODE, THEN WANT TO MAKE IT MORE EVENT BASED AND CLEAN (MEANING CODES SHOULDN'T DEPEND ON ONE ANOTHER TOO MUCH)

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
    public Tile startTile, wKTile, bKTile; //King tiles need to be updated (Pieces wouldn't have to, but I would still need to keep track of their pos)
    public Tile lWRookTile, rWRookTile, lBRookTile, rBRookTile;
    public Tile enPassantTile; //Needs to be reset as soon as a move is made after this has been turned on
    public SpriteRenderer[] promoSprites;
    public bool turnSwapper;


    //***
    //***
    //Initiate two players and give them human or computer, depending on playerS choice (start with human = white and computer = black)
    //***
    //***
    Player[] p = new Player[2];


    public void Awake()
    {
        state = InitiateBoardState();
        InitiateMoveNotificationDelegate();
        turnSwapper = false;

        promoSprites = InitiatePawnPromotionObjects();

        p[0] = new GameObject().AddComponent<Human>();
        p[0].isWhite = true;
        p[1] = new GameObject().AddComponent<Computer>();
//        p[1] = new GameObject().AddComponent<Human>();
        gameState = p[0].PickAPiece;

        //This will be done by game setup when first booting the game and after a game's done
    }

    private void Start()
    {
        //
    }

    public void Update()
    {
        gameState();

        //NEED PROPER ORDER - GIVE FREE-FOR-FALL OPTION, STANDARD OPTION (WHITE-BLACK TAKE TURNS), AND OPTION TO PLAY AGAINST A COMPUTER
        //ALLOW A COMPUTER TO PLAY (START AS BLACK, AND INCLUDE WHITE AFTER)
        //MINIMAL VIABLE PRODUCT WOULD INCLUDE computer playing
        //  Have to rework the move functions so that they can be controlled by a player, and by a computer (AI needs to handle mousePos and decision-making)
        //  gameState needs to take in player/AI flag, and screenPos input
        //part of cleaning code could be removing as many instances of != null as possible
        //NEXT - MAKE STATE FUNCTIONS ABSTRACT SO THAT AN AI CAN MAKE DECISIONS
        //Need esc button to reset game anytime

        //Identify functions that need to be made abstract
        //  PickAPiece, DropAPiece, PromotePawn
        //Figure out how to make functions abstract
        //  Need to replace mousePos, or work from a list of potential actions, choosing one at random, with weights towards the center of the board and towards the king ?
        //Devise a basic AI that chooses from a list of potential movements
        //  PickAPiece - Pick from list of pieces and keep chosen one if it has possible moves, otherwise find another piece
        //      Maybe I can find all possible moves and favor those towards the center or towards the opposing king
        //  DropAPiece - Pick from possible movements of grabbed piece, and drop
        //  PromotePawn - Randomly pick from 0 to 3, to decide between the 4 options, perhaps with more weight towards the queen and rook?
        //I will need some sort of abstraction that can result in an AI picking from a list to a player picking a piece with the mouse
        //Maybe a class could extend into Computer and Player, which could hold definitions of the gameState functions
        //Then, as soon as amount of player and amount of AI is chosen, gameState and the notiufication delegates can be filled up accordingly
        //choosing player/ai set up defines gameState's first function, and white/black functions alternate
    }

    public void PickAPiece()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) //Left click
        {
            Tile tile = TileAt(BoardUnits(mousePos));
            if (tile != null && tile.piece != null && tile.piece.isWhite != turnSwapper)
            {
                startTile = tile;
                grabbedPiece = tile.piece;
                gameState = DropAPiece;
            }
        }
    }

    public void DropAPiece()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        grabbedPiece.transform.position = new Vector3(mousePos.x, mousePos.y, -1.5f);

        if (Input.GetMouseButtonUp(0)) //Let go of left click
        {
            Tile endTile = TileAt(BoardUnits(grabbedPiece.transform.position));
            //            Debug.Log(startTile.piece + ", " + startTile.id + ", " + endTile.piece);
            if (endTile != null && grabbedPiece.CanMove(startTile, endTile))
            {
                moveNotification(grabbedPiece, startTile, endTile); //Notify subscribers that the grabbedPiece moved from startTile to endTile

                //Handle eating the piece occupied by the endTile, or enPassant
                if (endTile.piece != null) //Check for regular eating
                    DistributeEatenPieces(endTile);
                else if (grabbedPiece.isPawn != null && grabbedPiece.isPawn.enPassantPawn != null && endTile.pos.x == grabbedPiece.isPawn.enPassantPawn.pos.x) //Consider en passant eating
                    DistributeEatenPieces(TileAt(grabbedPiece.isPawn.enPassantPawn.pos));

                //Place grabbed piece into end tile
                grabbedPiece.transform.position = UnityUnits(endTile.pos);
                endTile.piece = grabbedPiece;
                endTile.piece.pos = endTile.pos;
                startTile.piece = null;

                //Consider possible changes of game states
                if (grabbedPiece.isPawn != null && (endTile.pos.y == 0 || endTile.pos.y == 7)) //Pawn reached a promotion row
                {
                    HandlePawnPromotionsMenuDisplay(grabbedPiece, true); //Display submenu on top of the board
                    gameState = PromotePawn;
                }
                else //Drop piece and return to regular play
                    gameState = PickAPiece;

                IsItGameOver(); //See whether opposing king is checkmated, or game reached a stalemate
                turnSwapper = !turnSwapper;
            }
            else
            {
                grabbedPiece.transform.position = UnityUnits(startTile.pos); //Reset grabbed piece back to original position
                gameState = PickAPiece;
                Debug.Log("Could not place here");
            }
        }
    }

    public void PromotePawn()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        void ChangePawn(int i) //Turn grabbedPiece.isAPawn into a piece with a new logic
        {
            Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

            Tile tile = TileAt(grabbedPiece.pos);
            Destroy(tile.piece.gameObject);
            switch (i) // i = [1, 4] = white, i = [5,8] = black (queen, rook, knight, bishop)
            {
                case 1:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Queen>(), pieceSprites[9], UnityUnits(tile.pos), true);
                    break;
                case 2:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Rook>(), pieceSprites[6], UnityUnits(tile.pos), true);
                    break;
                case 3:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Knight>(), pieceSprites[7], UnityUnits(tile.pos), true);
                    break;
                case 4:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], UnityUnits(tile.pos), true);
                    break;
                case 5:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Queen>(), pieceSprites[3], UnityUnits(tile.pos), false);
                    break;
                case 6:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Rook>(), pieceSprites[0], UnityUnits(tile.pos), false);
                    break;
                case 7:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Knight>(), pieceSprites[1], UnityUnits(tile.pos), false);
                    break;
                case 8:
                    state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(tile.pos), false);
                    break;
                default:
                    break;
            }
        }

        //After choosing
        if (Input.GetMouseButtonDown(0)) //Left click
        {
            for(int i = 1; i <= 4; i++)
            {
                Vector3 promoPos = (grabbedPiece.isWhite ? promoSprites[i] : promoSprites[i + 4]).transform.position;
                if (mousePos.x > promoPos.x - 15 && mousePos.x < promoPos.x + 15 && mousePos.y > promoPos.y - 15 && mousePos.y < promoPos.y + 15)
                {
                    ChangePawn(grabbedPiece.isWhite ? i : i + 4);
                    grabbedPiece = state[grabbedPiece.pos.x + 8 * grabbedPiece.pos.y].piece;
                    HandlePawnPromotionsMenuDisplay(null, false);
                    gameState = PickAPiece;
                    IsItGameOver(); //Promoting a pawn to a rook may checkmate or stalemate the opposing king
                    break;
                }
            }
        }
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
            Piece[] pieces = FindObjectsOfType<Piece>();
            for (int i = 0; i < pieces.Length; i++)
                Destroy(pieces[i].gameObject);

            state = InitiateBoardState();
            InitiateMoveNotificationDelegate();
            turnSwapper = false;

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

    #region BOARD INITIATING FUNCTIONS
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
        eatenPieces[eatenPieces.Count-1].transform.position = new Vector3((eatenPieces[eatenPieces.Count - 1].isWhite ? 150 : 180), 120 - (eatenPieces[eatenPieces.Count - 1].isWhite ? wCount : bCount) * 15, -1);
    }

    public bool IsEnemyKingChecked()
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

    public bool EnemyCanMove() //Look for possible moves that protect enemy king - return false as soon as one is found, otherwise it has to be checkmate
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
//                        {
//                            Debug.Log(state[i].piece + ", " + state[i].pos + ", " + j + ", " + possibleMoves[j].pos);
//                            return true;
//                        }
                    }
                }
            }
        }
        return false;
    }

    public bool IsItGameOver()
    {
        bool enemyCanMove = EnemyCanMove(); //Look for checkmate/stalemate
        bool isEnemyKingChecked = IsEnemyKingChecked();
        if(!enemyCanMove)
        {
            if (!isEnemyKingChecked)
            {
                Debug.Log("Stalemate");
                gameState = GameOver;
            }
            else
            {
                Debug.Log("Checkmate");
                gameState = GameOver;
            }
        }
        return false;
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

        //NEED PAWN PROMOTION AND CASTLING SPECIAL

        string addition = (p.isWhite ? "w " : "b ") + p.IdentifyThis() + special + Convert(e.pos.x) + (e.pos.y + 1).ToString();
        Debug.Log(addition);

        //THIS FUNCTION CAN BE REPLACED BY A NOTIFY FUNCTION TO ACHIEVE SOME RULES - TRACKING THE MOVES, 
        //*******************DETERMINING WHETHER KING AND ROOK MOVED - I WILL USE IT FOR THIS ESPECIALLY
    }
}