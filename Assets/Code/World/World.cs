using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class World : MonoBehaviour
{
    static World _instance;
    public static World instance => _instance ?? (_instance = FindObjectOfType<World>());
  
    [SerializeField] int _size = 512;

    int _vertexSize;
    int _vertexCount;

    Tile[,] _tiles;

    public Tile GetTile(Vector2DInt inPosition) => _tiles[inPosition.x, inPosition.y];

    Mesh _worldMesh;

    void Awake()
    {
        GenerateTiles();
        GenerateMesh();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            GenerateTiles();
            _worldMesh.uv2 = GenerateUV2();
        }

        if (Input.GetKey(KeyCode.R))
        {
            GenerateTiles();
            _worldMesh.uv2 = GenerateUV2();
        }
    }

    void GenerateTiles()
    {
        if (_tiles == null)
            _tiles = new Tile[_size, _size];

        for (int y = 0; y < _size; y++)
            for (int x = 0; x < _size; x++)
            {
                Vector2DInt position = new Vector2DInt(x, y);
                _tiles[x, y] = new Tile(position, new Terrain((TerrainType)Random.Range(0, 2)));
            }
    }


    void GenerateMesh()
    {
        _worldMesh = new Mesh();
        _worldMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        _vertexSize = _size * 2;
        _vertexCount = _vertexSize * _vertexSize * 4;

        _worldMesh.vertices = GenerateVertices();
        _worldMesh.triangles = GenerateTriangleIDs();
        _worldMesh.uv2 = GenerateUV2();

        gameObject.GetComponent<MeshFilter>().mesh = _worldMesh;
    }


    int[] GenerateTriangleIDs()
    {
        int[] triangles = new int[_size * _size * 6];
        int currentQuad = 0;
        for (int y = 0; y < _vertexSize; y += 2)
            for (int x = 0; x < _vertexSize; x += 2)
            {
                int triangleOffset = currentQuad * 6;
                int currentVertex = y * _vertexSize + x;

                triangles[triangleOffset + 0] = currentVertex + 0;                 // Bottom - Left
                triangles[triangleOffset + 1] = currentVertex + _vertexSize + 1;   // Top    - Right
                triangles[triangleOffset + 2] = currentVertex + 1;                 // Bottom - Right

                triangles[triangleOffset + 3] = currentVertex + 0;                 // Bottom - Left
                triangles[triangleOffset + 4] = currentVertex + _vertexSize + 0;   // Top    - Left
                triangles[triangleOffset + 5] = currentVertex + _vertexSize + 1;   // Top    - Right

                currentQuad++;
            }

        return triangles;
    }

    Vector3[] GenerateVertices()
    {
        Vector3[] vertices = new Vector3[_vertexCount];
        int vertexID = 0;
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                // Generate a quad 
                vertices[vertexID + 0].x = x;
                vertices[vertexID + 0].z = y;

                vertices[vertexID + 1].x = x + 1;
                vertices[vertexID + 1].z = y;

                vertices[vertexID + _vertexSize + 0].x = x;
                vertices[vertexID + _vertexSize + 0].z = y + 1;

                vertices[vertexID + _vertexSize + 1].x = x + 1;
                vertices[vertexID + _vertexSize + 1].z = y + 1;

                vertexID += 2;
            }
            vertexID += _vertexSize;
        }

        return vertices;
    }

    public Vector2[] GenerateUV2()
    {
        Vector2[] newUV2s = new Vector2[_vertexCount];

        int vertexID = 0;
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                int tileTextureID = _tiles[x, y].terrain.data.textureID;

                newUV2s[vertexID + 0]               = new Vector2(tileTextureID, tileTextureID);
                newUV2s[vertexID + 1]               = new Vector2(tileTextureID, tileTextureID);
                newUV2s[vertexID + _vertexSize + 0] = new Vector2(tileTextureID, tileTextureID);
                newUV2s[vertexID + _vertexSize + 1] = new Vector2(tileTextureID, tileTextureID);

                vertexID += 2;
            }
            vertexID += _vertexSize;
        }

        return newUV2s;
    }
}


public class Tile
{
    public readonly Vector2DInt worldPosition; 

    public readonly Node node;
    public readonly Terrain terrain;


    public Tile(Vector2DInt inPosition, Terrain inTerrain)
    {
        worldPosition = inPosition;

        terrain = inTerrain;

        node = new Node(this);
    }


    public List<Tile> GetNeighbours()
    {
        List<Tile> neighbours = new List<Tile>();

        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Up));
        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Left));
        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Right));
        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Down));
        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Up + Vector2DInt.Left));
        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Up + Vector2DInt.Right));
        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Down + Vector2DInt.Left));
        neighbours.AddIfNotNull(GetRelativeTile(Vector2DInt.Down + Vector2DInt.Right));

        return neighbours;
    }


    public Tile GetRelativeTile(Vector2DInt inOffset) =>
        Object.FindObjectOfType<World>().GetTile(worldPosition + inOffset);
}


