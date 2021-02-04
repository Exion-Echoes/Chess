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

    public Tile[] state = new Tile[64];
    public Piece grabbedPiece; //Piece being operated on
    public List<Piece> eatenPieces = new List<Piece>();
    public Tile startTile, wKTile, bKTile; //King tiles need to be updated (Pieces wouldn't have to, but I would still need to keep track of their pos)
    public Tile lWRookTile, rWRookTile, lBRookTile, rBRookTile;
    public Tile enPassantTile; //Needs to be reset as soon as a move is made after this has been turned on

    public void Awake()
    {
        state = InitialBoardState();

        gameState = PickAPiece;
    }

    private void Start()
    {
        //Define rooks references - Kings are initiated after Rooks, so there should be no problem relying on piece.isARook
        lWRookTile = state[0];
        rWRookTile = state[7];
        lBRookTile = state[0 + 7 * 8];
        rBRookTile = state[7 + 7 * 8];
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

                if (endTile.piece != null) //Check for regular eat
                    DistributeEatenPieces(ref endTile);

                else //Consider en passant eat
                    HandleEnPassantLogic(startTile, endTile);

                grabbedPiece.transform.position = UnityUnits(endTile.pos); //Place grabbed piece into end tile
                endTile.piece = grabbedPiece;
                endTile.piece.pos = endTile.pos;
                startTile.piece = null;

                VerifyIfKingTileNeedsToBeUpdated(endTile); //If grabbedPiece is a king

                if (!EnemyCanMove()) //In the case where there are no more possible enemy moves, the game's over, with a checkmate or a stalemate
                {
                    bool isEnemyKingChecked = IsEnemyKingChecked();
                    if (isEnemyKingChecked)
                        Debug.Log("Checkmate");
                    //gameState = GameOver("Checkmate"); - show who won and who lost
                    else
                        Debug.Log("Stalemate");
                    //gameState = GameOver("Stalemate");
                }

                //See if castling just occured
                if (grabbedPiece.isAKing != null && !grabbedPiece.isAKing.moved && Mathf.Abs(startTile.pos.x - endTile.pos.x) == 2)
                    MoveRookForCastling(grabbedPiece.isAKing);

                if (grabbedPiece.isAPawn != null && (endTile.pos.y == 0 || endTile.pos.y == 7)) //Check pawn reached promotion line
                    gameState = PromotePawn;

                else //Drop piece and return to regular play
                    gameState = PickAPiece;

                //This should be done by a delegate waiting for CanMove to be called for the relevant pieces, but for now i'm just gonna put it here
                #region TURN ON moved BOOLS ON KINGS AND ROOKS, IF NEEDED
                if (grabbedPiece.isAKing != null)
                    grabbedPiece.isAKing.moved = true;
                bool IsItARook(Tile rookTile)
                {
                    return rookTile.piece != null && rookTile.piece.isARook != null && grabbedPiece == rookTile.piece;
                }
                if (IsItARook(lWRookTile) || IsItARook(rWRookTile) || IsItARook(lBRookTile) || IsItARook(rBRookTile))
                    grabbedPiece.isARook.moved = true;
                #endregion
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
        Debug.Log("Promote pawn");
        //Produce pawn promotion board and 4 pieces (with possibility of highlighting the one hovered over by the mouse cursor

        //***
        //***
        //DO THIS AFTER CASTLING
        //***
        //***
    }

    public Tile TileAt(Vector2Int pos) //Look at current board and return tile
    {
        pos.x = Mathf.Clamp(pos.x, 0, 7); //Clamp to prevent wrapping issues (e.g. King at 0,7 could move to 7,7)
        pos.y = Mathf.Clamp(pos.y, 0, 7);
        if (pos.x + 8 * pos.y < 64 && pos.x + 8 * pos.y >= 0)
            return state[pos.x + 8 * pos.y];
        return null;
    }

    Vector3 UnityUnits(Vector2Int pos, float z = 0)
    {
        return new Vector3(-105 + pos.x * 30, -105 + pos.y * 30, -1 + z);
    }

    Vector2Int BoardUnits(Vector3 pos)
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
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[6], UnityUnits(piecePos), true);
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
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[6], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(4, 0);
        wKTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[10], UnityUnits(piecePos), true);
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
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(3, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Queen>(), pieceSprites[3], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(5, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(6, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Knight>(), pieceSprites[1], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(7, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Rook>(), pieceSprites[0], UnityUnits(piecePos), false);
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

    public void DistributeEatenPieces(ref Tile tile)
    {
        Piece eatenPiece = new GameObject().AddComponent<Piece>();
        eatenPiece.gameObject.AddComponent<SpriteRenderer>().sprite = tile.piece.sr.sprite;
        eatenPiece.isWhite = tile.piece.isWhite;
        eatenPieces.Add(eatenPiece);
        Destroy(tile.piece.gameObject);
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

        eatenPiece.transform.position = new Vector3((eatenPiece.isWhite ? 150 : 180), 120 - (eatenPiece.isWhite ? wCount : bCount) * 15, -1);
    }

    void VerifyIfKingTileNeedsToBeUpdated(Tile endTile)
    {
        if (startTile == wKTile) //Check if king tile needs to be updated
            wKTile = endTile;
        else if (startTile == bKTile)
            bKTile = endTile;
    }

    void HandleEnPassantLogic(Tile s, Tile e)
    {
        if (s.piece.isAPawn != null)
        {
            if (e == TileAt(new Vector2Int(s.pos.x, s.pos.y + 2 * (s.piece.isWhite ? 1 : -1)))) //If pawn double moved, it becomes a pawn that may be eaten en passant
                enPassantTile = e;

            //If pawn moved diagonally above the enPassantTile, then the piece at enPassantTile is eaten
            if (enPassantTile != null && enPassantTile.piece != null && (s.piece.pos == enPassantTile.pos - Vector2Int.right || s.piece.pos == enPassantTile.pos + Vector2Int.right))
            {
                if (e.pos == enPassantTile.pos + (s.piece.isWhite ? Vector2Int.up : Vector2Int.down) && s.piece.isWhite != enPassantTile.piece.isWhite)
                    DistributeEatenPieces(ref enPassantTile);
            }
        }
        if (enPassantTile != null && enPassantTile.piece != null && s.piece.isWhite == enPassantTile.piece.isWhite)// && s.piece.isAPawn == null) //When a non-pawn piece is picked up on the next turn, the en passant possibility is nullified
            enPassantTile = null;
    }

    bool IsEnemyKingChecked()
    {
        //Verify that king is attacked by grabbedPiece
        List<Tile> pieceAttacks = new List<Tile>();
        if (grabbedPiece.isAPawn == null)
            pieceAttacks = grabbedPiece.PossibleMoves();
        else
            pieceAttacks = grabbedPiece.isAPawn.Attacks();
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
                        {
                            Debug.Log(state[i].piece + ", " + state[i].pos + ", " + j + ", " + possibleMoves[j].pos);
                            return true;
                        }
                    }
                }
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
}