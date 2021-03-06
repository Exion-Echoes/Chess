﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public bool moved;
    public bool isChecked; //Limits movements this turn

    public override void Start()
    {
        base.Start();
    }

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
        return PerformCastling(startTile, endTile);
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
                if (!board.state[i].piece.isPawn) //Enemy pawns don't attack king according to their possible moves
                {
                    List<Tile> possibleMoves = board.state[i].piece.PossibleMoves();
                    if(possibleMoves != null)
                        enemyPossibleMoves.AddRange(possibleMoves);
                }
                else //King can't move where pawns may attack
                {
                    List<Tile> pawnAttacks = board.state[i].piece.isPawn.Attacks();
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

    bool PerformCastling(Tile s, Tile e)
    {
        List<Tile> enemyPossibleAttacks = DetermineEnemyAttacks();

        if (!moved)
        {
            if (e.pos == new Vector2Int(2, pos.y)) //If trying to castle to the left
            {
                if (isWhite ? board.lWRookTile.piece != null && board.lWRookTile.piece.isRook != null : board.lBRookTile.piece != null && board.lBRookTile.piece.isRook != null)
                {
                    Rook lRook = (isWhite ? board.lWRookTile.piece.isRook : board.lBRookTile.piece.isRook);
                    if (!lRook.moved) //Can only castle if none of the two pieces moved
                    {
                        //See if tiles beside king are occupied
                        Tile tileOne = board.TileAt(new Vector2Int(pos.x - 1, pos.y));
                        Tile tileTwo = board.TileAt(new Vector2Int(pos.x - 2, pos.y));
                        Tile tileThree = board.TileAt(new Vector2Int(pos.x - 3, pos.y));
                        if (tileOne.piece != null || tileTwo.piece != null || tileThree.piece != null)
                            return false;
                        //Check if any enemy are attacking the three tiles from the King to where it wants to castle
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < enemyPossibleAttacks.Count; j++)
                            {
                                if (enemyPossibleAttacks[j] == board.TileAt(pos + new Vector2Int(-i, 0)))
                                    return false;
                            }
                        }
                        return true; //Castling is allowed
                    }
                }
            }
            if (e.pos == new Vector2Int(6, pos.y)) //If trying to castle to the right
            {
                if (isWhite ? board.rWRookTile.piece != null && board.rWRookTile.piece.isRook != null : board.rBRookTile.piece != null && board.rBRookTile.piece.isRook != null)
                {
                    Rook rRook = (isWhite ? board.rWRookTile.piece.isRook : board.rBRookTile.piece.isRook);
                    if (!rRook.moved) //Can only castle if none of the two pieces moved
                    {
                        //See if tiles beside king are occupied
                        Tile tileOne = board.TileAt(new Vector2Int(pos.x + 1, pos.y));
                        Tile tileTwo = board.TileAt(new Vector2Int(pos.x + 2, pos.y));
                        if (tileOne.piece != null || tileTwo.piece != null)
                            return false;
                        //Check if any enemy are attacking the three tiles from the King to where it wants to castle
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < enemyPossibleAttacks.Count; j++)
                            {
                                if (enemyPossibleAttacks[j] == board.TileAt(pos + new Vector2Int(i, 0)))
                                    return false;
                            }
                        }
                        return true;
                    }
                }
            }
        }
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

    public void CheckIfMoved(Piece p, Tile s, Tile e)
    {
        if (p == this)
        {
            moved = true;
            board.moveNotification -= CheckIfMoved;
        }
    }

    public void UpdateKingTile(Piece p, Tile s, Tile e)
    {
        if (p == this)
        {
            if (s == board.wKTile) //Check if king tile needs to be updated
                board.wKTile = e;
            else if (s == board.bKTile)
                board.bKTile = e;
        }
    }
}