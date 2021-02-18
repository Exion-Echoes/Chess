using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isWhite;
    public Tile startTile;
    public Board board;

    public virtual void Awake()
    {
        board = FindObjectOfType<Board>();
    }
    public virtual void PickAPiece()
    {
        //
    }
    public virtual void DropAPiece()
    {
        //
    }
    public virtual void PromotePawn()
    {
        //
    }
}
