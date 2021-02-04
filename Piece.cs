using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool eaten;
    public bool isWhite;
    public Vector2Int pos;
    public List<Vector2Int> attacks = new List<Vector2Int>();
    public Pawn isAPawn;
    public Rook isARook;
    public King isAKing;

    public SpriteRenderer sr;
    public Board board;

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    public virtual void Start()
    {
        isAPawn = GetComponent<Pawn>();
        isARook = GetComponent<Rook>();
        isAKing = GetComponent<King>();
    }

    public virtual bool CanMove(Tile startTile, Tile endTile, bool stateTest = false)
    {
        if (IsAnAlly(endTile)) //Cannot end at an allied tile
            return false;

        bool moveAllowed = InTheList(PossibleMoves(), endTile);
        if (moveAllowed)
        {
            if (!MovingChecksOwnKing(startTile, endTile))
                return true;
        }
        return false;
        //Need to check opposing king after moving, if such a thing occured
        //stateTest is for cases where PossibleMoves() is sifted through completely, as opposed to a single move
        //  Without it, for every item in PossibleMoves(), I'd have to go through possibly the full list 
    }

    public bool IsAnAlly(Tile endTile)
    {
        if (endTile != null && endTile.piece != null && endTile.piece.isWhite == isWhite)// && !endTile.piece.hidden)
            return true;
        return false;
    }

    public bool IsAnEnemy(Tile endTile)
    {
        if (endTile != null && endTile.piece != null && endTile.piece.isWhite != isWhite)
            return true;
        return false;
    }

    public bool MovingChecksOwnKing(Tile startTile, Tile endTile)
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
                if (!board.state[i].piece.isAPawn)
                {
                    List<Tile> possibleMoves = board.state[i].piece.PossibleMoves();
                    if (possibleMoves != null) //Enemy pawns can't check a king by reveal
                        enemyPossibleMoves.AddRange(possibleMoves);
                }
                else
                {
                    List<Tile> pawnAttacks = board.state[i].piece.isAPawn.Attacks();
                    if (pawnAttacks != null) //If a pawn is currently attacking the king, it needs to be eaten, or king needs to be moved away
                        enemyPossibleMoves.AddRange(pawnAttacks);
                }
            }
        }

        for (int i = 0; i < enemyPossibleMoves.Count; i++) //Compare possible enemy moves with king's position
        {
            //            Debug.Log(i + ", " + enemyPossibleMoves[i].pos + ", " + board.TileAt(enemyPossibleMoves[i].pos).piece + ", " + (isWhite ? board.wKTile : board.bKTile).pos);
            if (enemyPossibleMoves[i] == (isWhite ? board.wKTile : board.bKTile))
            {
                ReturnBoardToNormal();
                return true;
            }
        }

        ReturnBoardToNormal(); //***NOTE: I think I have to reset the board before approving the movement (not 100% sure though, this might not be necessary, will have to test)
        return false;
    }

    public virtual List<Tile> PossibleMoves()
    {
        //These are compared CanMove() when determining the board state
        return null;
    }

    public bool InTheList(List<Tile> list, Tile endTile)
    {
        if (endTile != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (endTile.pos == list[i].pos)
                    return true;
            }
        }
        return false;
    }

    public List<Tile> DetermineEnemyAttacks()
    {
        List<Tile> enemyPossibleMoves = new List<Tile>();
        for (int i = 0; i < 64; i++)
        {
            if (board.state[i].piece != null && board.state[i].piece.isWhite != isWhite)
            {
                if (!board.state[i].piece.isAPawn) //Enemy pawns don't attack king according to their possible moves
                {
                    List<Tile> possibleMoves = board.state[i].piece.PossibleMoves();
                    if (possibleMoves != null)
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
        return enemyPossibleMoves;
    }
}