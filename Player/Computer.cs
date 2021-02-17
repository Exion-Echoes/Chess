using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Computer : Player
{
    struct UsableTiles
    {
        public Tile tile;
        public List<Tile> possibleMoves;
    }

    List<UsableTiles> usableTiles = new List<UsableTiles>();
    int tileNum;

    public override void PickAPiece()
    {
        ListAllUsableTiles();

        tileNum = Random.Range(0, usableTiles.Count);

        startTile = usableTiles[tileNum].tile;
        board.grabbedPiece = usableTiles[tileNum].tile.piece;
        board.gameState = DropAPiece;
    }

    public override void DropAPiece()
    {
        int moveNum = Random.Range(0, usableTiles[tileNum].possibleMoves.Count);
        Tile endTile = usableTiles[tileNum].possibleMoves[moveNum];

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
                board.gameState = PromotePawn;
            else //Drop piece and return to regular play
                board.gameState = board.SwitchPlayer(this).PickAPiece;

            board.IsItGameOver(); //See whether opposing king is checkmated, or game reached a stalemate
            board.turnSwapper = !board.turnSwapper;
        }
        else //I don't think I need this check for the computer, but I'll leave it in case - it seems useful for protecting king from checks
        {
            board.grabbedPiece.transform.position = board.UnityUnits(startTile.pos); //Reset grabbed piece back to original position
            board.gameState = PickAPiece;
            Debug.Log("Could not place here");
        }
    }

    public override void PromotePawn()
    {
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

        int promotionNum = Random.Range(0, 4);
        Debug.Log(promotionNum + ", " + (promotionNum + (isWhite ? 0 : 4)));
        ChangePawn(promotionNum + (isWhite ? 1 : 5));
        board.grabbedPiece = board.state[board.grabbedPiece.pos.x + 8 * board.grabbedPiece.pos.y].piece;
        board.gameState = board.SwitchPlayer(this).PickAPiece;
        board.IsItGameOver(); //Promoting a pawn to a rook may checkmate or stalemate the opposing king
    }


    void ListAllUsableTiles()
    {
        usableTiles.Clear();
        for (int i = 0; i < 64; i++)
        {
            if (board.state[i].piece != null && board.state[i].piece.isWhite == isWhite)
            {
                List<Tile> possibleMoves = board.state[i].piece.PossibleMoves();
                if (possibleMoves.Count > 0) //It doesn't matter if a player tries to move a piece with no possible moves, but there's no reason the AI should be able to too
                {
                    UsableTiles uT;
                    uT.tile = board.state[i];
                    uT.possibleMoves = possibleMoves;
                    usableTiles.Add(uT);
                }
            }
        }
    }
}
