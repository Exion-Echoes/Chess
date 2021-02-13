using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Vector2Int pos;
    public Piece piece;
    public int id; //[0,63] - id formula: (x,y) => x + 8 * y

    public Tile (Vector2Int pos, Piece piece, Sprite sprite, Vector3 unityPos, bool isWhite) //Constructor
    {
        this.pos = pos; //Tile is still created even if no piece is placed on top
        if (piece != null)
        {
            this.piece = piece;
            this.piece.pos = pos;
            this.piece.sr = this.piece.gameObject.AddComponent<SpriteRenderer>();
            this.piece.sr.sprite = sprite;
            this.piece.transform.position = unityPos;
            this.piece.isWhite = isWhite;
        }
    }
}