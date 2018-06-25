using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] int _worldSize = 128;

    Dictionary<Vector2DInt, Tile> _tiles = new Dictionary<Vector2DInt, Tile>();

    View _view;


    void Awake()
    {
        _view = new View(gameObject);
    }


    public class View
    {
        readonly Material _material;

        readonly int _vertexSize;
        readonly int _vertexCount;

        readonly int[] _triangles;
        readonly Vector3[] _vertices;

        readonly MeshFilter   _meshFilter;
        readonly MeshRenderer _meshRenderer;


        public View(GameObject inViewGO)
        {
            _material = (Material)Resources.Load("Material_World", typeof(Material));

            _meshFilter   = inViewGO.AddComponent<MeshFilter>();
            _meshRenderer = inViewGO.AddComponent<MeshRenderer>();

            _meshFilter.mesh = GenerateMesh();
        }


        Mesh GenerateMesh()
        {
            Mesh newMesh = new Mesh();



            return newMesh;
        }
    }
}

public class Tile
{
    
}



