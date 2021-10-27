using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSegment : MonoBehaviour
{
    public LineRenderer segmentRenderer;
    public Vector3[] FEPoint = new Vector3[2];
    public int segmentIndex;
    private List<Vector3> segmentPnts;
    public GameObject subControlPnt;

    private void Start()
    {
        segmentRenderer = GetComponent<LineRenderer>();
    }

    public void UpdateVert(Vector3 startPnt, Vector3 endPnt)
    {
        // Call when you need to update points.
        FEPoint[0] = startPnt;
        FEPoint[1] = endPnt;
        // Line Renderer
        segmentRenderer.positionCount = 2;
        segmentRenderer.SetPositions(FEPoint);
    }

    public void Initialize(Vector3 startPnt, Vector3 endPnt, int index)
    {
        FEPoint[0] = startPnt;
        FEPoint[1] = endPnt;
        segmentIndex = index;
        segmentRenderer.SetPositions(FEPoint);
    }
}
