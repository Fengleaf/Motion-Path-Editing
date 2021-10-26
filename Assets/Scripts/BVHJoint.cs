using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHJoint : MonoBehaviour
{
    public List<string> channels = new List<string>();

    public GameObject FindJoint(string name)
    {
        if (gameObject.name.Contains(name))
            return gameObject;
        else
        {
            for (int i = 0;i < transform.childCount;i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                GameObject target = child.GetComponent<BVHJoint>().FindJoint(name);
                if (target != null)
                    return target;
            }
        }
        return null;
    }
}
