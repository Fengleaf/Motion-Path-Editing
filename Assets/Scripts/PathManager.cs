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

    public List<LineSegment> segments;
    public List<GameObject> controllPntObs;
    public GameObject controllPntOb;
    public Button addPntBut;
    public Button minusPntBut;
    public Text pntCountTxt;
    public LineSegment lineSegment;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 point = Vector3.zero;
        controllPntObs = new List<GameObject>();
        segments = new List<LineSegment>();
        addPntBut.onClick.AddListener(addPointButListener);
        minusPntBut.onClick.AddListener(minusPointButListener);
        controllPntObs.Add(newControllPntOb(point));
        point.z += 10;
        controllPntObs.Add(newControllPntOb(point));
        addSegment();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            Vector3 startVert = controllPntObs[i].transform.position;
            Vector3 endVert = controllPntObs[i + 1].transform.position;
            segments[i].UpdateVert(startVert, endVert);
        }
    }

    public void updateSegment()
    {

    }

    private GameObject newControllPntOb(Vector3 pos)
    {
        GameObject newPntOb = Instantiate(controllPntOb, transform);
        newPntOb.transform.position = pos;
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
        addSegment();
        pntCountTxt.text = (segments.Count + 1).ToString();
    }
    public void minusPointButListener()
    {
        if (segments.Count > 1)
        {
            Destroy(controllPntObs[controllPntObs.Count - 1].gameObject);
            controllPntObs.RemoveAt(controllPntObs.Count - 1);
            Destroy(segments[segments.Count - 1].gameObject);
            segments.RemoveAt(segments.Count - 1);
            pntCountTxt.text = (segments.Count + 1).ToString();
        }
    }
}
