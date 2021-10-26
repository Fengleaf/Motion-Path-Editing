using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionFrameList
{

}

public class BVH : MonoBehaviour
{
    public BVHJoint jointPrefab;
    public GameObject bonePrefab;

    public BVHJoint root;

    public void AddRoot(string name)
    {
        root = Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
        root.gameObject.name = name;
        root.transform.SetParent(transform);
    }

    public void AddJoint(string name, string parent)
    {
        GameObject parentJoint = root.FindJoint(parent);
        BVHJoint joint = Instantiate(jointPrefab, parentJoint.transform);
        joint.gameObject.name = name;
        joint.parentJoint = parentJoint.GetComponent<BVHJoint>();
        // 產生joint後，生成骨架
        GameObject bone = Instantiate(bonePrefab, parentJoint.transform);
        bone.name = parentJoint.name + " " + joint.name + " Bone";
        bone.transform.localPosition = Vector3.zero;
        parentJoint.GetComponent<BVHJoint>().bones.Add(joint.name, bone);
    }

    public void SetJointOffset(string name, Vector3 offset)
    {
        BVHJoint joint = root.FindJoint(name).GetComponent<BVHJoint>();
        joint.transform.localPosition = offset;
        // 調整骨架
        joint.parentJoint?.UpdateBone(joint.name);
    }

    public void SetJointChannels(string name, List<string> channels)
    {
        BVHJoint joint = root.FindJoint(name).GetComponent<BVHJoint>();
        joint.channels = channels;
    }
}
