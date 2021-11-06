using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Text frameText;

    private void Awake()
    {
        Instance = this;
    }

    public void RunBVH()
    {
        List<BVH> bvhs = new List<BVH>(FindObjectsOfType<BVH>());
        foreach (BVH bvh in bvhs)
            bvh.Run();
    }
    public void Test(int index)
    {
        BVH bvh = new List<BVH>(FindObjectsOfType<BVH>())[index];
        List<Vector3> path = bvh.GetAllPath();
        for (int i = 0; i < path.Count; i++)
        {
            GameObject point = new GameObject(i.ToString());
            point.transform.position = path[i];
        }
    }
}
