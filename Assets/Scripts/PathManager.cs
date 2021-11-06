using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PathManager : MonoBehaviour
{
    private static object lockObject = new object();
    private static PathManager instance;
    public static PathManager Instance
    {
        get
        {
            lock(lockObject)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PathManager>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<PathManager>();
                        singletonObject.name = "PathManager";
                    }
                }
                return instance;
            }
        }
    }

    public List<LineSegment> segments;
    public List<GameObject> controllPntObs;
    public List<GameObject> subControllPntObs;
    private List<Vector3> controllPnts;
    public GameObject controllPntOb;
    public GameObject subControllPntOb;
    public Button addPntBut;
    public Button minusPntBut;
    public Text pntCountTxt;
    public LineSegment lineSegment;
    public double bezierErrorPar = 10;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 point = Vector3.zero;
        controllPntObs = new List<GameObject>();
        subControllPntObs = new List<GameObject>();
        controllPnts = new List<Vector3>();
        segments = new List<LineSegment>();
        addPntBut.onClick.AddListener(addPointButListener);
        minusPntBut.onClick.AddListener(minusPointButListener);
        // controllPntObs.Add(newControllPntOb(point));
        // subControllPntObs.Add(newSubControllPntOb(point, 3, controllPntObs[controllPntObs.Count - 1].transform));
        // point.z += 10;
        // controllPntObs.Add(newControllPntOb(point));
        // subControllPntObs.Add(newSubControllPntOb(point, -3, controllPntObs[controllPntObs.Count - 1].transform));
        // addSegment();
    }

    // Update is called once per frame
    void Update()
    {
        if (segments.Count > 0)
            segments[0].calculateBezierCurve(controllPntObs[0].transform.position, 
                controllPntObs[1].transform.position, subControllPntObs[0].transform.position, subControllPntObs[1].transform.position);
        for (int i = 1; i < segments.Count; i++)
        {
            Vector3 startVert = controllPntObs[i].transform.position;
            Vector3 endVert = controllPntObs[i + 1].transform.position;
            Vector3 subControllPntOp = startVert - subControllPntObs[i].transform.localPosition;

            //segments[i].UpdateVert(startVert, endVert);
            int index = i - 1;
            if (index < 0)
                index = 0;
            int indexNext = i + 2;
            if (indexNext > segments.Count)
                indexNext = i + 1;
            segments[i].calculateBezierCurve(startVert, endVert, subControllPntOp, subControllPntObs[i + 1].transform.position);
        }
    }

    public void SetBezierFitPath(List<Vector3> pnts)
    {
        // May need to reset these lists
        //controllPntObs = new List<GameObject>();
        //subControllPntObs = new List<GameObject>();
        //controllPnts = new List<Vector3>();
        //segments = new List<LineSegment>();
        Vector3[] result = FitCurves.GetBezierFitCurve(pnts.ToArray(), bezierErrorPar);
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new Vector3(result[i].x, 0, result[i].z);
        }
        controllPntObs.Add(newControllPntOb(result[0]));
        subControllPntObs.Add(newSubControllPntOb(result[1], 0, controllPntObs[controllPntObs.Count - 1].transform));
        controllPntObs.Add(newControllPntOb(result[3]));
        subControllPntObs.Add(newSubControllPntOb(result[2], 0, controllPntObs[controllPntObs.Count - 1].transform));
        addSegment();
    }

    public void updateSegment()
    {

    }

    private GameObject newControllPntOb(Vector3 pos)
    {
        GameObject newPntOb = Instantiate(controllPntOb, transform);
        newPntOb.transform.position = pos;
        controllPnts.Add(pos);
        return newPntOb;
    }
    private GameObject newSubControllPntOb(Vector3 pos, int offset, Transform parent)
    {
        GameObject newPntOb = Instantiate(subControllPntOb, parent);
        Vector3 newPos = pos;
        newPos.z = pos.z + offset;
        newPntOb.transform.position = newPos;
        return newPntOb;
    }

    private void addSegment()
    {
        segments.Add(Instantiate(lineSegment, transform));
        segments[segments.Count - 1].Initialize(controllPntObs[controllPntObs.Count - 2].gameObject.transform.position, 
            controllPntObs[controllPntObs.Count - 1].gameObject.transform.position, segments.Count);
    }

    public void addPointButListener()
    {
        Vector3 newPnt = controllPntObs[controllPntObs.Count - 1].gameObject.transform.position;
        newPnt.z += 10;
        controllPntObs.Add(newControllPntOb(newPnt));
        subControllPntObs.Add(newSubControllPntOb(newPnt, -3, controllPntObs[controllPntObs.Count - 1].transform));
        addSegment();
        pntCountTxt.text = (segments.Count + 1).ToString();
    }
    public void minusPointButListener()
    {
        if (segments.Count > 1)
        {
            Destroy(controllPntObs[controllPntObs.Count - 1].gameObject);
            controllPntObs.RemoveAt(controllPntObs.Count - 1);
            Destroy(subControllPntObs[subControllPntObs.Count - 1].gameObject);
            subControllPntObs.RemoveAt(subControllPntObs.Count - 1);
            controllPnts.RemoveAt(controllPnts.Count - 1);
            Destroy(segments[segments.Count - 1].gameObject);
            segments.RemoveAt(segments.Count - 1);
            pntCountTxt.text = (segments.Count + 1).ToString();

        }
    }
}
