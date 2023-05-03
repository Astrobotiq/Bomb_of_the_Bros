using UnityEngine;

public struct Cell
{
    public enum Type
    {
        invalid,
        empty,
        number,
        mine,
        castle,
    }

    public Vector3Int position;
    public Type type;
    public int num;
    public bool isExploded;
    public bool isRevealed;
    public bool isFlagged;
    public bool isCastle;
    public bool isCastleOne;

}