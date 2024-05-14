using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Edge
{
    // 2nd point is edge extremity!
    public Edge(Vector3 v1, Vector3 v2, int _id)
    {
        V1 = v1;
        V2 = v2;
        edgeVector = V2 - v1;
        thickness = 0.5f;
        id = _id;
    }

    public override string ToString() => $"({V1}, {V2})";

    public Vector3[] getPoints()
    {
        Vector3[] points = new Vector3[2];
        points[0] = V1;
        points[1] = V2;
        return points;
    }

    public Vector3 V1 { get; }
    public Vector3 V2 { get; }
    public float thickness;
    public Vector3 edgeVector;
    public int id;
}

public class SCA : MonoBehaviour
{
    // Params.
    public GameObject Particle;
    public GameObject BranchParticle;

    private int NPointVolumeStart = 10;
    private Vector3 startPointVolumeCenter = new Vector3(0.0f,0.0f,0.0f);
    private float startPointVolumeSize = 10.0f;
    private Vector3 StartingPoint = new Vector3(0.0f,-30.0f,0.0f);

    float branchParticleStepSize = 0.3f;
    int currBranchID = 0;

    // Variables.
    private List<Vector3> PointVolume = new List<Vector3>();
    private List<Edge> Edges = new List<Edge>();    
    private MeshFilter meshFilter;
    //private MeshRenderer meshRenderer;

    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        //meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize starting point volume.
        for (int i = 0; i < NPointVolumeStart; i++)
        {
            Vector3 Position = Random.insideUnitSphere * startPointVolumeSize + startPointVolumeCenter;
            Instantiate(Particle, Position, Quaternion.identity);
            PointVolume.Add(Position);
        }

