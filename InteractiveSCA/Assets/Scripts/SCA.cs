using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCA : MonoBehaviour
{
    // Params.
    public GameObject Particle;
    public GameObject BranchParticle;

    // TODO: enable adding start points
    private Vector3 StartingPoint = new Vector3(0.0f, 0.0f, 0.0f);

    public GameObject PotentialPoint;
    public GameObject PotentialPointActive;
    private List<GameObject> potentialPointsForGrowth = new List<GameObject>();

    float branchParticleStepSize = 0.3f;
    int currBranchID = 0;
    float currThickness = 0.5f;
    float thicknessFalloff = 0.98f;
    int nVertPerEdgeVertex = 4;

    // Variables.
    private List<Vector3> PointVolume = new List<Vector3>();
    private List<Edge> Edges = new List<Edge>();    
    private MeshFilter meshFilter;

    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        int nPoints = 100;
        float range = 25.0f;
        for (int i = 0; i < nPoints; ++i)
        {
            float x = (Random.value - 0.5f) * 2.0f * range;
            float y = (Random.value) * 2.0f * range;
            float z = (Random.value - 0.5f) * 2.0f * range;
            potentialPointsForGrowth.Add(Instantiate(PotentialPoint, new Vector3(x,y,z), Quaternion.identity));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Interactively add volume points for growth.
        InteractiveAddVolumePoints();

        // Perform growth.
        List<Edge> newEdges = BranchGrowth();

        // Draw branches.
        foreach (Edge edge in newEdges)
        {
            //edge.drawEdgeAsParticles(branchParticleStepSize, BranchParticle);
            edge.drawEdgeAsMesh(meshFilter);
            edge.drawEdgeVertices(BranchParticle);
        }
    }

    void InteractiveAddVolumePoints()
    {
        // 3D space is filled with points.
        // User selects the points which are used for growth.

        
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // TODO: add collision layers.
        GameObject activePotentialPoint = null;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            GameObject activePotentialPointDrawable = Instantiate(PotentialPointActive, hit.transform.position, Quaternion.identity);
            
            // Point per point.
            if (Input.GetMouseButtonDown(0))
            {
                PointVolume.Add(hit.point);
            }

            // Groups of points.
            float radius = 10.0f;
            Collider[] hitColliders = Physics.OverlapSphere(hit.point, radius);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                // TODO: remove once not hovering
                Instantiate(PotentialPointActive, hitColliders[i].transform.position, Quaternion.identity);
            }
            if (Input.GetMouseButtonDown(1))
            {
                for (int i = 0; i < hitColliders.Length; i++)
                {
                    PointVolume.Add(hitColliders[i].transform.position);
                }
            }

            // TODO: remove potential point when used.

        }
        
        /*
        if (Input.GetMouseButton(0))
        {
            Debug.Log("The left mouse button is being held down.");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            for (int i = 0; i < 1; i++)
            {
                Vector3 Position = ray.origin + ray.direction * 30.0f;
                Position += Random.insideUnitSphere * 10.0f;
                //Instantiate(Particle, Position, Quaternion.identity);
                PointVolume.Add(Position);
            }
        }
        */
    }

    List<Edge> BranchGrowth()
    {
        List<Edge> newEdges = new List<Edge>();

        if (PointVolume.Count > 0 && Edges.Count == 0)
        {
            // Create first edge by connecting starting position and 
            // closest point from point volume.
            Vector3 closestPoint = FindClosestPointTo(StartingPoint, PointVolume);
            Edge newEdge = new Edge(StartingPoint, closestPoint, currBranchID, nVertPerEdgeVertex, null, currThickness);
            currBranchID += 1;
            Edges.Add(newEdge);
            PointVolume.Remove(closestPoint);
            //newEdge.drawEdgeAsParticles(branchParticleStepSize, BranchParticle);
            newEdge.drawEdgeAsMesh(meshFilter);
            newEdge.drawEdgeVertices(BranchParticle);
        }

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
            Edge NewEdge = new Edge(closestEdge.V2, closestPoint, currBranchID, nVertPerEdgeVertex, null, currThickness); // 2nd point is edge extremity!
            currBranchID += 1;
            currThickness *= thicknessFalloff;
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

    // TODO: separate into logic and rendering classes.
    // Logic class contains only what is needed for SCA algorithm.
    // Rendering class contains methods for drawing.
    class Edge
    {
        // 2nd point is edge extremity!
        public Edge(Vector3 v1, Vector3 v2, int _id, int _nVertPerEdgeVertex, Edge _parent, float _thickness)
        {
            V1 = v1;
            V2 = v2;
            edgeVector = v2 - v1;
            thickness = _thickness;
            id = _id;
            nVertPerEdgeVertex = _nVertPerEdgeVertex;
            parent = _parent;

            nVerts = 2 * nVertPerEdgeVertex;
            nTriangles = 2 * nVertPerEdgeVertex * 3;
            vertices = new Vector3[nVerts];
            triangles = new int[nTriangles];   

            
            Vector3[] vertices1 = new Vector3[nVertPerEdgeVertex];
            if (_parent == null)
            {
                vertices1 = computeVerticesAroundVector(nVertPerEdgeVertex, V1, edgeVector, thickness);
            }
            else
            {
                // TODO: fix problem with vertices and triangle indices!
                for(int i = 0; i < nVertPerEdgeVertex; i++)
                {
                    vertices1[i] = _parent.vertices[i+nVertPerEdgeVertex];
                } 
            }
            Vector3[] vertices2 = computeVerticesAroundVector(nVertPerEdgeVertex, V2, edgeVector, thickness);
            for(int i = 0; i < nVertPerEdgeVertex; i++)
            {
                vertices[i] = vertices1[i];
                vertices[i+nVertPerEdgeVertex] = vertices2[i];
            }
            triangles = computeClockwiseTriangleIndices(nVertPerEdgeVertex);
        }

        public override string ToString() => $"({V1}, {V2})";

        public Vector3[] getPoints()
        {
            Vector3[] points = new Vector3[2];
            points[0] = V1;
            points[1] = V2;
            return points;
        }

        public Vector3[] computeVerticesAroundVector(int nVert, Vector3 origin, Vector3 dir, float distance)
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

        public int[] computeClockwiseTriangleIndices(int nVert)
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

        public void drawEdgeAsParticles(float StepSize, GameObject BranchParticle)
        {
            Vector3 dir = V2 - V1;
            int NSteps = (int)Mathf.Ceil(Vector3.Magnitude(dir) / StepSize);
            dir = Vector3.Normalize(dir);
            for (int i = 0; i < NSteps; ++i)
            {
                Instantiate(BranchParticle, V1 + StepSize * i * dir, Quaternion.identity);
            }
        }

        public void drawEdgeVertices(GameObject BranchParticle)
        {
            float drawableMeshScale = thickness * 4 + 0.1f;
            Vector3 drawableMeshScaleVector = new Vector3(drawableMeshScale, drawableMeshScale, drawableMeshScale);
            GameObject v1Drawable = Instantiate(BranchParticle, V1, Quaternion.identity);
            v1Drawable.transform.localScale = drawableMeshScaleVector;
            GameObject v2Drawable = Instantiate(BranchParticle, V2, Quaternion.identity);
            v2Drawable.transform.localScale = drawableMeshScaleVector;
        }

        public void drawEdgeAsMesh(MeshFilter meshFilter)
        {
            // Update mesh filter of whole SCA.
            Mesh mesh = meshFilter.mesh;
            Vector3[] updatedVertices = new Vector3[mesh.vertices.Length + nVerts];
            int[] updatedTriangles = new int[mesh.triangles.Length + nTriangles];
            Vector3[] existingVertices = mesh.vertices;
            int[] existingTriangles = mesh.triangles;
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
            mesh.Clear();
            mesh.vertices = updatedVertices;
            mesh.triangles = updatedTriangles;
            mesh.RecalculateNormals();
        }

        public Vector3 V1 { get; }
        public Vector3 V2 { get; }
        public float thickness;
        public Vector3 edgeVector;
        public int id;
        public Edge parent;

        public int nVertPerEdgeVertex;
        public int nVerts;
        public int nTriangles;
        public Vector3[] vertices;
        public int[] triangles;
    }

}

