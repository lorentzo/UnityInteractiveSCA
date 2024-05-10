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
    public GameObject BranchPoint;
    private int NParticlesStart = 100;
    private Vector3 VolumeCenter = new Vector3(0.0f,0.0f,0.0f);
    private float VolumeSize = 10.0f;
    private Vector3 StartingPosition = new Vector3(0.0f,-30.0f,0.0f);

    // Variables.
    private Stack<Vector3> NotConnectedPoints = new Stack<Vector3>();
    private List<Vector3> ConnectedPoints = new List<Vector3>();
    private List<Edge> Edges = new List<Edge>();
    

    // Start is called before the first frame update
    void Start()
    {
        // Initialize starting volume - not connected points.
        for (int i = 0; i < NParticlesStart; i++)
        {
            Vector3 Position = Random.insideUnitSphere * VolumeSize + VolumeCenter;
            Position.x = 0.0f;
            Instantiate(Particle, Position, Quaternion.identity);
            NotConnectedPoints.Push(Position);
        }

        // Initialize connected points.
        ConnectedPoints.Add(StartingPosition);

        // Perform connecting.
        while (NotConnectedPoints.Count > 0)
        {
            Vector3 NotConnectedPoint = NotConnectedPoints.Pop();
            float CurrDist = VolumeSize * 1000.0f; // sth large
            Vector3 ClosestPoint = ConnectedPoints[0]; // random
            foreach (Vector3 ConnectedPoint in ConnectedPoints)
            {
                float Dist = Vector3.Distance(NotConnectedPoint, ConnectedPoint);
                if (Dist < CurrDist)
                {
                    CurrDist = Dist;
                    ClosestPoint = ConnectedPoint;
                }
            }
            Edge NewEdge = new Edge(NotConnectedPoint, ClosestPoint);
            Edges.Add(NewEdge);
            ConnectedPoints.Add(NotConnectedPoint);
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
                Instantiate(BranchPoint, edge.V1 + StepSize * i * dir, Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
