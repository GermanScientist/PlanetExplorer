using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The planet face class creates a single (out of 6) faces for the planet
public class PlanetFace {
    Mesh mesh;
    int resolution;
    Vector3 localUp;

    Vector3 axisOne;
    Vector3 axisTwo;

    //The constructor
    public PlanetFace(Mesh mesh, int resolution, Vector3 localUp)
    {
        //Instantiate the properties
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        //Calculate the other 2 directions the face is facing
        axisOne = new Vector3(localUp.y, localUp.z, localUp.x);
        axisTwo = Vector3.Cross(localUp, axisOne);
    }

    //Create the mesh
    public void CreateMesh()
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

        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                //Calculates the percentage of how close to completage the loop is,
                //which we can use to calculate where the vertex should be
                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                //Use the percentage to figure out where the vertex needs to be on the cube
                Vector3 vertexOnCube = localUp + (percent.x - 0.5f) * 2 * axisOne + (percent.y - 0.5f) * 2 * axisTwo;

                //To turn the cube into a sphere, we want to inflate it by making every vertex the same distance away from the center
                Vector3 vertexOnSphere = vertexOnCube.normalized;

                //Set the current vertex to the newly calculated vertex position
                vertices[index] = vertexOnSphere;

                //Create triangle, aslong as the current vertex isn't along the right or bottom edge
                //in other words, as long as x isn't equal to r-1 and y isn't equal to r-1
                if(x != resolution - 1 && y != resolution - 1)
                {
                    //Update the first triangle
                    triangles[triangleIndex] = index; //The first vertex of the triangle is equal to the current vertex
                    triangles[triangleIndex + 1] = index + resolution + 1; //The second vertex of the triangle is equal to the current vertex + r + 1
                    triangles[triangleIndex + 2] = index + resolution; //The third vertex of the triangle is equal to the current vertex + r

                    //Update the second triangle
                    triangles[triangleIndex + 3] = index; //The first vertex of the triangle is equal to the current vertex
                    triangles[triangleIndex + 4] = index + 1; //The second vertex of the triangle is equal to the current vertex + 1
                    triangles[triangleIndex + 5] = index + resolution + 1; //The third vertex of the triangle is equal to the current vertex + r + 1

                    //Since we added 2 triangles, aka 6 vertices, we increment the triangle index by 6
                    triangleIndex += 6;
                }

                //Increment the index
                index++;

            }
        }

        //Clear the mesh data
        mesh.Clear();

        //Assign the newly calculated vertices to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //Recalculte the normals of the mesh
        mesh.RecalculateNormals();
    }
}
