using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionFrameList
{

}

public class BVH : MonoBehaviour
{
    public BVHJoint jointPrefab;

    public BVHJoint root;

    public void AddRoot(string name)
    {
        root = Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
        root.gameObject.name = name;
        root.transform.SetParent(transform);
    }

    public void AddJoint(string name, string parent)
    {
        BVHJoint joint = Instantiate(jointPrefab, root.FindJoint(parent).transform);
        joint.gameObject.name = name;
    }

    public void SetJointOffset(string name, Vector3 offset)
    {
        BVHJoint joint = root.FindJoint(name).GetComponent<BVHJoint>();
        joint.transform.localPosition = offset;
    }

    public void SetJointChannels(string name, List<string> channels)
    {
        BVHJoint joint = root.FindJoint(name).GetComponent<BVHJoint>();
        joint.channels = channels;
    }
}
