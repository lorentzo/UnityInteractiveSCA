using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Edge
{
    // 2nd point is edge extremity!
    public Edge(Vector3 v1, Vector3 v2)
    {
        V1 = v1;
        V2 = v2;
    }

    public Vector3 V1 { get; }
    public Vector3 V2 { get; }

    public override string ToString() => $"({V1}, {V2})";
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

    // Variables.
    private List<Vector3> PointVolume = new List<Vector3>();
    private List<Edge> Edges = new List<Edge>();    

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
        Edge newEdge = new Edge(StartingPoint, closestPoint);
        Edges.Add(newEdge);
        PointVolume.Remove(closestPoint);
        drawEdge(newEdge, branchParticleStepSize, BranchParticle);
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
            drawEdge(edge, branchParticleStepSize, BranchParticle);
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
            Edge NewEdge = new Edge(closestEdge.V2, closestPoint); // 2nd point is edge extremity!
            Edges.Add(NewEdge);
            newEdges.Add(NewEdge);
            PointVolume.Remove(closestPoint);
        }
        return newEdges;
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
}
