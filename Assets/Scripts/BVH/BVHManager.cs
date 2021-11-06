using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BVHManager : MonoBehaviour
{
    public static BVHManager Instance;
    
    private List<BVH> bVHs;

    private void Awake()
    {
        Instance = this;
    }

}
