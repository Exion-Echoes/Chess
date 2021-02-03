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

public class Board : MonoBehaviour
{
    delegate void GameStateDelegate();
    GameStateDelegate gameState;

    public Tile[] state = new Tile[64];
    public Piece grabbedPiece; //Piece being operated on
    public List<Piece> eatenPieces = new List<Piece>();
    public Tile startTile, wKTile, bKTile; //King tiles need to be updated (Pieces wouldn't have to, but I would still need to keep track of their pos)
    public Tile enPassantTile; //Needs to be reset as soon as a move is made after this has been turned on

    public void Awake()
    {
        state = InitialBoardState();

        gameState = PickAPiece;
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

                //***
                //***
                //EAT PIECE IF ANY PRESENT
                //Need to consider en passant pawns as well - might be placeable in CanMove
                //***
                //***

                if (endTile.piece != null) //Check for regular eat
                    DistributeEatenPieces(ref endTile);

                //
                //
                //WORK ENPASSANT - CURRENTLY I HAVE THE PAWN VARIABLE IN THIS SCRIPT, AND I WANT TO TURN IT ON IN THE CANMOVE SCRIPT
                //if picked up piece isn'T enpassantpawn, then nullify the reference to that variable
                //pawn :19:20 I am ascribing enpassant to the pawn that double moves
                //am not sure yet how to best do this, I would like if the logic was contained and not scattered
                //The enpassant pawn
                //enPassantTile is active here, if it's not used, turn it off here, if it's used, turn it off ofc
                //
                //
                HandleEnPassantLogic(startTile, endTile);



                grabbedPiece.transform.position = UnityUnits(endTile.pos); //Place grabbed piece into end tile
                endTile.piece = grabbedPiece;
                endTile.piece.pos = endTile.pos;
                startTile.piece = null;

                VerifyIfKingTileNeedsToBeUpdated(endTile); //If grabbedPiece is a king

                if (grabbedPiece.isAPawn != null && (endTile.pos.y == 0 || endTile.pos.y == 7)) //Check pawn reached promotion line
                    gameState = PromotePawn;

                //***
                //***
                //Need to look for a check against the king, a checkmate, and a stalemate
                //Checkmate = king is checked and there are no available moves (possible moves)
                //Stalemate = king is not checked and there are no available moves (moving anywhere would check the king)
                //***
                //***

                else //Drop piece and return to regular play
                    gameState = PickAPiece;
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
        //
    }

    public Tile TileAt(Vector2Int pos) //Look at current board and return tile
    {
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
        piecePos = new Vector2Int(4, 0);
        wKTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[10], UnityUnits(piecePos), true);
        piecePos = new Vector2Int(5, 0);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], UnityUnits(piecePos), true);
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
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(3, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Queen>(), pieceSprites[3], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(4, 7);
        bKTile = boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<King>(), pieceSprites[4], UnityUnits(piecePos), false);
        piecePos = new Vector2Int(5, 7);
        boardState[piecePos.x + 8 * piecePos.y] = new Tile(piecePos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], UnityUnits(piecePos), false);
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
            //If moved pawn moved diagonally above the enPassantTile, then the piece at enPassantTile is eaten
            if (s.piece.pos == enPassantTile.pos - Vector2Int.right || s.piece.pos == enPassantTile.pos + Vector2Int.right)
            {
                if (e.pos == enPassantTile.pos + Vector2Int.up)
                    DistributeEatenPieces(ref enPassantTile);
            }

            //If this piece just became the en passant Pawn, I have to make sure it isn't reset straight away
            if (s.piece.isAPawn.becameAnEnPassantPawn)
                s.piece.isAPawn.becameAnEnPassantPawn = false;
            else
                enPassantTile = null;
        }
        //I don't think this is gonna work because when the opponent will play, the flag will be turned off already, and the enpassanttile will be deleted
    }
}