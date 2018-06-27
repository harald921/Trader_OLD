using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    Chunk[,] _chunks;

    void Awake()
    {

    }
}

public class Chunk
{
    public readonly Vector2DInt position;
    public Tile[,] _tiles;

    readonly GameObject _viewGO;

    public event Action<Chunk> OnTilesChanged;


    public Chunk(Vector2DInt inPosition, Tile[,] inTiles, GameObject inViewGO)
    {
        position = inPosition;
        _tiles = inTiles;

        _viewGO = inViewGO;

        OnTilesChanged?.Invoke(this);
    }
}

public class Tile
{

}

public struct Terrain
{
    public class Data
    {
        public readonly int   textureID;
        public readonly float moveSpeedMultiplier;
        public readonly bool  passable;


    }

    public enum Type
    {
        Grass,
        Sand
    }
}


public class ChunkGenerator
{
    GameObject GenerateBlankView()
    {
        return new GameObject();
    }

    int[,] GenerateUV2(Chunk inChunk)
    {

        return new int[0, 0];
    }
}