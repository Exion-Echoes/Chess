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
    public Tile startTile, endTile, wKTile, bKTile; //King tiles need to be updated (Pieces wouldn't have to, but I would still need to keep track of their pos)

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

                grabbedPiece.transform.position = UnityUnits(endTile.pos); //Place grabbed piece into end tile
                endTile.piece = grabbedPiece;
                endTile.piece.pos = endTile.pos;
                startTile.piece = null;

                if (startTile == wKTile) //Check if king tile needs to be updated
                    wKTile = endTile;
                else if (startTile == bKTile)
                    bKTile = endTile;
                if (grabbedPiece.isAPawn != null && (endTile.pos.y == 0 || endTile.pos.y == 7)) //Check pawn reached promotion line
                    gameState = PromotePawn;

                //***
                //***
                //Need to look for a check against the king, a checkmate, and a stalemate
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
}