public struct Terrain
{
    static Dictionary<TerrainType, TerrainData> _staticTerrainData = new Dictionary<TerrainType, TerrainData>()
    {
        { TerrainType.Grass, new TerrainData(inTextureID: 2, inMoveSpeedModifier: 1.0f, inPassable: true) },
        { TerrainType.Sand,  new TerrainData(inTextureID: 1, inMoveSpeedModifier: 0.5f, inPassable: true) },
    };

    public readonly TerrainType type;

    public TerrainData data => _staticTerrainData[type];


    public Terrain(TerrainType inType)
    {
        type = inType;
    }
}

public class TerrainData
{
    public readonly int   textureID;
    public readonly float moveSpeedModifier;
    public readonly bool  passable; // TODO: Replace this with some kinda bit flag

    public TerrainData(int inTextureID, float inMoveSpeedModifier, bool inPassable)
    {
        textureID         = inTextureID;
        moveSpeedModifier = inMoveSpeedModifier;
        passable          = inPassable;
    }
}

public enum TerrainType
{
    Grass,
    Sand
}


public class Node
{
    public readonly Tile owner;
    public Tile parent;

    public int costToStart;
    public int distanceToEnd;

    public int totalCost => costToStart + distanceToEnd;


    public int DistanceTo(Tile inTargetTile)
    {
        int distanceX = Mathf.Abs(owner.worldPosition.x - inTargetTile.worldPosition.x);
        int distanceY = Mathf.Abs(owner.worldPosition.y - inTargetTile.worldPosition.y);

        return (distanceX > distanceY) ? 14 * distanceY + 10 * (distanceX - distanceY) :
                                         14 * distanceX + 10 * (distanceY - distanceX);
    }

    public int CostBetween(Tile inTargetTile) =>
        Mathf.RoundToInt(DistanceTo(inTargetTile) / inTargetTile.terrain.data.moveSpeedModifier);


    public Node(Tile inOwner)
    {
        owner = inOwner;
    }
}

public static class Pathfinder
{
    public static List<Tile> FindPath(Tile inStart, Tile inDestination)
    {
        List<Tile> closedTiles = new List<Tile>();
        List<Tile> openTiles = new List<Tile>() {
            inStart
        };

        while (openTiles.Count > 0)
        {
            Tile currentTile = GetTileWithLowestCost(openTiles);

            // If the current tile is the target tile, the path is completed
            if (currentTile == inDestination)
                return RetracePath(inStart, inDestination);

            // Add all walkable neighbours to "openTiles"
            List<Tile> neighbours = currentTile.GetNeighbours();
            foreach (Tile neighbour in neighbours)
            {
                if (!neighbour.terrain.data.passable)
                    continue;

                if (closedTiles.Contains(neighbour))
                    continue;

                // Calculate the neighbours cost from start
                int newNeighbourCostToStart = currentTile.node.costToStart + currentTile.node.CostBetween(neighbour);

                // If open tiles contains the neighbour and the new cost to start is higher than the existing, skip
                if (openTiles.Contains(neighbour))
                    if (newNeighbourCostToStart > neighbour.node.costToStart)
                        continue;

                // Since this is either a newly discovered tile or a tile with now better score, set all the node data and update the parent
                neighbour.node.costToStart = newNeighbourCostToStart;
                neighbour.node.distanceToEnd = neighbour.node.DistanceTo(inDestination);
                neighbour.node.parent = currentTile;

                // If it's newly discovered, add it as an open tile
                if (!openTiles.Contains(neighbour))
                    openTiles.Add(neighbour);
            }

            // This tile is now closed...
            closedTiles.Add(currentTile);
            openTiles.Remove(currentTile);
        }

        // If this is reached, no path was found. Return an empty list.
        return new List<Tile>();
    }


    static Tile GetTileWithLowestCost(List<Tile> inNodeList)
    {
        Tile currentTile = inNodeList[0];

        foreach (Tile openTile in inNodeList)
            if (openTile.node.totalCost <= currentTile.node.totalCost)
            {
                // If the open tile has the same total cost but is further away from the end, skip it
                if (openTile.node.totalCost == currentTile.node.totalCost)
                    if (openTile.node.distanceToEnd > currentTile.node.distanceToEnd)
                        continue;

                currentTile = openTile;
            }

        return currentTile;
    }


    static List<Tile> RetracePath(Tile inStartTile, Tile inTargetTile)
    {
        List<Tile> path = new List<Tile>();

        Tile currentTile = inTargetTile;
        while (currentTile != inStartTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.node.parent;
        }

        path.Reverse();

        return path;
    }
}