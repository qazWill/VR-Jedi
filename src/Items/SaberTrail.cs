using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberTrail : MonoBehaviour
{
    public Transform tip;
    public Transform hilt;

    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector3 lastTipPos;
    private Vector3 lastTipPos2;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private Vector3[] normals;
    private int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        vertices = new Vector3[4];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(1, 0, 0);
        vertices[2] = new Vector3(0, 0, 1);
        vertices[3] = new Vector3(1, 0, 1);

        triangles = new int[12];
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 0;
        triangles[4] = 3;
        triangles[5] = 2;
        triangles[6] = 1;
        triangles[7] = 2;
        triangles[8] = 0;
        triangles[9] = 2;
        triangles[10] = 3;
        triangles[11] = 0;

        uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 1);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(1, 0);

        normals = new Vector3[4];
        normals[0] = new Vector3(0, 1, 0);
        normals[1] = new Vector3(0, 1, 0);
        normals[2] = new Vector3(0, 1, 0);
        normals[3] = new Vector3(0, 1, 0);



        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        lastTipPos = tip.transform.position;
        lastTipPos2 = tip.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        vertices[0] = transform.InverseTransformPoint(tip.transform.position);
        vertices[1] = transform.InverseTransformPoint(hilt.transform.position);
        vertices[2] = transform.InverseTransformPoint(hilt.transform.position);
        vertices[3] = transform.InverseTransformPoint(lastTipPos2);

        lastTipPos2 = lastTipPos;
        lastTipPos = tip.transform.position;


        

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }
}
