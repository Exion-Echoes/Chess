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


public class Board : MonoBehaviour
{
    delegate void GameStateDelegate();
    GameStateDelegate gameState;

    public Tile[] state = new Tile[64];
    public Piece grabbedPiece; //Piece being operated on
    public Tile startTile, endTile, wKTile, bKTile; //king references are always pointing to the king's tile, after being initialized; I don't have to update them in the code

    public void Awake()
    {
        state = InitialBoardState();

//        for (int i = 0; i < 64; i++)
//            Debug.Log(i + ", " + state[i].id + ", " + state[i].piece);

        gameState = PickAPiece;

        for(int i = 0; i < state.Length; i++)
        {
            if (state[i] != null && state[i].piece != null && state[i].piece.GetComponent<Bishop>() != null)
            {
//                for(int j = 0; j < state[i].piece.PossibleMoves().Count; j++)
//                    Debug.Log(state[i].piece.pos + ", " + state[i].piece.PossibleMoves()[j]);
            }
        }
    }

    public void Update()
    {
        gameState();
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


        //        Debug.Log(BoardUnits(mousePos) + ", " + UnityUnits(BoardUnits(mousePos)) + ", " + mousePos);
        //  if(TileAt(BoardUnits(mousePos)) != null)
        //        Debug.Log(mousePos + ", " + BoardUnits(mousePos) + ", " + UnityUnits(BoardUnits(mousePos)) + ", " + TileAt(BoardUnits(mousePos)).pos);
        //      else
        //            Debug.Log(mousePos + ", " + BoardUnits(mousePos) + ", " + UnityUnits(BoardUnits(mousePos)));
    }

    public void DropAPiece()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        grabbedPiece.transform.position = new Vector3(mousePos.x, mousePos.y, -1.5f);

