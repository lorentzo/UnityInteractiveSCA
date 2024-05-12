using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Edge
{
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

    private int NPointVolumeStart = 100;
    private Vector3 PointVolumeCenter = new Vector3(0.0f,0.0f,0.0f);
    private float PointVolumeSize = 10.0f;
    private Vector3 StartingPoint = new Vector3(0.0f,-30.0f,0.0f);

    // Variables.
    private List<Vector3> PointVolume = new List<Vector3>();
    private List<Edge> Edges = new List<Edge>();
    

    // Start is called before the first frame update
    void Start()
    {
        // Initialize starting volume - not connected points.
        for (int i = 0; i < NPointVolumeStart; i++)
        {
            Vector3 Position = Random.insideUnitSphere * PointVolumeSize + PointVolumeCenter;
            Instantiate(Particle, Position, Quaternion.identity);
            PointVolume.Add(Position);
        }

        // Create first edge by connecting starting position and 
        // closest point from point volume.
        Vector3 closestPoint = FindClosestPointTo(StartingPoint, PointVolume);
        Edge newEdge = new Edge(StartingPoint, closestPoint); // 2nd point is edge extremity!
        Edges.Add(newEdge);
        PointVolume.Remove(closestPoint);

        // Perform growth.
        while (PointVolume.Count > 0)
        {
            // Find overall closest point.
            float minDist = PointVolumeSize * 1000.0f; // sth large
            closestPoint = PointVolume[0]; // random
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
            PointVolume.Remove(closestPoint);
        }

        // Draw.
        float StepSize = 0.3f;
        foreach (Edge edge in Edges)
        {
            Vector3 dir = edge.V2 - edge.V1;
            int NSteps = (int)Mathf.Ceil(Vector3.Magnitude(dir) / StepSize);
            dir = Vector3.Normalize(dir);
            for (int i = 0; i < NSteps; ++i)
            {
                Instantiate(BranchParticle, edge.V1 + StepSize * i * dir, Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3 FindClosestPointTo(Vector3 targetPoint, List<Vector3> allPoints)
    {
        // TODO: Space query acceleration structure!
        float minDist = PointVolumeSize * 10000.0f;
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
