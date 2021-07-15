using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The planet face class creates a single (out of 6) faces for the planet
public class PlanetFace {
    private Mesh mesh;
    private int resolution;
    private Vector3 localUp;

    private Vector3 axisOne;
    private Vector3 axisTwo;

    private float radius;
    private Planet planet;

    //These will be filled with the generated data
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    private Chunk parentChunk;

    //The constructor
    public PlanetFace(Mesh _mesh, int _resolution, Vector3 _localUp, float _radius, Planet _planet)
    {
        //Instantiate the properties
        this.mesh = _mesh;
        this.resolution = _resolution;
        this.localUp = _localUp;
        this.radius = _radius;
        this.planet = _planet;

        //Calculate the other 2 directions the face is facing
        axisOne = new Vector3(localUp.y, localUp.z, localUp.x);
        axisTwo = Vector3.Cross(localUp, axisOne);
    }

    //Create a quadtree to allow for LOD
    public void CreateQuadTree() {
        //Reset the mesh
        vertices.Clear();
        triangles.Clear();

        //Create the parent chunk and it's children
        parentChunk = new Chunk(planet, null, null, localUp.normalized * planet.size, radius, 0, localUp, axisOne, axisTwo, resolution);
        parentChunk.CreateChildren();

        //Get chunk mesh data
        int triangleOffset = 0;
        foreach (Chunk child in parentChunk.GetVisibleChildren()) {
            (Vector3[], int[]) verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            triangleOffset += verticesAndTriangles.Item1.Length;
        }

        //Reset the mesh and add new data
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    //Update the quadtree
    public void UpdateQuadTree()
    {
        //Reset the mesh
        vertices.Clear();
        triangles.Clear();

        //Create the parent chunk and it's children
        parentChunk.UpdateChunk();

        //Get chunk mesh data
        int triangleOffset = 0;
        foreach (Chunk child in parentChunk.GetVisibleChildren())
        {
            (Vector3[], int[]) verticesAndTriangles = (new Vector3[0], new int[0]);
            if(child.vertices == null || child.vertices.Length == 0)
            {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            }
            else 
            {
                verticesAndTriangles = (child.vertices, child.GetTrianglesWithOffset(triangleOffset));
            }

            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            triangleOffset += verticesAndTriangles.Item1.Length;
        }

        //Reset the mesh and add new data
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}

//The chunk class will generate each chunk of the planet
public class Chunk {
    public Chunk[] children;
    public Chunk parent;
    public Vector3 position;
    public float radius;
    public int lod;
    public Vector3 localUp;
    public Vector3 axisOne;
    public Vector3 axisTwo;
    public int resolution;
    public Planet planet;
    
    public Vector3[] vertices;
    public int[] triangles;

    //The constructor
    public Chunk(Planet _planet, Chunk[] _children, Chunk _parent, Vector3 _position, float _radius, int _lod, Vector3 _localUp, Vector3 _axisOne, Vector3 _axisTwo, int _resolution)
    {
        this.planet = _planet;
        this.children = _children;
        this.parent = _parent;
        this.position = _position;
        this.radius = _radius;
        this.lod = _lod;
        this.localUp = _localUp;
        this.axisOne = _axisOne;
        this.axisTwo = _axisTwo;
        this.resolution = _resolution;
    }

    //Create the children of this chunk
    public void CreateChildren() {
        if(lod <= 8 && lod >= 0) {
            if(Vector3.Distance(position.normalized * planet.size, planet.player.position) <= planet.lodDistances[lod]) {
                //Assign the children of the chunk
                //Position is calculated on a cube and based on the fact that each child is half the radius of the parent
                children = new Chunk[4];
                children[0] = new Chunk(planet, new Chunk[0], this, position + axisOne * radius / 2 + axisTwo * radius / 2, radius / 2, lod + 1, localUp, axisOne, axisTwo, resolution);
                children[1] = new Chunk(planet, new Chunk[0], this, position + axisOne * radius / 2 - axisTwo * radius / 2, radius / 2, lod + 1, localUp, axisOne, axisTwo, resolution);
                children[2] = new Chunk(planet, new Chunk[0], this, position - axisOne * radius / 2 + axisTwo * radius / 2, radius / 2, lod + 1, localUp, axisOne, axisTwo, resolution);
                children[3] = new Chunk(planet, new Chunk[0], this, position - axisOne * radius / 2 - axisTwo * radius / 2, radius / 2, lod + 1, localUp, axisOne, axisTwo, resolution);
            }

            //Create grandchildren
            foreach(Chunk child in children)
                child.CreateChildren();
        }
    }

    //Return every chunk in the branch that needs to be rendered
    public Chunk[] GetVisibleChildren() {
        List<Chunk> toBeRendered = new List<Chunk>();

        //Calculate which chunk should be rendered
        if (Mathf.Acos((Mathf.Pow(planet.size, 2) + Mathf.Pow(planet.distanceToPlayer, 2) - Mathf.Pow(Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size), 
            planet.player.position), 2)) / (2 * planet.size * planet.distanceToPlayer)) < planet.cullingMinAngle) 
                toBeRendered.Add(this);

        //Return the array of all the chunks that need to be rendered
        return toBeRendered.ToArray();
    }

