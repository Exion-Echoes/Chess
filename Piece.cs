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

    public SpriteRenderer sr;
    public Board board;

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    public virtual void Start()
    {
        isAPawn = GetComponent<Pawn>();
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
                List<Tile> possibleMoves = board.state[i].piece.PossibleMoves();
                if (!board.state[i].piece.isAPawn && possibleMoves != null) //Enemy pawns can't check a king by reveal
                    enemyPossibleMoves.AddRange(possibleMoves);
            }
        }

        for(int i = 0; i < enemyPossibleMoves.Count; i++) //Compare possible enemy moves with king's position
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


    #region
    /*
    public bool canBeMoved; //This will be determined sequentially whenever player tries to pick up a piece
    public bool isWhite; //If false, then piece is black
    public bool beingCarried; //State of piece when it's being held by player
    public Vector2Int boardCoords; //[0,0] = A1, [0,1] = A2, etc.
    public Vector2Int pastBoardCoords;
    public List<Vector2Int> allowedDestinations = new List<Vector2Int>();
    public List<Vector2Int> positionsDefended = new List<Vector2Int>(); //Friendly pieces attacked by this (Determine if King can eat a piece or not)
    public List<Vector2Int> linesOfAttack = new List<Vector2Int>(); //Spaces that a piece may attack from beyond first piece to the next (Determine checks by reveal)
    public bool isOnBoard;
    public bool beginLinesOfAttack; //Instead of breaking out of the old for loops, turn on this bool instead and keep going to determine lines of attack (where applicable)

    public Board board;
    [HideInInspector]
    public King king;
    [HideInInspector]
    public King enemyKing;

    [HideInInspector]
    public Pawn isAPawn;
    [HideInInspector]
    public Rook isARook;
    [HideInInspector]
    public Queen isAQueen;
    [HideInInspector]
    public Bishop isABishop;
    [HideInInspector]
    public Knight isAKnight;
    [HideInInspector]
    public King isAKing;

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();

        isAPawn = GetComponent<Pawn>();
        isAKing = GetComponent<King>();
        isARook = GetComponent<Rook>();
        isAKnight = GetComponent<Knight>();
        isABishop = GetComponent<Bishop>();
        isAQueen = GetComponent<Queen>();
    }

    public virtual void Start()
    {
        king = isWhite ? board.wPieces[0].isAKing : board.bPieces[0].isAKing;
        enemyKing = isWhite ? board.bPieces[0].isAKing : board.wPieces[0].isAKing;
    }

    public virtual void InitialAllowedDestinations()
    {
        //
    }

    public bool IsThereAnEnemy(Vector2Int pos)
    {
        Piece piece = board.GetBoardPiece(pos);
        if (piece != null && piece.isWhite != isWhite)
            return true;
        return false;
    }

    public bool IsThereAnAlly(Vector2Int pos)
    {
        Piece piece = board.GetBoardPiece(pos);
        if (piece != null && piece.isWhite == isWhite)
            return true;
        return false;
    }

    public bool IsThisOOB(Vector2Int coords) //Are given coordinates out of bounds, i.e. not within {[0,7] , [0,7]}?
    {
        if (coords.x < 0 || coords.x > 7 || coords.y < 0 || coords.y > 7)
            return true;
        return false;
    }

    public bool IsOwnKingChecked(Vector2Int testPos) //Determines whether King is checked after this.boardCoords is emptied
    {
        Vector2Int kingPos = king.boardCoords; //Check in this direction for potential checks from the enemy
        Vector2Int testDirection = Vector2Int.zero;

        //Check if revealing the king would check it
        Vector2Int EnemyInTheWay (bool checkQueen = false, bool checkBishop = false, bool checkRook = false)
        {
            bool IsAQueen(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isAQueen != null)
                    return true;
                return false;
            }
            bool IsARook(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isARook != null)
                    return true;
                return false;
            }
            bool IsABishop(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isABishop != null)
                    return true;
                return false;
            }

            for (int i = 1; i <= 7; i++)
            {
                Vector2Int testBoardCoords = king.boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
                Piece piece = board.GetBoardPiece(testBoardCoords);
                if (IsThereAnEnemy(testBoardCoords) && (IsAQueen(checkQueen, piece) || IsARook(checkRook, piece) || IsABishop(checkBishop, piece)))
                    return testBoardCoords;

                else if (IsThereAnAlly(testBoardCoords) && piece != this) //This lets the function exit the for loop earlier sometimes, but otherwise isn't necessary
                    return new Vector2Int(-1,-1);
            }
            return new Vector2Int(-1, -1);
        }

        if (Mathf.Abs(boardCoords.x - kingPos.x) == Mathf.Abs(boardCoords.y - kingPos.y) && boardCoords.x != kingPos.x && boardCoords.y != kingPos.y)
        {
            testDirection = new Vector2Int((int) Mathf.Sign(boardCoords.x - kingPos.x), (int) Mathf.Sign(boardCoords.y - kingPos.y));
            Vector2Int enemyLocation = EnemyInTheWay(true, true);
            Vector2Int testPosDirection = new Vector2Int((int)Mathf.Sign(testPos.x - kingPos.x), (int)Mathf.Sign(testPos.y - kingPos.y));

            //**
            //**
            //**
            //**
            //DIAGONAL DOESN'T WORK!!
            //**
            //**
            //**
            //**

            if (enemyLocation == new Vector2Int(-1, -1) || testPosDirection != testDirection) //No king check in a situation where there's no enemy lined up
                return false;

            //Check if piece is between king and enemy piece, diagonally
            if (testPosDirection == testDirection && (testPos.y - kingPos.y < enemyLocation.y - kingPos.y) && (testPos.x - kingPos.x < enemyLocation.x - kingPos.x))
                return false;

            else if (testPos.x != boardCoords.x && testPos.y != boardCoords.y)
                return true;
        }
        else if (boardCoords.x == kingPos.x) //Vertical checks (Rooks and Queen)
        {
            testDirection = new Vector2Int(0, (int)Mathf.Sign(boardCoords.y - kingPos.y));
            Vector2Int enemyLocation = EnemyInTheWay(true, false, true);

            if (enemyLocation == new Vector2Int(-1, -1)) //No king check in a situation where there's no enemy lined up
                return false;

            else if (testPos.x == boardCoords.x && (testPos.y - kingPos.y < enemyLocation.y - kingPos.y))
                return false;

            else if(testPos.x != boardCoords.x) //This is ultimately to forbid such movements
                return true;
            //IT LOOKS LIKE THIS WORKS FOR A QUEEN, BUT I NOW HAVE TO APPLY IT TO DIAGONAL AND HORIZONTAL, AND TEST IT FURTHER
            //I THIGNK I NEED TO CHANGE THE BOOL TO A VECTOR2INT SO THAT IT RETURNS AN ENEMY PIECE, OR NOTHING IF NO ENEMY IS FOUND, IN WHICH CASE THE ISCHECK RETURNS FALSE RIGHT AWAY
            //WHEN AN ENEMY IS FOUND, THEN IT HAS TO BE TESTED AGAISNT TESTPOS, AFTER WHICH THE ISCHECK RETURNS TRUE OR FALSE
        }
        else if (boardCoords.y == kingPos.y)
        {
            testDirection = new Vector2Int((int)Mathf.Sign(boardCoords.x - kingPos.x), 0);
            Vector2Int enemyLocation = EnemyInTheWay(true, false, true);

            if (enemyLocation == new Vector2Int(-1, -1)) //No king check in a situation where there's no enemy lined up
                return false;

            else if (testPos.y == boardCoords.y && (testPos.x - kingPos.x < enemyLocation.x - kingPos.x))
                return false;

            else if (testPos.y != boardCoords.y) //This is ultimately to forbid such movements
                return true;
        }
        return false;

        #region old code
        /*
         *     //    if (this.name == "bBishop_0")
      //      Debug.Log("lmao " + travelDistance);
        for (int i = 1; i <= 7; i++)
        {
            Vector2Int testBoardCoords = boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
            Piece piece = board.GetBoardPiece(testBoardCoords);
            if (IsThereAnEnemy(testBoardCoords))
            {
                for (int j = 0; j < piece.linesOfAttack.Count; j++)
                {
//                    if (this.name == "bBishop_0")
  //                      Debug.Log(piece + ", " + j + ", " + piece.linesOfAttack[j] + ", " + king.boardCoords);
//                    if (king.boardCoords == piece.linesOfAttack[j])
  //                      return true;
                }
            }
//            if (IsThereAnAlly(testBoardCoords) || IsThisOOB(testBoardCoords))
//                return false;
        }

        //Check if moving to testPos stops a check
        //
        //
        //

        //Check if opening up pastPos would make the king checked, in which case return true

        //Check if moving to pos would stop the king from being checked, in which case return false

        //MAKE KING MOVEMENT DETERMINATIONS
        //CONSIDER THE CHECK CASE DESCRIBED BELOW
        //ADD ISOWNKINGCHECKED FUNCTION SO IT WORKS PROPERLY

        //The case where the king is the piece that checks if it moves into a checked position is resolved in the King script
        //...Every other piece's allowed destination is determined before the king's, so the allowed destinations may be referenced along with the king's test positions
                 Vector2Int kingPos = king.boardCoords; //Check in this direction for potential checks from the enemy

//        int travelDistance = 0;
        Vector2Int testDirection = Vector2Int.zero;

        //Check if revealing the king would check it
        bool EnemyInTheWay (bool checkQueen = false, bool checkBishop = false, bool checkRook = false)
        {
            bool IsAQueen(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isAQueen != null)
                    return true;
                return false;
            }
            bool IsARook(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isARook != null)
                    return true;
                return false;
            }
            bool IsABishop(bool checkThis, Piece piece)
            {
                if (checkThis && piece.isABishop != null)
                    return true;
                return false;
            }

//            if (this.name == "wPawn_4" || this.name == "wPawn_6")
//                Debug.Log(this + ", " + travelDistance + ", " + testDirection);

            //**
            //**Use testPos for this and see if that's a check too, if not, then it's an allowed movement and the function must return false
            //**Currently this feature's not included
            //**I want to check if this is between king and enemy
            //**

            for (int i = 1; i <= 7; i++)
            {
                Vector2Int testBoardCoords = king.boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
                Piece piece = board.GetBoardPiece(testBoardCoords);
                if (IsThereAnEnemy(testBoardCoords) && (IsAQueen(checkQueen, piece) || IsARook(checkRook, piece) || IsABishop(checkBishop, piece)))
                    return true;

                else if (IsThereAnAlly(testBoardCoords) && piece != this) //This lets the function exit the for loop earlier sometimes, but otherwise isn't necessary
                    return false;
            }
            return false;
        }

        if (Mathf.Abs(boardCoords.x - kingPos.x) == Mathf.Abs(boardCoords.y - kingPos.y) && boardCoords.x != kingPos.x && boardCoords.y != kingPos.y)
        {
//            travelDistance = Mathf.Abs(boardCoords.x - kingPos.x);
            testDirection = new Vector2Int((int) Mathf.Sign(boardCoords.x - kingPos.x), (int) Mathf.Sign(boardCoords.y - kingPos.y));
            return EnemyInTheWay(true) || EnemyInTheWay(false, true); //If a Queen or a Bishop attacks, return true
        }
        else if (boardCoords.x == kingPos.x)
        {
//            travelDistance = Mathf.Abs(boardCoords.y - kingPos.y);
            if(testPos.x == boardCoords.x)
            {
//                if (testPos.y - kingPos.y < )
            }
            //Attempt at making

            testDirection = new Vector2Int(0, (int)Mathf.Sign(boardCoords.y - kingPos.y));
            //            return EnemyInTheWay(true) || EnemyInTheWay(false, false, true); //If a Queen or a Rook attacks, return true
            //Vertical/Horizontal check - rooks and queen


            Vector2Int enemyLocation = new Vector2Int(-1,-1);
            for (int i = 1; i <= 7; i++)
            {
                Vector2Int testBoardCoords = king.boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
                Piece piece = board.GetBoardPiece(testBoardCoords);
                if (IsThereAnEnemy(testBoardCoords) && (piece != null && piece.isAQueen != null || piece.isARook != null))
                {
                    enemyLocation = testBoardCoords;
                    break;
                }
                //                    return true;

//                else if (IsThereAnAlly(testBoardCoords) && piece != this) //This lets the function exit the for loop earlier sometimes, but otherwise isn't necessary
//                    return false;
            }

            if (enemyLocation == new Vector2Int(-1, -1)) //If no enemy was found, then return false right away
                return false;

            if (testPos.x == boardCoords.x)
            {
                if (testPos.y - kingPos.y < enemyLocation.y - kingPos.y)
                    return false;
            }

            if(testPos.x != boardCoords.x)
                return true;
            //IT LOOKS LIKE THIS WORKS FOR A QUEEN, BUT I NOW HAVE TO APPLY IT TO DIAGONAL AND HORIZONTAL, AND TEST IT FURTHER
            //I THIGNK I NEED TO CHANGE THE BOOL TO A VECTOR2INT SO THAT IT RETURNS AN ENEMY PIECE, OR NOTHING IF NO ENEMY IS FOUND, IN WHICH CASE THE ISCHECK RETURNS FALSE RIGHT AWAY
            //WHEN AN ENEMY IS FOUND, THEN IT HAS TO BE TESTED AGAISNT TESTPOS, AFTER WHICH THE ISCHECK RETURNS TRUE OR FALSE
        }
        else if (boardCoords.y == kingPos.y)
        {
//            travelDistance = Mathf.Abs(boardCoords.x - kingPos.x);
            testDirection = new Vector2Int((int)Mathf.Sign(boardCoords.x - kingPos.x), 0);
            return EnemyInTheWay(true) || EnemyInTheWay(false, false, true); //If a Queen or a Rook attacks, return true
        }

    //    if (this.name == "bBishop_0")
      //      Debug.Log("lmao " + travelDistance);
        for (int i = 1; i <= 7; i++)
        {
            Vector2Int testBoardCoords = boardCoords + new Vector2Int(testDirection.x * i, testDirection.y * i);
            Piece piece = board.GetBoardPiece(testBoardCoords);
            if (IsThereAnEnemy(testBoardCoords))
            {
                for (int j = 0; j < piece.linesOfAttack.Count; j++)
                {
//                    if (this.name == "bBishop_0")
  //                      Debug.Log(piece + ", " + j + ", " + piece.linesOfAttack[j] + ", " + king.boardCoords);
//                    if (king.boardCoords == piece.linesOfAttack[j])
  //                      return true;
                }
            }
//            if (IsThereAnAlly(testBoardCoords) || IsThisOOB(testBoardCoords))
//                return false;
        }

        //Check if moving to testPos stops a check
        //
        //
        //

        //Check if opening up pastPos would make the king checked, in which case return true

        //Check if moving to pos would stop the king from being checked, in which case return false

        //MAKE KING MOVEMENT DETERMINATIONS
        //CONSIDER THE CHECK CASE DESCRIBED BELOW
        //ADD ISOWNKINGCHECKED FUNCTION SO IT WORKS PROPERLY

        //The case where the king is the piece that checks if it moves into a checked position is resolved in the King script
        //...Every other piece's allowed destination is determined before the king's, so the allowed destinations may be referenced along with the king's test positions
        return false;
         */
    #endregion
}

//public virtual void UpdateAllowedDestinations()
//  {
//Check for allied / enemy pieces and check if moving at the test position would put own king in check
//    }
