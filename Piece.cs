using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool canBeMoved; //This will be determined sequentially whenever player tries to pick up a piece
    public bool isWhite; //If false, then piece is black
    public bool beingCarried; //State of piece when it's being held by player
    public Vector2Int boardCoords; //[0,0] = A1, [0,1] = A2, etc.
    public Vector2Int[] allowedDestinations;

    public Board board;

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    public virtual void Rule()
    {
        //
    }
}