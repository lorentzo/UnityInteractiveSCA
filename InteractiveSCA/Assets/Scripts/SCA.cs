using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Edge
{
    // 2nd point is edge extremity!
    public Edge(Vector3 v1, Vector3 v2, int _id, int _nVertPerEdgeVertex, Vector3[] parentVertices)
    {
        V1 = v1;
        V2 = v2;
        edgeVector = V2 - v1;
        thickness = 0.5f;
        id = _id;
        nVertPerEdgeVertex = _nVertPerEdgeVertex;

        nVerts = 2 * nVertPerEdgeVertex;
        nTriangles = 2 * nVertPerEdgeVertex * 3;
        vertices = new Vector3[nVerts];
        triangles = new int[nTriangles];   

        Vector3[] vertices1 = SCA.computeVerticesAroundVector(nVertPerEdgeVertex, V2, edgeVector, thickness);
        for(int i = 0; i < nVertPerEdgeVertex; i++)
        {
            vertices[i] = vertices1[i];
            vertices[i+nVertPerEdgeVertex] = parentVertices[i];
            
        }

        triangles = SCA.computeClockwiseTriangleIndices(nVertPerEdgeVertex);
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

    public int nVertPerEdgeVertex;
    public int nVerts;
    public int nTriangles;
    public Vector3[] vertices;
    public int[] triangles;
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
    float thickness = 0.5f;

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
        int nVertPerEdgeVertex = 4;
        int nVerts = 2 * nVertPerEdgeVertex;
		Vector3[] vertices = new Vector3[nVerts];
        for (int s = 0; s < nVertPerEdgeVertex; s++) 
        {
            // quaternion to rotate the vertices along the branch direction
		    Quaternion quat = Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(closestPoint-StartingPoint));
            // radial angle of the vertex
            float alpha = ((float)s/nVertPerEdgeVertex) * Mathf.PI * 2f;
            // radius is hard-coded to 0.1f for now
            Vector3 pos = new Vector3(Mathf.Cos(alpha)* thickness, 0, Mathf.Sin(alpha) * thickness);
            pos = quat * pos; // rotation
            vertices[s] = pos + StartingPoint;
        }
        Edge newEdge = new Edge(StartingPoint, closestPoint, currBranchID, 4, vertices);
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
            Edge NewEdge = new Edge(closestEdge.V2, closestPoint, currBranchID, 4, closestEdge.vertices); // 2nd point is edge extremity!
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

    public static Vector3[] computeVerticesAroundVector(int nVert, Vector3 origin, Vector3 dir, float distance)
    {
        Vector3[] vertices = new Vector3[nVert];
        for (int i = 0; i < nVert; i++) 
        {
            // quaternion to rotate the vertices along the branch direction
		    Quaternion quat = Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(dir));
            // radial angle of the vertex
            float alpha = ((float)i/nVert) * Mathf.PI * 2f;
            // radius is hard-coded to 0.1f for now
            Vector3 pos = new Vector3(Mathf.Cos(alpha) * distance, 0, Mathf.Sin(alpha) * distance);
            pos = quat * pos; // rotation
            vertices[i] = origin + pos;
        }
        return vertices;
    }

    public static int[] computeClockwiseTriangleIndices(int nVert)
    {
        List<int> triangleIndices = new List<int>();
        for (int s = 0; s < nVert; s++) 
        {
            // NOTE: clockwise!
            if (s < nVert-1)
            {
                triangleIndices.Add(nVert+s);
                triangleIndices.Add(s+1);
                triangleIndices.Add(s);
                
                triangleIndices.Add(nVert+s);
                triangleIndices.Add(nVert+s+1);
                triangleIndices.Add(s+1);
            }
            else
            {
                triangleIndices.Add(nVert*2-1);
                triangleIndices.Add(0);
                triangleIndices.Add(nVert-1);

                triangleIndices.Add(nVert*2-1);
                triangleIndices.Add(nVert);
                triangleIndices.Add(0);
            }
        }
        return triangleIndices.ToArray();
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
        //Vector3[] vertices = edge.vertices;
		//int[] triangles = edge.triangles;  

        Mesh mesh = meshFilter.mesh;

        Vector3[] updatedVertices = new Vector3[mesh.vertices.Length + nVerts];
        int[] updatedTriangles = new int[mesh.triangles.Length + nTriangles];
        Vector3[] existingVertices = mesh.vertices;
        int[] existingTriangles = mesh.triangles;

        mesh.Clear();

        Vector3[] vertices1 = computeVerticesAroundVector(nVertPerEdgeVertex, edge.V1, edge.edgeVector, edge.thickness);
        Vector3[] vertices2 = computeVerticesAroundVector(nVertPerEdgeVertex, edge.V2, edge.edgeVector, edge.thickness);
        for (int i = 0; i < nVertPerEdgeVertex; ++i)
        {
            vertices[i] = vertices1[i];
            vertices[i+nVertPerEdgeVertex] = vertices2[i];
        }
        triangles = computeClockwiseTriangleIndices(nVertPerEdgeVertex);

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