        // Create first edge by connecting starting position and 
        // closest point from point volume.
        Vector3 closestPoint = FindClosestPointTo(StartingPoint, PointVolume);
        Edge newEdge = new Edge(StartingPoint, closestPoint, currBranchID);
        currBranchID += 1;
        Edges.Add(newEdge);
        PointVolume.Remove(closestPoint);
        //drawEdge(newEdge, branchParticleStepSize, BranchParticle);
        drawEdge3(newEdge);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            Debug.Log("The left mouse button is being held down.");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            for (int i = 0; i < 1; i++)
            {
                Vector3 Position = ray.origin + ray.direction * 30.0f;
                Position += Random.insideUnitSphere * 10.0f;
                Instantiate(Particle, Position, Quaternion.identity);
                PointVolume.Add(Position);
            }
        }

        // Perform growth.
        List<Edge> newEdges = BranchGrowth();

        // Draw branches.
        foreach (Edge edge in newEdges)
        {
            //drawEdge(edge, branchParticleStepSize, BranchParticle);
            drawEdge3(edge);
        }
        
    }

    List<Edge> BranchGrowth()
    {
        List<Edge> newEdges = new List<Edge>();
        while (PointVolume.Count > 0)
        {
            // Find overall closest point.
            float minDist = Mathf.Infinity; // sth large
            Vector3 closestPoint = PointVolume[0]; // random
            Edge closestEdge = Edges[0]; // random
            foreach (Edge edge in Edges)
            {
               Vector3 currClosestPoint = FindClosestPointTo(edge.V2, PointVolume);
               float currDist = Vector3.Distance(closestPoint, edge.V2);
               if (currDist < minDist)
               {
                    minDist = currDist;
                    closestPoint = currClosestPoint;
                    closestEdge = edge;
               }
            }
            Edge NewEdge = new Edge(closestEdge.V2, closestPoint, currBranchID); // 2nd point is edge extremity!
            currBranchID += 1;
            Edges.Add(NewEdge);
            newEdges.Add(NewEdge);
            PointVolume.Remove(closestPoint);
        }
        return newEdges;
    }

    Vector3 FindClosestPointTo(Vector3 targetPoint, List<Vector3> allPoints)
    {
        // TODO: Space query acceleration structure!
        float minDist = Mathf.Infinity;
        Vector3 ClosestPoint = allPoints[0]; // random
        foreach (Vector3 point in allPoints)
        {
            float currDist = Vector3.Distance(targetPoint, point);
            if (currDist < minDist)
            {
                minDist = currDist;
                ClosestPoint = point;
            }
        }
        return ClosestPoint;
    } 

    void drawEdge(Edge edge, float StepSize, GameObject BrancParticle)
    {
        Vector3 dir = edge.V2 - edge.V1;
        int NSteps = (int)Mathf.Ceil(Vector3.Magnitude(dir) / StepSize);
        dir = Vector3.Normalize(dir);
        for (int i = 0; i < NSteps; ++i)
        {
            Instantiate(BranchParticle, edge.V1 + StepSize * i * dir, Quaternion.identity);
        }
    }

    void drawEdge3(Edge edge)
    {
        int nVertPerEdgeVertex = 4;
        int nVerts = 2 * nVertPerEdgeVertex;
        int nTriangles = 2 * nVertPerEdgeVertex * 3;
		Vector3[] vertices = new Vector3[nVerts];
		int[] triangles = new int[nTriangles];   

        Mesh mesh = meshFilter.mesh;

        Vector3[] updatedVertices = new Vector3[mesh.vertices.Length + nVerts];
        int[] updatedTriangles = new int[mesh.triangles.Length + nTriangles];
        Vector3[] existingVertices = mesh.vertices;
        int[] existingTriangles = mesh.triangles;

        mesh.Clear();

        for (int s = 0; s < nVertPerEdgeVertex; s++) 
        {
            // quaternion to rotate the vertices along the branch direction
		    Quaternion quat = Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(edge.edgeVector));
            // radial angle of the vertex
            float alpha = ((float)s/nVertPerEdgeVertex) * Mathf.PI * 2f;
            // radius is hard-coded to 0.1f for now
            Vector3 pos = new Vector3(Mathf.Cos(alpha)* edge.thickness, 0, Mathf.Sin(alpha) * edge.thickness);
            pos = quat * pos; // rotation
            vertices[s] = pos + edge.V1;
            vertices[s+nVertPerEdgeVertex] = pos + edge.V2;
        }

        for (int i = 0; i < vertices.Length; ++i)
        {
            Debug.Log(i + " " + vertices[i]);
        }

        int tid = 0;
        for (int s = 0; s < nVertPerEdgeVertex; s++) 
        {
            // NOTE: clockwise!
            if (s < nVertPerEdgeVertex-1)
            {
                triangles[tid] = nVertPerEdgeVertex+s;
                tid++;
                triangles[tid] = s+1;
                tid++;
                triangles[tid] = s;
                tid++;
                
                
                triangles[tid] = nVertPerEdgeVertex+s;
                tid++;triangles[tid] = nVertPerEdgeVertex+s+1;
                tid++;
                triangles[tid] = s+1;
                tid++;
            }
            else
            {
                triangles[tid] = nVertPerEdgeVertex*2-1;
                tid++;
                triangles[tid] = 0;
                tid++;
                triangles[tid] = nVertPerEdgeVertex-1;
                tid++;
                
                
                triangles[tid] = nVertPerEdgeVertex*2-1;
                tid++;
                triangles[tid] = nVertPerEdgeVertex;
                tid++;
                triangles[tid] = 0;
                tid++;
            }
        }

        Debug.Log("N triangle indices:" + triangles.Length);
        for (int i = 0; i < triangles.Length; ++i)
        {
            Debug.Log(i + " " + triangles[i]);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Instantiate(BranchParticle, vertices[i], Quaternion.identity);
        }

        for (int i = 0; i < existingVertices.Length; i++)
        {
            updatedVertices[i] = existingVertices[i];
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            updatedVertices[existingVertices.Length+i] = vertices[i];
        }
        for (int i = 0; i < existingTriangles.Length; i++)
        {
            updatedTriangles[i] = existingTriangles[i];
        }
        for (int i = 0; i < triangles.Length; i++)
        {
            updatedTriangles[existingTriangles.Length+i] = triangles[i]+existingVertices.Length;
        }
        mesh.vertices = updatedVertices;
		mesh.triangles = updatedTriangles;
		mesh.RecalculateNormals();
    }

}
