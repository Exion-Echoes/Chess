using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Functions to evaluate a board state and produce the number of possible states, with possibly ideas of which move's the best in a given situation

public class DepthSearching : MonoBehaviour
{
    //Need method to initialize different states (begin with current method, but go on to define a generalized method)
    //  Generalized method can use rnbqkbnr/pppppppp/8/8/8/... notation to produce pieces, with capital letters symbolizing white/black, and non-capital the other
    //Determine PossibleMoves() at multiple layers and compare with accepted results from the literature

    Board b;
    bool counted; //Counted the possible moves (required because DepthSearch is currently called in the main Update function of the game)
    bool wPly = true;

    Tile startTile;

    private void Start()
    {
        b = FindObjectOfType<Board>();
    }

    public void DepthSearch()
    {
        //Start with calculating possible moves for several layers at the basic state, then add starting state variation

        //b.state is the starting state - I should write a customizable state initiation function in here later on
        //Currently, b.state is the regular starting position

        //One ply = one player's turn (e.g. white's possible moves)
        //Two ply = second player's turn following one player's turn (so for each of white's moves, consider each of black's moves)
        //So on...

        //IN ESSENCE, I HAVE TO RUN THE SAME MOVEMENT FUNCTIONS AS THE PLAYER'S, BUT WITH LOGIC ON CHOOSING PIECES AND GOING THROUGH EACH POSSIBLE POSITION
        //EACH SUBSEQUENT LAYER MUST HAVE A PICK/DROP ASSIGNMENT PERFORMED TO CHANGE THE BOARD STATE, AS WE ONLY NEED INSTANCES OF CanMove() == TRUE

        //1st layer: Calculate all possible moves for white
        //2nd layer: 
        //  Pick & Drop first tile from list of possible moves for white
        //  Calulate all possible moves for black
        //  Pick & Drop tile moved in 1st step back to its starting position
        //  Repeat with second tile from list of possible moves for white, until the list is exhausted

        //"Calculate all possible moves" means Determine all PossibleMoves() and run them through CanMove()

//        For each moves of the 1st layer, calculate all possible moves from black
        //3rd layer: For each moves from the 2nd layer, calculate all possible moves from white

        //WORK THE FUNCTIONS FOR 2ND LAYER ONLY, THEN ADAPT FOR ANY LAYER
        //NEED TO CREATE TEMP STATES AND COUNTALLPOSSIBLEMOVES FROM THEM

        if (!counted)
        {
            List<PieceMoves> w0Moves = FindAllPossibleMoves(b.state, true);

            int w0Count = 0;
            int b0Count = 0;

            for(int i = 0; i < w0Moves.Count; i++)
            {
                for (int j = 0; j < w0Moves[i].possibleMoves.Count; j++)
                    w0Count++;
            }

            for (int i = 0; i < w0Moves.Count; i++) //Loop through pieces
            {
                for (int j = 0; j < w0Moves[i].possibleMoves.Count; j++) //Loop through possible moves of the ith piece
                {
                    Piece oldPiece = null;
                    if (w0Moves[i].possibleMoves[j].piece != null)
                        oldPiece = w0Moves[i].possibleMoves[j].piece;
                    w0Moves[i].possibleMoves[j].piece = w0Moves[i].p;
                    b.state[w0Moves[i].p.pos.x + 8 * w0Moves[i].p.pos.y].piece = null;

                    List<PieceMoves> b0Moves = FindAllPossibleMoves(b.state, false);

                    for (int ii = 0; ii < b0Moves.Count; ii++)
                    {
                        for (int jj = 0; jj < b0Moves[ii].possibleMoves.Count; jj++)
                        {
                            b0Count++;



                        }
                    }

                    //Reset temporary state
                    b.state[w0Moves[i].p.pos.x + 8 * w0Moves[i].p.pos.y].piece = w0Moves[i].p;
                    w0Moves[i].possibleMoves[j].piece = oldPiece;
                }
            }
            //now need to change board state for each of ply 1's moves and calculate each of black's moves (ply 2)
            //to change board state, need to adapt the functions below


            Debug.Log(w0Count + ", " + b0Count);

            counted = true;
        }
    }

    List<PieceMoves> FindAllPossibleMoves(Tile[] state, bool wPly) //Finds possible moves for a given board state for the player of isWhite = wPly
    {
        List<PieceMoves> pM = new List<PieceMoves>();
        for (int i = 0; i < 64; i++)
        {
            if (state[i].piece != null && state[i].piece.isWhite == wPly)
            {
                List<Tile> allMoves = state[i].piece.PossibleMoves();
                List<Tile> allowedMoves = new List<Tile>();
                if (allMoves.Count != 0)
                {
                    for (int j = 0; j < allMoves.Count; j++)
                    {
                        if (state[i].piece.CanMove(state[i], allMoves[j]))
                            allowedMoves.Add(allMoves[j]);
                    }
                }
                pM.Add(new PieceMoves(state[i].piece, allowedMoves));
            }
        }
        return pM;
    }
}

class PieceMoves
{
    public Piece p;
    public List<Tile> possibleMoves;

    public PieceMoves(Piece p, List<Tile> possibleMoves)
    {
        this.p = p;
        this.possibleMoves = possibleMoves;
    }
}