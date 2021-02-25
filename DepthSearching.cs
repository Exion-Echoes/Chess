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
        if (!counted)
        {
            List<PieceMoves> w0Moves = FindAllPossibleMoves(b.state, true);

            int w0Count = 0;
            int b0Count = 0;
            int w1Count = 0;
            int b1Count = 0;
            int w2Count = 0;

            for (int i = 0; i < w0Moves.Count; i++)
            {
                for (int j = 0; j < w0Moves[i].possibleMoves.Count; j++)
                {
                    w0Count++; //Count ply = 1 movements from the state

                    //Create temporary state
                    Piece w0P = w0Moves[i].possibleMoves[j].piece;
                    b.state[w0Moves[i].possibleMoves[j].id].piece = w0Moves[i].t.piece;
                    b.state[w0Moves[i].possibleMoves[j].id].piece.pos = w0Moves[i].possibleMoves[j].pos;
                    b.state[w0Moves[i].t.id].piece = null;

                    //Calculate on temporary state
                    List<PieceMoves> b0Moves = FindAllPossibleMoves(b.state, false);

                    for(int ii = 0; ii < b0Moves.Count; ii++)
                    {
                        for(int jj = 0; jj < b0Moves[ii].possibleMoves.Count; jj++)
                        {
                            b0Count++;

                            //Create temporary state
                            Piece b0P = b0Moves[ii].possibleMoves[jj].piece;
                            b.state[b0Moves[ii].possibleMoves[jj].id].piece = b0Moves[ii].t.piece;
                            b.state[b0Moves[ii].possibleMoves[jj].id].piece.pos = b0Moves[ii].possibleMoves[jj].pos;
                            b.state[b0Moves[ii].t.id].piece = null;

                            //Calculate on temporary state
                            List<PieceMoves> w1Moves = FindAllPossibleMoves(b.state, true);

                            for(int iii = 0; iii < w1Moves.Count; iii++)
                            {
                                for(int jjj = 0; jjj < w1Moves[iii].possibleMoves.Count; jjj++)
                                {
                                    w1Count++;

                                    //Create a temporary state
                                    Piece w1P = w1Moves[iii].possibleMoves[jjj].piece;
                                    b.state[w1Moves[iii].possibleMoves[jjj].id].piece = w1Moves[iii].t.piece;
                                    b.state[w1Moves[iii].possibleMoves[jjj].id].piece.pos = w1Moves[iii].possibleMoves[jjj].pos;
                                    b.state[w1Moves[iii].t.id].piece = null;

                                    //Calculate on temporary state
                                    List<PieceMoves> b1Moves = FindAllPossibleMoves(b.state, false);

                                    for(int iiii = 0; iiii < b1Moves.Count; iiii++)
                                    {
                                        for(int jjjj = 0; jjjj < b1Moves[iiii].possibleMoves.Count; jjjj++)
                                        {
                                            b1Count++;

                                            //
                                        }
                                    }
                                    //Return to original state
                                    b.state[w1Moves[iii].t.id].piece = b.state[w1Moves[iii].possibleMoves[jjj].id].piece;
                                    b.state[w1Moves[iii].t.id].piece.pos = w1Moves[iii].t.pos;
                                    b.state[w1Moves[iii].possibleMoves[jjj].id].piece = w1P;
                                }
                            }
                            //Return to original state
                            b.state[b0Moves[ii].t.id].piece = b.state[b0Moves[ii].possibleMoves[jj].id].piece;
                            b.state[b0Moves[ii].t.id].piece.pos = b0Moves[ii].t.pos;
                            b.state[b0Moves[ii].possibleMoves[jj].id].piece = b0P;
                        }
                    }
                    //Return to original state
                    b.state[w0Moves[i].t.id].piece = b.state[w0Moves[i].possibleMoves[j].id].piece;
                    b.state[w0Moves[i].t.id].piece.pos = w0Moves[i].t.pos;
                    b.state[w0Moves[i].possibleMoves[j].id].piece = w0P;
                }
            }

            Debug.Log(w0Count + ", " + b0Count + ", " + w1Count + ", " + b1Count + ", " + w2Count); //w1Count should be 8902
            
            counted = true;
        }
    }

    List<PieceMoves> FindAllPossibleMoves(Tile[] state, bool wPly) //Finds possible moves for a given board state for the player of isWhite = wPly
    {
//        Debug.Log("lmao");
        List<PieceMoves> pM = new List<PieceMoves>();
//        Debug.Log("AYYYYYYY LMAO " + b.state[18].piece + ", " + b.state[1].piece);// + ", " + b.state[18].piece.PossibleMoves().Count);
        for (int i = 0; i < 64; i++)
        {
            if (state[i].piece != null && state[i].piece.isWhite == wPly)
            {
  //              if (state[i].piece.pos == new Vector2Int(2, 2))
//                    Debug.Log("ayy lmao");
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
//                if (state[i].piece.pos == new Vector2Int(2, 2))
  //                  Debug.Log(state[i].id + ", " + state[i].piece.pos + ", " + state[i].piece + ", " + allMoves.Count + ", " + allowedMoves.Count);
                pM.Add(new PieceMoves(state[i], allowedMoves));
            }
        }
        return pM;
    }
}

class PieceMoves
{
    public Tile t;
    public List<Tile> possibleMoves;

    public PieceMoves(Tile t, List<Tile> possibleMoves)
    {
        this.t = t;
        this.possibleMoves = possibleMoves;
    }
}