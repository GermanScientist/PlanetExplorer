using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The planet class creates 6 faces of the planet
public class Planet : MonoBehaviour {
    [SerializeField, HideInInspector] private MeshFilter[] meshFilters;
    private PlanetFace[] planetFaces;

    [Range(2, 256)] public int resolution = 32;

    public float size = 30; //Must be same size as set in inspector

    public Transform player;
    [HideInInspector] public float distanceToPlayer;
    public float cullingMinAngle = 1.45f;

    //Hardcoded detail leves, the first value is the level of detail, the second value is the distance from the player
    public Dictionary<int, float> lodDistances = new Dictionary<int, float>() {
        {0, Mathf.Infinity},
        {1, 60.0f},
        {2, 25.0f},
        {3, 10.0f},
        {4, 4.0f},
        {5, 1.5f},
        {6, 0.7f},
        {7, 0.3f},
        {8, 0.1f}
    };

    //Gets called before start
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Slow, but that doesn't really matter in this case
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    //Gets called at the first frame 
    private void Start()
    {
        InitializePlanet();
        CreateMesh();

        StartCoroutine(PlanetGenerationLoop());
    }

    //Update on validation, so it updates in the editor
    private void OnValidate() {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        InitializePlanet();
        CreateMesh();

        StartCoroutine(PlanetGenerationLoop());
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    //Gets called every frame
    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    //Regenerate the mesh every second
    private IEnumerator PlanetGenerationLoop()
    {
        while(true) {
            yield return new WaitForSeconds(1.0f);
            UpdateMesh();
        }
    }

    //Initialize the planet
    private void InitializePlanet() {
        //Create 6 mesh filters and planet faces, one for each face
        if(meshFilters == null || meshFilters.Length == 0)
            meshFilters = new MeshFilter[6];
        planetFaces = new PlanetFace[6];

        //Create 6 different possible directions the faces could face
        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};

        //Loop through each face
        for (int i = 0; i < 6; i++)
        {
            //If the mesh filter doesn't exist yet
            if(meshFilters[i] == null)
            {
                //Create a game object for each planet face
                GameObject meshObject = new GameObject("mesh");
                
                //Set the parent of each face to the current transform, to keep the hierarchy organized
                meshObject.transform.parent = transform;

                //Add a mesh renderer to the mesh object, and assign a default material
                meshObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                //Set the current meshfilter to the meshfilter of meshObject
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();

                //Assign a mesh to the meshfilter
                meshFilters[i].sharedMesh = new Mesh();
            }

            //Create the face of a planet, passing in the shared mesh of the mesh filter, the resolution and the direction depending on the index
            planetFaces[i] = new PlanetFace(meshFilters[i].sharedMesh, resolution, directions[i], size, this);
        }
    }

    //Create the mesh of the planet
    private void CreateMesh() {
        //Loop through all the planet faces
        foreach(PlanetFace planetFace in planetFaces)
            planetFace.CreateQuadTree();
    }

    //Update the mesh of the planet
    private void UpdateMesh()
    {
        //Loop through all the planet faces
        foreach (PlanetFace planetFace in planetFaces)
            planetFace.UpdateQuadTree();
    }
}
