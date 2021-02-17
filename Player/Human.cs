using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Player
{
    public override void PickAPiece()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) //Left click
        {
            Tile tile = board.TileAt(board.BoardUnits(mousePos));
            if (tile != null && tile.piece != null)
            {
                startTile = tile;
                board.grabbedPiece = tile.piece;
                board.gameState = DropAPiece;
            }
        }
    }

    public override void DropAPiece()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        board.grabbedPiece.transform.position = new Vector3(mousePos.x, mousePos.y, -1.5f);

        if (Input.GetMouseButtonUp(0)) //Let go of left click
        {
            Tile endTile = board.TileAt(board.BoardUnits(board.grabbedPiece.transform.position));
            //            Debug.Log(startTile.piece + ", " + startTile.id + ", " + endTile.piece);
            if (endTile != null && board.grabbedPiece.CanMove(startTile, endTile))
            {
                board.moveNotification(board.grabbedPiece, startTile, endTile); //Notify subscribers that the grabbedPiece moved from startTile to endTile

                //Handle eating the piece occupied by the endTile, or enPassant
                if (endTile.piece != null) //Check for regular eating
                    board.DistributeEatenPieces(endTile);
                else if (board.grabbedPiece.isPawn != null && board.grabbedPiece.isPawn.enPassantPawn != null && endTile.pos.x == board.grabbedPiece.isPawn.enPassantPawn.pos.x) //Consider en passant eating
                    board.DistributeEatenPieces(board.TileAt(board.grabbedPiece.isPawn.enPassantPawn.pos));

                //Place grabbed piece into end tile
                board.grabbedPiece.transform.position = board.UnityUnits(endTile.pos);
                endTile.piece = board.grabbedPiece;
                endTile.piece.pos = endTile.pos;
                startTile.piece = null;

                //Consider possible changes of game states
                if (board.grabbedPiece.isPawn != null && (endTile.pos.y == 0 || endTile.pos.y == 7)) //Pawn reached a promotion row
                {
                    board.HandlePawnPromotionsMenuDisplay(board.grabbedPiece, true); //Display submenu on top of the board
                    board.gameState = PromotePawn;
                }
                else //Drop piece and return to regular play
                    board.gameState = board.SwitchPlayer(this).PickAPiece;

                board.IsItGameOver(); //See whether opposing king is checkmated, or game reached a stalemate
                board.turnSwapper = !board.turnSwapper;

                //GAMNESTATE = OTHER PLAYER'S PICKAPIECE
                //NEED A METHOD TO REFER TO THE OTHER PLAYER - FROM BOARD?
            }
            else
            {
                board.grabbedPiece.transform.position = board.UnityUnits(startTile.pos); //Reset grabbed piece back to original position
                board.gameState = PickAPiece;
                Debug.Log("Could not place here");
            }
        }
    }

    public override void PromotePawn()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        void ChangePawn(int i) //Turn grabbedPiece.isAPawn into a piece with a new logic
        {
            Sprite[] pieceSprites = Resources.LoadAll<Sprite>("Pieces");

            Tile tile = board.TileAt(board.grabbedPiece.pos);
            Destroy(tile.piece.gameObject);
            switch (i) // i = [1, 4] = white, i = [5,8] = black (queen, rook, knight, bishop)
            {
                case 1:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Queen>(), pieceSprites[9], board.UnityUnits(tile.pos), true);
                    break;
                case 2:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Rook>(), pieceSprites[6], board.UnityUnits(tile.pos), true);
                    break;
                case 3:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Knight>(), pieceSprites[7], board.UnityUnits(tile.pos), true);
                    break;
                case 4:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Bishop>(), pieceSprites[8], board.UnityUnits(tile.pos), true);
                    break;
                case 5:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Queen>(), pieceSprites[3], board.UnityUnits(tile.pos), false);
                    break;
                case 6:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Rook>(), pieceSprites[0], board.UnityUnits(tile.pos), false);
                    break;
                case 7:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Knight>(), pieceSprites[1], board.UnityUnits(tile.pos), false);
                    break;
                case 8:
                    board.state[tile.pos.x + 8 * tile.pos.y] = new Tile(tile.pos, new GameObject().AddComponent<Bishop>(), pieceSprites[2], board.UnityUnits(tile.pos), false);
                    break;
                default:
                    break;
            }
        }

        //After choosing
        if (Input.GetMouseButtonDown(0)) //Left click
        {
            for (int i = 1; i <= 4; i++)
            {
                Vector3 promoPos = (board.grabbedPiece.isWhite ? board.promoSprites[i] : board.promoSprites[i + 4]).transform.position;
                if (mousePos.x > promoPos.x - 15 && mousePos.x < promoPos.x + 15 && mousePos.y > promoPos.y - 15 && mousePos.y < promoPos.y + 15)
                {
                    ChangePawn(board.grabbedPiece.isWhite ? i : i + 4);
                    board.grabbedPiece = board.state[board.grabbedPiece.pos.x + 8 * board.grabbedPiece.pos.y].piece;
                    board.HandlePawnPromotionsMenuDisplay(null, false);
                    board.gameState = board.SwitchPlayer(this).PickAPiece;
                    board.IsItGameOver(); //Promoting a pawn to a rook may checkmate or stalemate the opposing king
                    break;
                }
            }
        }
    }
}
