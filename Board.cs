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
    delegate void GameStateDelegate();
    GameStateDelegate gameState;

    //    Action
    public delegate void NotifyPieceMoved(Piece p, Tile s, Tile e);
    public NotifyPieceMoved notifyPieceMoved;

    private List<string> trackedMoves = new List<string>();

    public Tile[] state = new Tile[64];
    public Piece grabbedPiece; //Piece being operated on
    public List<Piece> eatenPieces = new List<Piece>();
    public Tile startTile, wKTile, bKTile; //King tiles need to be updated (Pieces wouldn't have to, but I would still need to keep track of their pos)
    public Tile lWRookTile, rWRookTile, lBRookTile, rBRookTile;
    public Tile enPassantTile; //Needs to be reset as soon as a move is made after this has been turned on
    public SpriteRenderer[] promoSprites;

    public void Awake()
    {
        state = InitialBoardState();
        promoSprites = InitiatePawnPromotionObjects();

        gameState = PickAPiece;
    }

    private void Start()
    {
        //Define rooks references - Kings are initiated after Rooks, so there should be no problem relying on piece.isARook
        notifyPieceMoved += TrackMoves;
        notifyPieceMoved += lWRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled; //Castling isn't allowed after rook moves
        notifyPieceMoved += rWRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled;
        notifyPieceMoved += lBRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled;
        notifyPieceMoved += rBRookTile.piece.GetComponent<Rook>().CheckIfMovedAndIfCastled;
        notifyPieceMoved += wKTile.piece.GetComponent<King>().CheckIfMoved;
        notifyPieceMoved += bKTile.piece.GetComponent<King>().CheckIfMoved;
        notifyPieceMoved += wKTile.piece.GetComponent<King>().UpdateKingTile;
        notifyPieceMoved += bKTile.piece.GetComponent<King>().UpdateKingTile;
        for (int i = 0; i < 64; i++)
        {
            if (state[i].piece != null && state[i].piece.GetComponent<Pawn>() != null)
                notifyPieceMoved += state[i].piece.GetComponent<Pawn>().EnPassantCheck;
        }
        //Maybe these can be part of the InitiatePiece functions, with a complete reset of the notifier delegate beforehand

        //**********DON't FORGET THAT THESE HAVE TO BE RESET AS WELL WHEN RE-INITILIAZING A BOARD

        //
        //THESE DELEGATES WORK, BUT NOW THE CASTLING HANDLING FUNCTION FAILS AS IT NEEDS KING TO HAVE MOVED AND TO HAVE ITS BOOL moved BE FALSE, SO I NEED A DIFFERENT METHOD
        //MAYBE THE moved SHOULD BE RENAMED TO movedForTheFirstTime, IN WHICH CASE IF CASTLING OCCURRED, THEN GO AHEAD AND ACHIEVE THE MOVEMENT
        //MAYBE THE CASTLING LOGIC COULD BE PLACED IN THE NOTIFYPIECEMOVED KING/ROOK LISTENERS
        //

    }

    public void Update()
    {
        gameState();

        //NEED BASIC GAMEOVER WORKING
        //NEED PROPER ORDER - GIVE FREE-FOR-FALL OPTION, STANDARD OPTION (WHITE-BLACK TAKE TURNS), AND OPTION TO PLAY AGAINST A COMPUTER
        //ALLOW A COMPUTER TO PLAY (START AS BLACK, AND INCLUDE WHITE AFTER)
        //MINIMAL VIABLE PRODUCT WOULD INCLUDE computer playing
        //  Have to rework the move functions so that they can be controlled by a player, and by a computer (AI needs to handle mousePos and decision-making)
        //  gameState needs to take in player/AI flag, and screenPos input
        //part of cleaning code could be removing as many instances of != null as possible
        //NEXT - MAKE STATE FUNCTIONS ABSTRACT SO THAT AN AI CAN MAKE DECISIONS
    }

    public void PickAPiece()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) //Left click
        {
            Tile tile = TileAt(BoardUnits(mousePos));
            if (tile != null && tile.piece != null)
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
                Debug.Log("Could place here");

                notifyPieceMoved(grabbedPiece, startTile, endTile);

                if (endTile.piece != null) //Check for regular eating
                    DistributeEatenPieces(endTile);
                else if (grabbedPiece.isPawn != null && grabbedPiece.isPawn.enPassantPawn != null)// && endTile.pos.y == startTile.pos.y + 1 * (int)Mathf.Sign(endTile.pos.y - startTile.pos.y)) //Consider en passant eating
                {
                    Debug.Log("eat enpassant");
                    DistributeEatenPieces(TileAt(grabbedPiece.isPawn.enPassantPawn.pos));
                }
//                    HandleEnPassantLogic(startTile, endTile);

                    //I think I can put the en passant logic in a delegate - as soon as a pawn double moves, evey enemy pawn determines whether they are close enough to en passant eat
                    //When a pawn moves, see if pawn has a en passant flag, and turn it off accordingly


                grabbedPiece.transform.position = UnityUnits(endTile.pos); //Place grabbed piece into end tile
                endTile.piece = grabbedPiece;
                endTile.piece.pos = endTile.pos;
                startTile.piece = null;

                if (grabbedPiece.isPawn != null && (endTile.pos.y == 0 || endTile.pos.y == 7)) //Check pawn reached promotion line
                {
                    HandlePawnPromotionsMenuDisplay(grabbedPiece, true);
                    gameState = PromotePawn;
                }
                else //Drop piece and return to regular play
                    gameState = PickAPiece; //**Not sure how this helps, but it does (makes pieces less sticky when clicking mouseUp)

                IsGameOver();
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
                    IsGameOver(); //Promoting a pawn to a rook may checkmate or stalemate the opposing king
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

            state = InitialBoardState();

            //MAYBE INSTEAD OF REINITILIAZING, I CAN UNDO EVERY MOVE UP TO THE FIRST ONE

            gameState = PickAPiece;
        }
    }

    void HandlePawnPromotionsMenuDisplay(Piece p, bool reveal) //Reveal(true) or hide(false) pawn promotion options
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

    Tile[] InitialBoardState()
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

    //**
    //**
    //LOOK AT HANDLE EN PASSANT LOGIC - WANT TO REPLACE THE FLAGGING WITH A DELEGATE POSSIBLY. I THINK EATING HAS TO STAY IN DSITRIBUTEEATENPIECES, AND EATING ENPASSANT NEEDS TO BE ADDED
    //Subscribe each pawn to notify function, and unsubscribe when they move. When they double move, TileAt() two pieces beside, if a piece is there, subscribe to notify, and turn off and unsubscribe on the next loop, if en passant eating occurred or not
    //**
    //**

    void HandleEnPassantLogic(Tile s, Tile e)
    {
        if (s.piece.isPawn != null)
        {
            if (e == TileAt(new Vector2Int(s.pos.x, s.pos.y + 2 * (s.piece.isWhite ? 1 : -1)))) //If pawn double moved, it becomes a pawn that may be eaten en passant
                enPassantTile = e;

            //If pawn moved diagonally above the enPassantTile, then the piece at enPassantTile is eaten
            if (enPassantTile != null && enPassantTile.piece != null && (s.piece.pos == enPassantTile.pos - Vector2Int.right || s.piece.pos == enPassantTile.pos + Vector2Int.right))
            {
                if (e.pos == enPassantTile.pos + (s.piece.isWhite ? Vector2Int.up : Vector2Int.down) && s.piece.isWhite != enPassantTile.piece.isWhite)
                    DistributeEatenPieces(enPassantTile);
            }
        }
        //RESET EN PASSANT
        if (enPassantTile != null && enPassantTile.piece != null && s.piece.isWhite == enPassantTile.piece.isWhite)// && s.piece.isAPawn == null) //When a non-pawn piece is picked up on the next turn, the en passant possibility is nullified
            enPassantTile = null;
    }

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

    bool IsGameOver()
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

    void MoveRookForCastling(King king)
    {
        //Move rooks in front of king - do I determine this according to whether king moved left or right?
        int yVal = king.isWhite ? 0 : 7;
        int castlingDirection = king.pos.x - 4; //King has already moved at this point, so king.pos should be accurate

        Tile rookTile = (king.isWhite ? (castlingDirection < 0 ? lWRookTile : rWRookTile) : (castlingDirection < 0 ? lBRookTile : rBRookTile));
        Tile tileNextToKing = TileAt(new Vector2Int(king.pos.x + 1 * (int)Mathf.Sign(-castlingDirection), yVal));
        tileNextToKing.piece = rookTile.piece;
        tileNextToKing.piece.pos = tileNextToKing.pos;
        tileNextToKing.piece.transform.position = UnityUnits(tileNextToKing.piece.pos);
        rookTile.piece = null;
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