    //Calculate the vertices and triangles
    public (Vector3[], int[]) CalculateVerticesAndTriangles(int _triangleOffset)
    {
        //The resolution is N of vertices along a single edge, so the total resolution is resolution^2
        Vector3[] vertices = new Vector3[resolution * resolution];

        //Calculate how many triangles there will be in the mesh
        //The amount of faces is (resolution-1)^2, since there is 1 face less than vertices and there are 2 axis
        //Each face consists of 2 triangles, so the amount of triangles is (resolution-1)^2 * 2
        //Each triangle consists of 3 vertices which means the amount of vertices is (resolution-1)^2 * 2 * 3
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 2 * 3];

        //Store the indices
        int index = 0;
        int triangleIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                //Calculates the percentage of how close to completage the loop is,
                //which we can use to calculate where the vertex should be
                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                //Use the percentage to figure out where the vertex needs to be on the cube
                Vector3 vertexOnCube = position + ((percent.x - 0.5f) * 2 * axisOne + (percent.y - 0.5f) * 2 * axisTwo) * radius;

                //To turn the cube into a sphere, we want to inflate it by making every vertex the same distance away from the center
                Vector3 vertexOnSphere = vertexOnCube.normalized;

                //Set the current vertex to the newly calculated vertex position
                vertices[index] = vertexOnSphere;

                //Create triangle, aslong as the current vertex isn't along the right or bottom edge
                //in other words, as long as x isn't equal to r-1 and y isn't equal to r-1
                if (x != resolution - 1 && y != resolution - 1)
                {
                    //Update the first triangle
                    triangles[triangleIndex] = index + _triangleOffset; //The first vertex of the triangle is equal to the current vertex + triangle offset
                    triangles[triangleIndex + 1] = index + resolution + 1 + _triangleOffset; //The second vertex of the triangle is equal to the current vertex + r + 1 + triangle offset
                    triangles[triangleIndex + 2] = index + resolution + _triangleOffset; //The third vertex of the triangle is equal to the current vertex + r + triangle offset

                    //Update the second triangle
                    triangles[triangleIndex + 3] = index + _triangleOffset; //The first vertex of the triangle is equal to the current vertex + triangle offset
                    triangles[triangleIndex + 4] = index + 1 + _triangleOffset; //The second vertex of the triangle is equal to the current vertex + 1 + triangle offset
                    triangles[triangleIndex + 5] = index + resolution + 1 + _triangleOffset; //The third vertex of the triangle is equal to the current vertex + r + 1 + triangle offset

                    //Since we added 2 triangles, aka 6 vertices, we increment the triangle index by 6
                    triangleIndex += 6;
                }

                //Increment the index
                index++;

            }
        }

        this.vertices = vertices;
        this.triangles = triangles;

        return (vertices, GetTrianglesWithOffset(_triangleOffset));
    }

    //Update the chunk
    public void UpdateChunk()
    {
        float distanceToPlayer = Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size), planet.player.position);

        if(lod <= 8)
        {
            if(distanceToPlayer > planet.lodDistances[lod])
            {
                children = new Chunk[0];
            }
            else
            {
                if(children.Length > 0)
                    foreach (Chunk child in children)
                        child.UpdateChunk();
                else
                    CreateChildren();
            }
        }
    }

    //Return the triangles with offset
    public int[] GetTrianglesWithOffset(int _triangleOffset)
    {
        //Store a local triangle variable
        int[] triangles = new int[this.triangles.Length];

        //Calculate the triangles with the offset
        for(int  i = 0; i < triangles.Length; i++)
            triangles[i] = this.triangles[i] + _triangleOffset;

        //Return the new triangles
        return triangles;
    }
}