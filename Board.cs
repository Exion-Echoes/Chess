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
///     

//Restarting project: 
/*  Program using an event based system
 *      Board controls and pawn promotion controls should be a function called in and out (like delegate += pawn promotion, then -= after a choice is made)
 *  Allowed movements should be calculated only once, and updated whenever a new piece has moved
 *      Must be able to determine where one piece could move if another piece moved (so that I can detect cases where moving would put own king in check, or cases when moving king would put it out of check)
 * 
 * 
 */


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

    [HideInInspector]
    public bool ongoingPawnPromotion;
    [HideInInspector]
    public Pawn promotedPawn;
    [HideInInspector]
    public Piece[] promotionPieces = new Piece[8]; //Permanent pieces that only appear when someone promotes a pawn
    [HideInInspector]
    public SpriteRenderer promotionBackground;

    //TEST
    //NOTE: First elemental of each list is the King
    public List<Piece> wPieces = new List<Piece>();
    public List<Piece> bPieces = new List<Piece>();
    SpriteRenderer[] traces = new SpriteRenderer[60]; //Bit of an overkill, but have room for 30 destination traces and 30 travel traces
    SpriteRenderer[] pawnPromotions = new SpriteRenderer[8];
    SpriteRenderer pawnPromotionBackground;
    delegate void GameStateDelegate();
    GameStateDelegate gameState;// = RegularPlayState;
    public Piece[,] board = new Piece[8, 8];
    Piece grabbedPiece = null;
    public Piece GetBoardPiece(Vector2Int coords)
    {
        if(coords.x >= 0 && coords.x <= 7 && coords.y >= 0 && coords.y <= 7)
            return board[coords.x, coords.y];
        return null;
    }
    bool IsMoveAllowed(Vector2Int testPos)
    {
        for (int i = 0; i < grabbedPiece.allowedDestinations.Count; i++)
        {
            if (testPos == grabbedPiece.allowedDestinations[i])
                return true;
        }
        return false;
    }
    void EatPieceTEST(Piece eaten, Piece eater, bool enPassantEat = false) //Replace eaten piece with eater, and list former cleanly on the sides of the board
    {
        if (!enPassantEat)
            board[eaten.boardCoords.x, eaten.boardCoords.y] = grabbedPiece;
        else
        {
            board[eaten.boardCoords.x, eaten.boardCoords.y + (eater.isWhite ? 1 : -1)] = grabbedPiece;
            board[eaten.boardCoords.x, eaten.boardCoords.y] = null;
        }
        board[eater.pastBoardCoords.x, eater.pastBoardCoords.y] = null;

        if (eaten.isWhite)
        {
            eaten.transform.position = new Vector3(200, 100 - eatenWhitePieces.Count * 10);
            eatenWhitePieces.Add(eaten);
            wPieces.Remove(eaten);
        }
        else
        {
            eaten.transform.position = new Vector3(230, 100 - eatenBlackPieces.Count * 10);
            eatenBlackPieces.Add(eaten);
            bPieces.Remove(eaten);
        }
    }
    void UpdateEnPassantFlags()
    {
        //Reset all flags
        for (int i = 0; i < wPieces.Count; i++)
        {
            if (wPieces[i].isAPawn != null)
                wPieces[i].isAPawn.enPassantPawn = null;
        }

        for (int i = 0; i < bPieces.Count; i++)
        {
            if (bPieces[i].isAPawn != null)
                bPieces[i].isAPawn.enPassantPawn = null;
        }

        //Check if piece that just moved needs to turn on en passant flags
        if (grabbedPiece.isAPawn != null && Mathf.Abs(grabbedPiece.pastBoardCoords.y - grabbedPiece.boardCoords.y) == 2)
        {
            Piece leftPiece = GetBoardPiece(grabbedPiece.boardCoords - Vector2Int.right);
            Piece rightPiece = GetBoardPiece(grabbedPiece.boardCoords + Vector2Int.right);

            if (leftPiece != null && leftPiece.isAPawn != null && leftPiece.isWhite != grabbedPiece.isWhite)
                leftPiece.isAPawn.enPassantPawn = grabbedPiece.isAPawn;
            if (rightPiece != null && rightPiece.isAPawn != null && rightPiece.isWhite != grabbedPiece.isWhite)
                rightPiece.isAPawn.enPassantPawn = grabbedPiece.isAPawn;
        }
    }
    void UpdateAllowedDestinations()
    {
        for (int i = 1; i < wPieces.Count; i++)
            wPieces[i].UpdateAllowedDestinations();
        for (int i = 1; i < bPieces.Count; i++)
            bPieces[i].UpdateAllowedDestinations();
        //Update kings last, so that they can consider other pieces' allowed destinations
        wPieces[0].UpdateAllowedDestinations();
        bPieces[0].UpdateAllowedDestinations();
    }
    void IsEnemyKingChecked() //NOTE: will be able to remove argument when I add proper turn functionality
    {
        bool lastPlayedWasWhite = grabbedPiece.isWhite;
        for (int i = 1; i < (lastPlayedWasWhite ? wPieces.Count : bPieces.Count); i++)
        {
            for (int j = 0; j < (lastPlayedWasWhite ? wPieces[i].allowedDestinations.Count : bPieces[i].allowedDestinations.Count); j++)
            {
                if ((lastPlayedWasWhite ? wPieces[i].allowedDestinations[j] == bPieces[0].boardCoords : bPieces[i].allowedDestinations[j] == wPieces[0].boardCoords))
                {
                    if (lastPlayedWasWhite)
                        bPieces[0].isAKing.isChecked = true;
                    else
                        wPieces[0].isAKing.isChecked = true;

                    return;
                }
            }
        }
    }

    void SelectAPieceState()
    {
        if (Input.GetMouseButtonDown(0)) //Left click
        {
            grabbedPiece = GetBoardPiece(BoardCoordinates(mousePos));
            if (grabbedPiece == null /*|| not a piece of player's color*/)
                return;
            else
                gameState = DropPieceState; //Change state
        }
    }

    void DropPieceState()
    {
        grabbedPiece.transform.position = new Vector3(mousePos.x, mousePos.y, -2); //Make the piece move along the cursor and have it appear on top of every other piece (z = -2)

        //Display piece travel and destination traces
        DisplayTraces(grabbedPiece);

        if (Input.GetMouseButtonUp(0))
        {
            Vector2Int testPos = BoardCoordinates(mousePos);

            if (!IsMoveAllowed(testPos))
            {
                Vector3 resetPos = new Vector3(UnityBoardCoordinates(grabbedPiece.boardCoords).x, UnityBoardCoordinates(grabbedPiece.boardCoords).y, -1);
                grabbedPiece.transform.position = resetPos;
            }
            else
            {
                grabbedPiece.pastBoardCoords = grabbedPiece.boardCoords;
                grabbedPiece.boardCoords = testPos;
                grabbedPiece.transform.position = UnityBoardCoordinates(testPos);

                //En passant eating functionality
                bool enPassantEat = grabbedPiece.isAPawn != null && grabbedPiece.isAPawn.enPassantPawn != null && testPos == grabbedPiece.isAPawn.enPassantPawn.boardCoords + (grabbedPiece.isWhite ? Vector2Int.up : Vector2Int.down);

                //Replace enemy piece with
                Piece enemyPiece = GetBoardPiece(testPos);
                if (enemyPiece != null && !enPassantEat) //Allowed destinations will already have considered whether this piece is the proper color, so if it's not null, it's an enemy piece
                    EatPieceTEST(enemyPiece, grabbedPiece);
                if (enemyPiece == null && enPassantEat)
                    EatPieceTEST(grabbedPiece.isAPawn.enPassantPawn, grabbedPiece, true);
                else
                {
                    board[grabbedPiece.boardCoords.x, grabbedPiece.boardCoords.y] = grabbedPiece;
                    board[grabbedPiece.pastBoardCoords.x, grabbedPiece.pastBoardCoords.y] = null;
                }

                if (grabbedPiece.isAPawn != null && (grabbedPiece.boardCoords.y == 0 || grabbedPiece.boardCoords.y == 7))
                    Debug.Log("initiate pawn promotion for " + grabbedPiece.isAPawn);

                //En passant flag control
                UpdateEnPassantFlags();

                UpdateAllowedDestinations(); //NOTE: This might be better suited to an async function; it's a lot of computations for one frame
                //***
                //***IT WOULD BE MORE EFFICIENT IF I HAD ONE FOR LOOP THROUGH THE PIECE LISTS, AND FOR EACH PIECE, I WOULD RUN THE BOOLS 
                //***THE CURRENT SETUP IS TO RUN THROUGH THE LIST EACH TIME TO DETERMINE BOOLS (IS IT OOB, IS IT CHECKING KING, ETC)

                //If no allowed destinations exist for the opponent, it's checkmate
                //IsItCheckMate()?
                //for loop from grabbedPiece.isWhite opponent's allowed destinations lists, if everything is zero then it'S checkmate

                IsEnemyKingChecked();
                //Make sure to reset king's check, if current player's king was in check before performing an allowed movement                
            }
            //**Have to allow for pawn promotion state as well (will need some sort of flag)

            gameState = SelectAPieceState; //NOTE: this will have to be changed to let other player play, but for now, for testing, it goes back to piece selecting of either player
            HideTraces(grabbedPiece);
        }
    }

    void PromotePawnState()
    {
        //
    }

    void GameOverState()
    {
        //
    }

    private void Awake()
    {
        INITIATETEST();

        return;

        InitiateBoardArray();
        InitiateMoveTraces();
        InitiatePawnPromotions();

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
        TEST_TEST();

        return;

        CheckForMouseClick();

        CarryAPiece();
    }

    public void TEST_TEST()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        gameState();
        //Would like to declare a generic action function that can refer to 1 of 3 methods, and never more than 1 at a time
        //3 methods are: playing the game, pawn promotion selecting, win/lose game over screen - with options to replay, restart, quit, etc.

        //GenericFunction()

        //Playing the game() Mouse hovering and events based calculations whenever a piece is picked up and dropped
        //Opponent plays (still need to test logic before letting this be automated)

        //Pawn promotion selecting() Mouse hovering and more event based actions when promotion is selected
        //Reverts to Playing the game()

        //Gameover() Mouse hovering again and selecting options changes scene or reverts to Playing the game()
    }

    void INITIATETEST()
    {
        gameState = SelectAPieceState; //**Would be main menu in final version

        #region CREATE PIECES
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        void CreatePiece<T>(Vector2Int boardCoords, bool isWhite, string name, Sprite sprite) where T : Piece
        {
            Piece piece = new GameObject().AddComponent<T>();
            piece.isWhite = isWhite;
            piece.name = name;
            piece.gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
            piece.boardCoords = boardCoords;
            board[boardCoords.x, boardCoords.y] = piece;
            piece.transform.position = UnityBoardCoordinates(boardCoords);
            (isWhite ? wPieces : bPieces).Add(piece);
        }

        //NOTE: white king is wPieces[0] and black king is bPieces[0]
        CreatePiece<King>(new Vector2Int(4, 0), true, "wKing", pieceSprites[10]);
        CreatePiece<King>(new Vector2Int(4, 7), false, "bKing", pieceSprites[4]);
        for (int i = 0; i < 8; i++)
        {
            CreatePiece<Pawn>(new Vector2Int(i, 1), true, "wPawn_" + i.ToString(), pieceSprites[11]);
            CreatePiece<Pawn>(new Vector2Int(i, 6), false, "bPawn_" + i.ToString(), pieceSprites[5]);
        }
        for (int i = 0; i < 2; i++)
        {
            CreatePiece<Rook>(new Vector2Int(7 * i, 0), true, "wRook_" + i.ToString(), pieceSprites[6]);
            CreatePiece<Rook>(new Vector2Int(7 * i, 7), false, "bRook_" + i.ToString(), pieceSprites[0]);
            CreatePiece<Knight>(new Vector2Int(1 + 5 * i, 0), true, "wKnight_" + i.ToString(), pieceSprites[7]);
            CreatePiece<Knight>(new Vector2Int(1 + 5 * i, 7), false, "bKnight_" + i.ToString(), pieceSprites[1]);
            CreatePiece<Bishop>(new Vector2Int(2 + 3 * i, 0), true, "wBishop_" + i.ToString(), pieceSprites[8]);
            CreatePiece<Bishop>(new Vector2Int(2 + 3 * i, 7), false, "bBishop_" + i.ToString(), pieceSprites[2]);
        }
        CreatePiece<Queen>(new Vector2Int(3, 0), true, "wQueen", pieceSprites[9]);
        CreatePiece<Queen>(new Vector2Int(3, 7), false, "bQueen", pieceSprites[3]);
        #endregion

        #region CREATE TRACE SPRITES
        Sprite destinationTraceSprite = Resources.Load<Sprite>("DestinationTrace");
        Sprite travelTraceSprite = Resources.Load<Sprite>("TravelTrace");
        
        for(int i = 0; i < traces.Length / 2; i++)
        {
            traces[i] = new GameObject().AddComponent<SpriteRenderer>();
            traces[i + traces.Length / 2] = new GameObject().AddComponent<SpriteRenderer>();
            traces[i].sprite = destinationTraceSprite;
            traces[i].name = "DestinationTrace_" + i.ToString();
            traces[i + traces.Length / 2].sprite = travelTraceSprite;
            traces[i + traces.Length / 2].name = "TravelTrace_" + i.ToString();

            traces[i].transform.localScale = traces[i + traces.Length / 2].transform.localScale = new Vector3(28, 28, 1);
            traces[i].transform.position = traces[i + traces.Length / 2].transform.position = new Vector3(0, 0, 1);
        }
        #endregion

        #region CREATE PAWN PROMOTIONS SPRITES
        SpriteRenderer CreateSpriteRenderer(Sprite sprite, Vector3 pos, string name)
        {
            SpriteRenderer sr = new GameObject().AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.transform.position = pos;
            sr.name = name; //Not necessary, but makes them easier to identify in Unity Editor
            
            return sr;
        }
        pawnPromotions[0] = CreateSpriteRenderer(pieceSprites[9], new Vector3(-45, 15, 1), "wQueen");
        pawnPromotions[1] = CreateSpriteRenderer(pieceSprites[8], new Vector3(-15, 15, 1), "wBishop");
        pawnPromotions[2] = CreateSpriteRenderer(pieceSprites[7], new Vector3(15, 15, 1), "wKnight");
        pawnPromotions[3] = CreateSpriteRenderer(pieceSprites[6], new Vector3(45, 15, 1), "wRook");
        pawnPromotions[4] = CreateSpriteRenderer(pieceSprites[3], new Vector3(-45, -15, 1), "bQueen");
        pawnPromotions[5] = CreateSpriteRenderer(pieceSprites[2], new Vector3(-15, -15, 1), "bBishop");
        pawnPromotions[6] = CreateSpriteRenderer(pieceSprites[1], new Vector3(15, -15, 1), "bKnight");
        pawnPromotions[7] = CreateSpriteRenderer(pieceSprites[0], new Vector3(45, -15, 1), "bRook");

        Sprite promoBackgroundSprite = Resources.Load<Sprite>("PromoBackground");
        pawnPromotionBackground = CreateSpriteRenderer(promoBackgroundSprite, new Vector3(0, 0, 1.25f), "promotionBG");
        #endregion

        #region ESTABLISH STARTING ALLOWED DESTINATIONS - Not sure if necessary (only pawns and knights have starting destinations)
        for (int i = 0; i < wPieces.Count; i++)
            wPieces[i].InitialAllowedDestinations();
        for (int i = 0; i < bPieces.Count; i++)
            bPieces[i].InitialAllowedDestinations();
        #endregion
    }

    void CheckForMouseClick()
    {
        Vector2Int mousedOverBoardCoords = BoardCoordinates(mousePos);
//        Debug.Log(mousedOverBoardCoords);

        if(ongoingPawnPromotion)
        {
            //Check for hovering over the 4 possible promotions, and when a click is registered on one of them, change promotedPawn into that piece and reset promotion display to regular values (hidden behind board)
            //            Debug.Log(mousePos + ", " + PawnPromotionArray(mousePos));

            Piece promotion = DisplayPawnPromotionArray(mousePos);
            if (Input.GetMouseButtonDown(0))
            {
                if(promotion != null)
                {
                    PromotePawn(promotion);
                    RemovePawnPromotionArray();
                }
            }
            return;
        }

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

            if(Input.GetMouseButtonUp(0)) //Let go of piece
            {
                movingPiece.DropOnBoard(BoardCoordinates(mousePos));

//                CleanUpBoardDisplay();
                movingPiece.beingCarried = false;
                movingPiece.transform.position = new Vector3(movingPiece.transform.position.x, movingPiece.transform.position.y, -1);
//                movingPiece.DidMovingPutOpposingKingInCheck();
                HideTraces(movingPiece);
                movingPiece = null;
            }
        }
    }

    //    public void DisplayTraces(Vector2Int[] destinationTraces, Vector2Int[] travelTraces)
    void DisplayTraces(Piece piece)
    {
        traces[0].transform.position = UnityBoardCoordinates(piece.boardCoords);
        for (int i = 0; i < piece.allowedDestinations.Count; i++)
            traces[i + 1].transform.position = UnityBoardCoordinates(piece.allowedDestinations[i], .5f);

//        return;
  //      for (int i = 0; i < piece.allowedDestinations.Count; i++)
    //        travelTraces[i].transform.position = UnityBoardCoordinates(piece.allowedDestinations[i], .5f);

      //  destinationTraces[0].transform.position = UnityBoardCoordinates(piece.boardCoords, .5f);
    }

    void HideTraces(Piece piece)
    {
        for (int i = 0; i < traces.Length; i++)
            traces[i].transform.position = new Vector3(0, 0, 1);

//        return;
//        for (int i = 0; i < piece.allowedDestinations.Count; i++)
  //      for(int i = 0; i< travelTraces.Length; i++)
    //        travelTraces[i].transform.position = new Vector3(0, 0, 1);
    
        //destinationTraces[0].transform.position = new Vector3(0, 0, 1);
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

    void InitiatePawnPromotions()
    {
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        promotionPieces[0] = new GameObject().AddComponent<Queen>();
        promotionPieces[0].name = "wQueenPromotion";
        promotionPieces[0].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[9];
        promotionPieces[1] = new GameObject().AddComponent<Bishop>();
        promotionPieces[1].name = "wBishopPromotion";
        promotionPieces[1].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[8];
        promotionPieces[2] = new GameObject().AddComponent<Knight>();
        promotionPieces[2].name = "wKnightPromotion";
        promotionPieces[2].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[7];
        promotionPieces[3] = new GameObject().AddComponent<Rook>();
        promotionPieces[3].name = "wRookPromotion";
        promotionPieces[3].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[6];

        for (int i = 0; i < 4; i++)
        {
            promotionPieces[i].isWhite = true;
            promotionPieces[i].transform.position = new Vector3(-45 + 30 * i, 15, 1);
        }

        promotionPieces[4] = new GameObject().AddComponent<Queen>();
        promotionPieces[4].name = "bQueenPromotion";
        promotionPieces[4].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[3];
        promotionPieces[5] = new GameObject().AddComponent<Bishop>();
        promotionPieces[5].name = "bBishopPromotion";
        promotionPieces[5].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[2];
        promotionPieces[6] = new GameObject().AddComponent<Knight>();
        promotionPieces[6].name = "bKnightPromotion";
        promotionPieces[6].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[1];
        promotionPieces[7] = new GameObject().AddComponent<Rook>();
        promotionPieces[7].name = "bRookPromotion";
        promotionPieces[7].gameObject.AddComponent<SpriteRenderer>().sprite = pieceSprites[0];

        for (int i = 4; i < 8; i++)
        {
            promotionPieces[i].isWhite = false;
            promotionPieces[i].transform.position = new Vector3(-45 + 30 * (i - 4), -15, 1);
        }

        Sprite promoBackgroundSprite = Resources.Load<Sprite>("PromoBackground");
        promotionBackground = new GameObject().AddComponent<SpriteRenderer>();
        promotionBackground.sprite = promoBackgroundSprite;
        promotionBackground.name = "promotionBackground";
        promotionBackground.transform.position = new Vector3(0, 0, 1);
    }
    #endregion

    public void EatPiece(Piece piece)
    {
        //**Would be nice to organize the eaten lists by value, and might need to manipulate z a bit to stack them nicely
        if (piece.isWhite)
        {
//            piece.transform.position = new Vector3(-200, 100 - eatenWhitePieces.Count * 10);
  //          eatenWhitePieces.Add(piece);
        }
        else
        {
    //        piece.transform.position = new Vector3(200, 100 - eatenBlackPieces.Count * 10);
      //      eatenBlackPieces.Add(piece);
        }

       // boardArray[piece.boardCoords.x, piece.boardCoords.y] = null;
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
        movingPiece.DidMovingPutOpposingKingInCheck();
        boardArray[testBoardCoords.x, testBoardCoords.y] = movingPiece; //Add new piece to board array
    }

    Piece DisplayPawnPromotionArray(Vector3 pos) //Helps identify and highlight pawn promotions
    {
        //Return piece from the promotion board that mouse is hovering over
        bool isWhite = promotedPawn.isWhite;
        for(int i = 0; i < 4; i++)
        {
            Vector2 piecePos = promotionPieces[isWhite ? i : i + 4].transform.position;
            if (pos.x < piecePos.x + 15 && pos.x > piecePos.x - 15 && pos.y < piecePos.y + 15 && pos.y > piecePos.y - 15)
            {
                destinationTraces[0].transform.position = new Vector3(promotionPieces[isWhite ? i : i + 4].transform.position.x, promotionPieces[isWhite ? i : i + 4].transform.position.y, -1.75f);
                return promotionPieces[isWhite ? i : i + 4];
            }
        }

        destinationTraces[0].transform.position = new Vector3(0,0,1);
        return null;
    }

    void RemovePawnPromotionArray()
    {
        for (int i = 0; i < 8; i++)
            promotionPieces[i].transform.position = new Vector3(0, 0, 1);

        destinationTraces[0].transform.position = promotionBackground.transform.position = new Vector3(0, 0, 1);
    }

    void PromotePawn(Piece promotion)
    {
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        if (promotion.isAQueen)
            DefinePiece<Queen>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedQueen", promotedPawn.isWhite ? pieceSprites[9] : pieceSprites[3]);
        else if (promotion.isABishop)
            DefinePiece<Bishop>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedBishop", promotedPawn.isWhite ? pieceSprites[8] : pieceSprites[2]);
        else if (promotion.isAKnight)
            DefinePiece<Knight>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedKnight", promotedPawn.isWhite ? pieceSprites[7] : pieceSprites[1]);
        else if (promotion.isARook)
            DefinePiece<Rook>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedRook", promotedPawn.isWhite ? pieceSprites[6] : pieceSprites[0]);

        Destroy(promotedPawn.gameObject);
        ongoingPawnPromotion = false;
    }
}