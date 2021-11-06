using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSegment : MonoBehaviour
{
    public LineRenderer segmentRenderer;
    public Vector3[] FEPoint = new Vector3[2];
    public int segmentIndex;
    public int segmentPntCount = 0;
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

    public void Initialize(Vector3 startPnt, Vector3 endPnt, int index, int pntCount)
    {
        FEPoint[0] = startPnt;
        FEPoint[1] = endPnt;
        segmentPntCount = pntCount;
        segmentIndex = index;
        segmentRenderer.SetPositions(FEPoint);
    }

    public void calculateCurve(Vector3 prePntEnd, Vector3 nowPntStart, Vector3 nowPntEnd, Vector3 nextPntStart)
    {
        Vector3 p0 = nowPntStart;
        Vector3 p1 = nowPntEnd;
        Vector3 m0 = (nowPntEnd - prePntEnd) / ((segmentIndex + 1) - (segmentIndex - 1));
        Vector3 m1 = (nextPntStart - nowPntStart) / ((segmentIndex + 1 + 1) - (segmentIndex + 1 - 1));
        FEPoint[0] = nowPntStart;
        FEPoint[1] = nowPntEnd;

        List<Vector3> pnts = new List<Vector3>();
        float omt = 1f;
        for (int i = 50; i >= 0; i--)
        {
            omt = i / 50f;
            Vector3 currentPosition = p0 * Mathf.Pow(omt, 3) + (p0 + m0 / 3) * (3 * omt * omt * (1f-omt)) + (p1 - m1 / 3) * (3 * omt * (1f - omt) * (1f - omt)) + p1 * Mathf.Pow((1f - omt), 3);
            pnts.Add(currentPosition);
        }
        
        Debug.Log("LEN" + pnts.Count.ToString());
        segmentRenderer.positionCount = pnts.Count;
        segmentRenderer.SetPositions(pnts.ToArray());
        Debug.Log("LEN@" + segmentRenderer.positionCount);
    }

    public void calculateBezierCurve(Vector3 startPnt, Vector3 endPnt, Vector3 subPnt1, Vector3 subPnt2)
    {
        Vector3 p0 = startPnt; //頭
        Vector3 p1 = subPnt1; //Control point
        Vector3 p2 = subPnt2; //Control point
        Vector3 p3 = endPnt; //尾
        FEPoint[0] = startPnt;
        FEPoint[1] = endPnt;

        List<Vector3> pnts = new List<Vector3>();
        float omt = 1f;
        for (int i = segmentPntCount; i > 0; i--)
        {
            omt = i / segmentPntCount;
            Vector3 currentPosition = p0 * Mathf.Pow(omt, 3) + p1 * (3 * omt * omt * (1f - omt)) + p2 * (3 * omt * (1f - omt) * (1f - omt)) + p3 * Mathf.Pow((1f - omt), 3);
            pnts.Add(currentPosition);
        }

        segmentRenderer.positionCount = pnts.Count;
        segmentRenderer.SetPositions(pnts.ToArray());

    }

    private void OnDrawGizmos()
    {
        //Vector3[] pnts = new Vector3[51];
        //int a = segmentRenderer.GetPositions(pnts);
        //for (int i = 0; i < a; i++)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawSphere(pnts[i], 0.05f);
        //}
    }
}