        if (Input.GetMouseButtonUp(0)) //Let go of left click
        {
            Tile endTile = TileAt(BoardUnits(grabbedPiece.transform.position));
            Debug.Log(startTile.piece + ", " + startTile.id + ", " + endTile.piece);
            if (endTile != null && grabbedPiece.CanMove(startTile, endTile))
            {
                Debug.Log("Could place here");

                //***
                //***
                //EAT PIECE IF ANY PRESENT
                //***
                //***

                grabbedPiece.transform.position = UnityUnits(endTile.pos);
                endTile.piece = grabbedPiece;
                endTile.piece.pos = endTile.pos;
                startTile.piece = grabbedPiece = null;
                gameState = PickAPiece;
            }
            else
            {
                grabbedPiece.transform.position = UnityUnits(startTile.pos);
                grabbedPiece = null;
                gameState = PickAPiece;
                Debug.Log("Could not place here");
            }
        }
    }

    public void TestBoardState(Tile removed, Tile added = null)
    {
        //Determine board state after removing a piece from startTile to endTile
        //Board state consists of determining tiles that are attacked
    }

    public Tile TileAt(Vector2Int pos) //Look at current board and return tile
    {
        if(pos.x + 8 * pos.y < 64 && pos.x + 8 * pos.y >= 0)
            return state[pos.x + 8 * pos.y];
        return null;
    }

    Vector3 UnityUnits(Vector2Int pos, float z = 0)
    {
        return new Vector3(-105 + pos.x * 30, -105 + pos.y * 30, -1 + z);
    }

    Vector2Int BoardUnits(Vector3 pos)
    {
        if(pos.x >= -120 && pos.x <= 120 && pos.y >= -120 && pos.y <= 120)
            return new Vector2Int((int)(120 + pos.x) / 30, (int) (120 + pos.y) / 30);
        return new Vector2Int(-1, -1);
    }

    Tile[] InitialBoardState()
    {
        Tile[] boardState = new Tile[64];
        Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        Vector2Int piecePos;
        piecePos = new Vector2Int(0, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[6], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(1, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[7], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(2, 0);
        //        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], UnityUnits(piecePos), true);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, null, pieceSprites[8], UnityUnits(piecePos), true); //DEBUG
        piecePos = new Vector2Int(3, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Queen>(), pieceSprites[9], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(4, 0);
        wKTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[10], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(5, 0);
        //        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], UnityUnits(piecePos), true);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, null, pieceSprites[8], UnityUnits(piecePos), true); //DEBUG
        piecePos = new Vector2Int(6, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[7], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(7, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[6], UnityUnits(piecePos), true);
        for (int i = 0; i < 8; i++)
        {
            piecePos = new Vector2Int(i, 1);
            boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Pawn>(), pieceSprites[11], UnityUnits(piecePos), true);
        }
        piecePos = new Vector2Int(0, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[0], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(1, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[1], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(2, 7);
        //        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, null, pieceSprites[2], UnityUnits(piecePos), false); //DEBUG
        piecePos = new Vector2Int(3, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Queen>(), pieceSprites[3], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(4, 7);
        //        bKTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[4], UnityUnits(piecePos), false);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, null, pieceSprites[4], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(5, 7);
        //        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, null, pieceSprites[2], UnityUnits(piecePos), false); //DEBUG
        piecePos = new Vector2Int(6, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[1], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(7, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[0], UnityUnits(piecePos), false);
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

        //temporary debug thingies
        piecePos = new Vector2Int(3, 4);
        boardState[3 + 8 * 4] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(5, 4);
        boardState[5 + 8 * 4] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(6, 5);
        bKTile = boardState[6 + 8 * 5] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[4], UnityUnits(piecePos), false);
        //      for (int i = 0; i < 64; i++)
        //            Debug.Log(i + ", " + boardState[i].pos);
        for (int i = 0; i < 64; i++) //Can be determined as a function of the tile position (to be changed later)
            boardState[i].id = i;
        return boardState;
    }

    //GET BISHOP WORKING, THEN GET BOARD STATE WORKING, AND GET MOVINGHCECKSOWNKING WORKING (WANT BISHOP TO MOVE ALONG THE DIAGONAL IT'S DEFENDING)
    //Bishop has CanMove working and PossibleMoves
    //Board State will determine every enemy piece's PossibleMoves (the collection of which I am calling the "board state")
    //MovingChecksOwnKing will look at a temporary board state and will recalculate enemy piece's possiblemoves to determine if king's checked
    
    //Make a temporary board state with just bishops and Kings to test movingchecksownking
    //Board state involves finding out where enemy pieces attack and determining if a check is occurring
    //If no moves can be made, it's checkmate (if king is checked) and a stalemate (if king isn't checked)

    //To see if bishop will check king by moving:
    //  First run CanMove and confirm that it is a possiblemove
    //  In the MoveChecksKing part
    //      Create the temporary board state (starttile empty and endtile replaced by piece)
    //      Assemble all PossibleMove() of other pieces (ignore pawns as they can't be made to check the King by a reveal)
    //      If any coincide with king's position, return false on the piece.CanMove

    //**I think I can remove stateTest bool from CanMove








    #region
    /*
    //Board is 120 x 120 and situated at (0, 0) - squares are 30 x 30

    public Piece[,] boardArray = new Piece[8, 8]; //Used to manage piece-piece interactions
    public Piece movingPiece;
    public List<Piece> eatenWhitePieces = new List<Piece>(); //REPLACE THIS BY BOOL - isOnBoard
    public List<Piece> eatenBlackPieces = new List<Piece>();
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
    public List<Piece> wCheckers = new List<Piece>(); //Pieces responsible for having checked the king
    public List<Piece> bCheckers = new List<Piece>(); 
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
                    {
                        bPieces[0].isAKing.isChecked = true;
                        wCheckers.Add(grabbedPiece);
                    }
                    else
                    {
                        wPieces[0].isAKing.isChecked = true;
                        bCheckers.Add(grabbedPiece);
                    }
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
            if (grabbedPiece == null /*|| not a piece of player's color*///)
                                                                         //                return;
    /*            else
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

                if (!IsMoveAllowed(testPos)) //If not dropped in an allowed destination
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
        }

        void Update()
        {
            UpdateCurrentGameState();
        }

        public void UpdateCurrentGameState()
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

        void DisplayTraces(Piece piece)
        {
            traces[0].transform.position = UnityBoardCoordinates(piece.boardCoords);
            for (int i = 0; i < piece.allowedDestinations.Count; i++)
                traces[i + 1].transform.position = UnityBoardCoordinates(piece.allowedDestinations[i], .5f);

            for (int i = 0; i < piece.linesOfAttack.Count; i++)
                traces[piece.allowedDestinations.Count + 1 + i].transform.position = UnityBoardCoordinates(piece.linesOfAttack[i], .5f);
        }

        void HideTraces(Piece piece)
        {
            for (int i = 0; i < traces.Length; i++)
                traces[i].transform.position = new Vector3(0, 0, 1);
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

        void PromotePawn(Piece promotion)
        {
            Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

        //    if (promotion.isAQueen)
      //          DefinePiece<Queen>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedQueen", promotedPawn.isWhite ? pieceSprites[9] : pieceSprites[3]);
    //        else if (promotion.isABishop)
            //    DefinePiece<Bishop>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedBishop", promotedPawn.isWhite ? pieceSprites[8] : pieceSprites[2]);
          //  else if (promotion.isAKnight)
        //        DefinePiece<Knight>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedKnight", promotedPawn.isWhite ? pieceSprites[7] : pieceSprites[1]);
      //      else if (promotion.isARook)
    //            DefinePiece<Rook>(promotedPawn.boardCoords, promotedPawn.isWhite, "promotedRook", promotedPawn.isWhite ? pieceSprites[6] : pieceSprites[0]);

            Destroy(promotedPawn.gameObject);
            ongoingPawnPromotion = false;
        }
        */
    #endregion
}