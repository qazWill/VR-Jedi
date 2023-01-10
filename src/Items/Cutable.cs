using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutable : MonoBehaviour
{

    private Vector3 firstContact;
    public int recursiveCutLimit = 10;
    private Rigidbody rb;
    private ForceInteractable fi;

    private void Awake()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Scale(transform.localScale);
        }
        mesh.vertices = vertices;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        transform.localScale = new Vector3(1, 1, 1);

        rb = GetComponent<Rigidbody>();
        fi = GetComponent<ForceInteractable>();

    }

    void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.tag.Equals("Blade"))
        {
            firstContact = c.gameObject.GetComponent<Deflection>().hilt.position;
        }
    }

    void OnTriggerExit(Collider c)
    {

        if (c.gameObject.tag.Equals("Blade"))
        {
            Transform hiltTrans = c.gameObject.GetComponent<Deflection>().hilt;
            Transform bladeTrans = c.gameObject.GetComponent<Deflection>().tip;
            cut(firstContact, hiltTrans.position, bladeTrans.position);

        }
    }

    private void TestCut()
    {
        GameObject userPlane = GameObject.Find("UserPlane");
        Transform trans = userPlane.transform;
        //Plane plane = new Plane(userPlane.transform.up, userPlane.transform.position);
        //Color color = Color.red;
        //Debug.DrawLine(Vector3.zero, new Vector3(0, 5, 0), color);
        cut(trans.position + trans.up, trans.position + trans.right, trans.position);
        Debug.Log("test cut");
    }

    // cuts the mesh in two based on given points of a plane in world space
    public GameObject cut(Vector3 planePoint1, Vector3 planePoint2, Vector3 planePoint3)
    {
        if (fi)
        {
            if (fi.beingForceHeld)
            {
                return null;
            }
        }
        if (recursiveCutLimit == 0 || rb.isKinematic)
        {
            return null;
        }
        planePoint1 -= transform.position;
        planePoint2 -= transform.position;
        planePoint3 -= transform.position;
        planePoint1.Scale(transform.localScale);
        planePoint2.Scale(transform.localScale);
        planePoint3.Scale(transform.localScale);
        planePoint1 = Quaternion.Inverse(transform.rotation) * planePoint1;
        planePoint2 = Quaternion.Inverse(transform.rotation) * planePoint2;
        planePoint3 = Quaternion.Inverse(transform.rotation) * planePoint3;
        
        Plane plane = new Plane(planePoint1, planePoint2, planePoint3);
        //Debug.Log(trans.forward);
        //Debug.Log(trans.position);
        //plane.Translate(transform.position);
        

        // this object
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshCollider collider = GetComponent<MeshCollider>();


        // the other object
        //GameObject other = (GameObject)Instantiate(Resources.Load("CutableObject"));
        GameObject other = (GameObject)Instantiate(gameObject);
        //other.transform.Translate(transform.position);
        //other.transform.rotation = transform.rotation;
        Mesh mesh2 = other.GetComponent<MeshFilter>().mesh;
        MeshCollider collider2 = other.GetComponent<MeshCollider>();


        // the new exposed face
        Vector3[] cutVertices = new Vector3[0];
        int cutSize = 0;
        List<Vector3> allCutVertices = new List<Vector3>();
        List<List<int>> cutTris = new List<List<int>>(); // all tris associated with each cutVertex


        // information for the two new meshes
        int[] newTriangles1 = new int[0];
        int size1 = 0;
        int[] newTriangles2 = new int[0];
        int size2 = 0;
        Vector3[] newVertices1 = new Vector3[0];
        Vector2[] newUvs1 = new Vector2[0];
        Vector3[] newVertices2 = new Vector3[0];
        Vector2[] newUvs2 = new Vector2[0];

        // loop through faces
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {

            // get points on the face
            Vector3 a = mesh.vertices[mesh.triangles[i]];
            Vector3 b = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 c = mesh.vertices[mesh.triangles[i + 2]];

            // get normal of the face
            Vector3 faceNormal = Vector3.Cross(b - a, c - a).normalized;

            // finds intersection points with plane
            bool intersectAB, intersectBC, intersectCA;
            Vector3 splitAB = intersectLinePlane(a, b, plane, out intersectAB);
            Vector3 splitBC = intersectLinePlane(b, c, plane, out intersectBC);
            Vector3 splitCA = intersectLinePlane(c, a, plane, out intersectCA);
            Vector3 split1 = new Vector3(0, 0, 0);
            Vector3 split2 = new Vector3(0, 0, 0);
            Color color = Color.red;
            int count = 0;
            if (intersectAB)
            {

                split1 = splitAB;
                trackTriNeighbors(splitAB, ref allCutVertices, ref cutTris, i);
                addUniqueVertex(splitAB, ref cutVertices, ref cutSize);
                count++;
            }
            if (intersectBC)
            {

                if (count == 0)
                {
                    split1 = splitBC;
                }
                else
                {
                    split2 = splitBC;
                }
                trackTriNeighbors(splitBC, ref allCutVertices, ref cutTris, i);
                addUniqueVertex(splitBC, ref cutVertices, ref cutSize);
                count++;
            }
            if (intersectCA)
            {

                if (count == 0)
                {
                    split1 = splitCA;
                }
                else
                {
                    split2 = splitCA;
                }
                trackTriNeighbors(splitCA, ref allCutVertices, ref cutTris, i);
                addUniqueVertex(splitCA, ref cutVertices, ref cutSize);
                count++;
            }

            // add triangle to newTriangles
            if (count == 0)
            {
                if (plane.GetSide(a))
                {
                    addTriangle(a, b, c, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices1, ref newTriangles1, ref newUvs1, ref size1, faceNormal);
                }
                else
                {
                    addTriangle(a, b, c, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices2, ref newTriangles2, ref newUvs2, ref size2, faceNormal);
                }
                
            }

            // split this triangle into just two triangles
            else if (count == 1)
            {

            }

            // split this triangle into 3 new triangles
            else
            {

                // find the lone point
                Vector3 lonePoint = a;
                Vector3 doublePoint = b;
                Vector3 singlePoint = c;
                if (plane.GetSide(b) != plane.GetSide(a) && plane.GetSide(b) != plane.GetSide(c))
                {
                    lonePoint = b;
                    doublePoint = a;
                    singlePoint = c;
                }
                else if(plane.GetSide(c) != plane.GetSide(a) && plane.GetSide(c) != plane.GetSide(b))
                {
                    lonePoint = c;
                    doublePoint = a;
                    singlePoint = b;
                }

                // lone point triangle
                if (plane.GetSide(lonePoint))
                {
                    addTriangle(lonePoint, split1, split2, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices1, ref newTriangles1, ref newUvs1, ref size1, faceNormal);
                }
                else
                {
                    addTriangle(lonePoint, split1, split2, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices2, ref newTriangles2, ref newUvs2, ref size2, faceNormal);
                }

                // double point triangle
                if (plane.GetSide(doublePoint))
                {
                    addTriangle(doublePoint, split1, split2, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices1, ref newTriangles1, ref newUvs1, ref size1, faceNormal);
                }
                else
                {
                    addTriangle(doublePoint, split1, split2, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices2, ref newTriangles2, ref newUvs2, ref size2, faceNormal);
                }

                // single point triangle
                Vector3 usefulSplit = split1;
                if (isPointOnRay(split2, singlePoint, lonePoint))
                {
                    usefulSplit = split2;
                }
                if (plane.GetSide(singlePoint))
                {
                    addTriangle(singlePoint, doublePoint, usefulSplit, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices1, ref newTriangles1, ref newUvs1, ref size1, faceNormal);
                    /*Debug.DrawLine(usefulSplit, usefulSplit + (faceNormal / 3.0f), Color.red, 1000, false);
                    Debug.DrawLine(singlePoint, singlePoint + (faceNormal / 3.0f), Color.blue, 1000, false);
                    Debug.DrawLine(lonePoint, lonePoint + (faceNormal / 3.0f), Color.yellow, 1000, false);
                    Debug.DrawLine(doublePoint, doublePoint + (faceNormal / 3.0f), Color.green, 1000, false);*/
                }
                else
                {
                    addTriangle(singlePoint, doublePoint, usefulSplit, Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices2, ref newTriangles2, ref newUvs1, ref size2, faceNormal);
                }
            }

        }



        // for concave shapes determine if two or more cuts were made
       bool multiFace = AreMultipleFaces(allCutVertices, cutTris);
        //Debug.Log("unvisitedVerts: " + unvisitedCount);
        /*if (true)
        {
            for (int i = 0; i < allCutVertices.Count; i++)
            {
                Debug.Log("  Vertex: " + allCutVertices[i]);
                foreach (var tri in cutTris[i])
                {
                    Debug.Log("    Tri: " + tri);
                }
            }
        }*/
        if (multiFace) // dont cut!
        {
            Destroy(other);
            return null;
        }
        





        // add new cut tris (this needs fixed, order isnt right for every cut)
        sortClockwise(cutVertices, -plane.normal);
        for (int k = 1; k < cutVertices.Length - 1; k++)
        {
            
            addTriangle(cutVertices[0], cutVertices[k], cutVertices[k + 1], Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices1, ref newTriangles1, ref newUvs1, ref size1, -plane.normal);
            addTriangle(cutVertices[0], cutVertices[k], cutVertices[k + 1], Vector2.zero, Vector2.zero, Vector2.zero, ref newVertices2, ref newTriangles2, ref newUvs2, ref size2, plane.normal);
        }



        // update this object
        Vector3 center = getCenterOf(newVertices1);
        vectorShift(newVertices1, -center);
        transform.Translate(center);
        mesh.Clear();
        mesh.vertices = newVertices1;
        mesh.triangles = newTriangles1;
        //mesh.uv = newUvs1;
        mesh.RecalculateNormals();
        collider.sharedMesh = mesh;

        // spawn new one from split
        Vector3 center2 = getCenterOf(newVertices2);
        vectorShift(newVertices2, -center2);
        other.transform.Translate(center2);
        mesh2.Clear();
        mesh2.vertices = newVertices2;
        mesh2.triangles = newTriangles2;
        //mesh.uv = newUvs2;
        mesh2.RecalculateNormals();
        collider2.sharedMesh = mesh2;

        recursiveCutLimit -= 1;
        other.GetComponent<Cutable>().recursiveCutLimit -= 1;
        return other;
    }

    // used to determine how to group cut faces
    bool AreMultipleFaces(List<Vector3> cutVertices, List<List<int>> cutTris)
    {
        bool[] visited = new bool[cutVertices.Count];
        for (int i = 0; i < visited.Length; i++)
        {
            visited[i] = false;
        }
        int unVisitedCount = cutVertices.Count;
        int currentIndex = 0;
        bool found = true;
        while (unVisitedCount > 0 && found)
        {
            found = false;
            for (int i = 0; i < cutVertices.Count; i++)
            {
                if (i == currentIndex || visited[i])
                {
                    continue;
                }

                // find vertex on same tri (the neighbor)
                foreach (int triA in cutTris[i])
                {
                    foreach (int triB in cutTris[currentIndex])
                    {
                        if (triA == triB)
                        {
                            found = true;
                            currentIndex = i;
                            visited[i] = true;
                            unVisitedCount--;
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (found) {
                    break;
                }
            }
        }

        int tripleCount = 0;
        for (int i = 0; i < cutVertices.Count; i++)
        {
            if (cutTris[0].Count > 2)
            {
                tripleCount++;
            }
        }

        //Debug.Log("  unvisited: " + unVisitedCount);
        //Debug.Log("  triple+: " + tripleCount);
        //Debug.Log(" ======================== ");
        return unVisitedCount > 0 || tripleCount > 0 ;
    }

    void trackTriNeighbors(Vector3 vertex, ref List<Vector3> allCutVerts, ref List<List<int>> cutTris, int tri)
    {
        bool unique = true;
        for (int i = 0; i < allCutVerts.Count; i++)
        {
            if (allCutVerts[i] == vertex)
            {
                unique = false;
                if (!cutTris[i].Contains(tri))
                {
                    cutTris[i].Add(tri);
                }
            }
        }
        if (unique)
        {
            allCutVerts.Add(vertex);
            cutTris.Add(new List<int>());
            cutTris[allCutVerts.Count - 1].Add(tri);
        }
    }

    // add vertex if not in list ( also dont add if already have bigger line
    void addUniqueVertex(Vector3 vertex, ref Vector3[] verts, ref int size)
    {

        bool unique = true;  
        for (int i = 0; i < size; i++)
        {
            if (verts[i] == vertex)
            {
                unique = false;
            }
        }
        if (unique)
        {
            bool addVertex = true;
            for (int i = 0; i < verts.Length; i++)
            {
                for (int j = i + 1; j < verts.Length; j++)
                {
                    if (isPointOnRay(vertex, verts[i], verts[j]))
                    {
                        addVertex = false;
                        float oldLength = (verts[i] - verts[j]).magnitude;
                        float newLength1 = (verts[i] - vertex).magnitude;
                        float newLength2 = (verts[j] - vertex).magnitude;
                        if (newLength1 > oldLength && newLength1 > newLength2)
                        {
                            verts[j] = vertex; // replacement
                            
                            
                        }
                        else if (newLength2 > oldLength && newLength2 > newLength1)
                        {
                             verts[i] = vertex;
                             
                        }
                    }
                }
            }
            
            if (addVertex)
            {
                size++;
                Array.Resize<Vector3>(ref verts, size);
                verts[size - 1] = vertex;
            }
        }
    }

    void vectorShift(Vector3[] verts, Vector3 shift)
    {
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += shift;
        }
    }

    Vector3 getCenterOf(Vector3[] verts)
    {
        Vector3 sum = new Vector3(0, 0, 0);
        foreach (Vector3 vert in verts)
        {
            sum += vert;

        }

        return sum / (float)verts.Length;

    }


    // arranges points on a plane clockwise about a normal
    void sortClockwise(Vector3[] verts, Vector3 cutNormal)
    {
        for (int i = 0; i < verts.Length - 1; i++)
        {
            for (int j = i + 1; j < verts.Length; j++)
            {
                Plane plane = new Plane(verts[i], verts[j] + cutNormal, verts[j]);

                // check to see if any points lie behind plane
                bool behind = false;
                for (int k = 0; k < verts.Length; k++)
                {
                    if (k == j || k == i)
                    {
                        continue;
                    }
                    if (!plane.GetSide(verts[k]))
                    {
                        behind = true;
                    }
                }

                // swap points if found right one
                if (!behind)
                {
                    Vector3 temp = verts[i + 1];
                    verts[i + 1] = verts[j];
                    verts[j] = temp;
                }
            }
        }
    }

    // add new triangle to vertices and triangle array
    void addTriangle(Vector3 a, Vector3 b, Vector3 c, Vector2 uvA, Vector2 uvB, Vector2 uvC, ref Vector3[] verts, ref int[] tris, ref Vector2[] uvs, ref int size, Vector3 otherNormal)
    {
        Vector3 faceNormal = Vector3.Cross(b - a, c - a).normalized;
        if (faceNormal != otherNormal)
        {
            Vector3 temp = c;
            c = b;
            b = temp;
        }
        size += 3;
        Array.Resize<Vector3>(ref verts, size);
        Array.Resize<int>(ref tris, size);
        Array.Resize<Vector2>(ref uvs, size);
        verts[size - 3] = a;
        verts[size - 2] = b;
        verts[size - 1] = c;
        tris[size - 3] = size - 3;
        tris[size - 2] = size - 2;
        tris[size - 1] = size - 1;
        uvs[size - 3] = uvA;
        uvs[size - 2] = uvB;
        uvs[size - 1] = uvC;

    }

    bool isPointOnRay(Vector3 point, Vector3 a, Vector3 b)
    {
        return Vector3.Cross(a - point, b - point) == Vector3.zero;
    }


    // returns point of intersection between line and plane if there is none intersecting will be false
    Vector3 intersectLinePlane(Vector3 a, Vector3 b, Plane plane, out bool intersecting)
    {
        
        // finds point of intersection with ray
        Ray ray = new Ray(a, b - a);
        //Color color = Color.red;
        //Debug.DrawRay((a + b) / 2.0f, b - a, color);
        float offset = 0.0f;
        plane.Raycast(ray, out offset);
        Vector3 point = ray.GetPoint(offset);

        // check that point of intersection is on the line
        float max_dist = (b - a).magnitude;
        intersecting = false;
        if (plane.Raycast(ray, out offset)) // if intersects with ray
        {
            if ((point - a).magnitude < max_dist && (point - b).magnitude < max_dist && point != a && point != b) // if within line segment
            {
                intersecting = true;
            }
        }

        return point;
    }
}
