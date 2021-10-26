using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHJoint : MonoBehaviour
{
    public BVHJoint parentJoint;

    public List<string> channels = new List<string>();

    public Dictionary<string, GameObject> bones = new Dictionary<string, GameObject>();

    public GameObject FindJoint(string name)
    {
        if (gameObject.name.Contains(name))
            return gameObject;
        else
        {
            for (int i = 0;i < transform.childCount;i++)
            {
                BVHJoint child = transform.GetChild(i).GetComponent<BVHJoint>();
                GameObject target = child?.FindJoint(name);
                if (target != null)
                    return target;
            }
        }
        return null;
    }

    public void UpdateBone(string childName)
    {
        BVHJoint joint = FindJoint(childName).GetComponent<BVHJoint>();
        GameObject bone = bones[childName];
        // 中間
        bone.transform.localPosition = joint.transform.localPosition / 2;
        // 縮放
        bone.transform.localScale = new Vector3(1, 1, bone.transform.localPosition.magnitude * 2);
        bone.transform.LookAt(joint.transform);
    }
}
