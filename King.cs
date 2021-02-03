using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public bool moved;
    public bool isChecked; //Limits movements this turn

    public override bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        //Possible moves are such that adding x and y movements gives 1 when one component is 0 (hori/vert) and 2 when both components are equal (diagonal)
  //      Vector2Int move = new Vector2Int(Mathf.Abs(startTile.pos.x - endTile.pos.x), Mathf.Abs(startTile.pos.y - endTile.pos.y));
//        bool moveAllowed = move.x + move.y == 1 || move.x * move.y == 1;// ((posMove.x == 0 || posMove.y == 0) && (posMove.x + posMove.y == 1)) || ((posMove.x == posMove.y) && (posMove.x + posMove.y == 2));
        bool moveAllowed = InTheList(PossibleMoves(), endTile);
        if (moveAllowed)
        {
            if (!KingAttacked(startTile, endTile))
                return true;
        }
        return PerformCastling(endTile);
    }

    bool KingAttacked(Tile startTile, Tile endTile)
    {
        Piece tempStartPiece = startTile.piece; //Store pieces
        Piece tempEndPiece = endTile.piece;
        List<Tile> enemyPossibleMoves = new List<Tile>();

        void ReturnBoardToNormal() //Return stored tiles to original positions
        {
            endTile.piece = tempEndPiece;
            startTile.piece = tempStartPiece;
            startTile.piece.pos = startTile.pos;
        }

        //Create temporary state by replacing end tile's piece with the one originating from start tile
        endTile.piece = startTile.piece;
        endTile.piece.pos = endTile.pos;
        startTile.piece = null;


        for (int i = 0; i < 64; i++)
        {
            if (board.state[i].piece != null && board.state[i].piece.isWhite != isWhite)
            {
                if (!board.state[i].piece.isAPawn) //Enemy pawns don't attack king according to their possible moves
                {
                    List<Tile> possibleMoves = board.state[i].piece.PossibleMoves();
                    if(possibleMoves != null)
                        enemyPossibleMoves.AddRange(possibleMoves);
                }
                else //King can't move where pawns may attack
                {
                    List<Tile> pawnAttacks = board.state[i].piece.isAPawn.Attacks();
                    if (pawnAttacks != null)
                        enemyPossibleMoves.AddRange(pawnAttacks);
                }
            }
        }

        for (int i = 0; i < enemyPossibleMoves.Count; i++) //Compare possible enemy moves with king's position
        {
            if (enemyPossibleMoves[i] == endTile) //If an enemy attacks endTile, movement is not allowed
            {
                ReturnBoardToNormal();
                return true;
            }
        }

        ReturnBoardToNormal(); //***NOTE: I think I have to reset the board before approving the movement (not 100% sure though, this might not be necessary, will have to test)
        return false;
    }

    bool PerformCastling(Tile tile)
    {
        //tile will be one left of right rook or two right of left rook
        //Ensure there are empty tiles on the way there
        //Ensure there are no enemies attacking the empty tiles
        //Ensure king and chosen rook haven't moved all game
        int distToRookTile = tile.pos.x + 1 - pos.x; //Castling would be triggered by moving king two tiles right or two tiles left
//        if (!moved && (tile.pos.x + 1 - pos.x == 3 || distToRookTile == 4) && tile.piece)

            return false;
    }

    public override List<Tile> PossibleMoves()
    {
        List<Tile> moves = new List<Tile>();

        Tile[] possibleMoves = { //8 tiles around King
            board.TileAt(pos + new Vector2Int(1, 1)), board.TileAt(pos + new Vector2Int(1, 0)), board.TileAt(pos + new Vector2Int(1, -1)), board.TileAt(pos + new Vector2Int(0, -1)),
            board.TileAt(pos + new Vector2Int(-1, -1)), board.TileAt(pos + new Vector2Int(-1, 0)), board.TileAt(pos + new Vector2Int(-1, 1)), board.TileAt(pos + new Vector2Int(0, 1))
        };

        for(int i = 0; i < 8; i++)
        {
            if (possibleMoves[i] != null && (!IsAnAlly(possibleMoves[i]) || IsAnEnemy(possibleMoves[i])))
                moves.Add(board.TileAt(possibleMoves[i].pos));
        }
        return moves;
    }
}