using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Vector2Int pos;
    public Piece piece;
    public int id; //[0,63]

    public Tile (Vector2Int pos, Piece piece, Sprite sprite, Vector3 unityPos, bool isWhite)
    {
        if (piece != null)
        {
            this.piece = piece;
            this.pos = this.piece.pos = pos;
            this.piece.gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
            this.piece.transform.position = unityPos;
            this.piece.isWhite = isWhite;
        }
        else
            this.pos = pos;
    }

    public void CopyTile(Tile tileBeingCopied)
    {
        this.pos = tileBeingCopied.pos;
        this.id = tileBeingCopied.id;
        this.piece = tileBeingCopied.piece;
    } //Might not need this
}