using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHJoint : MonoBehaviour
{
    public BVHJoint parentJoint;

    public List<string> channels = new List<string>();

    public List<Dictionary<int, float>> frames = new List<Dictionary<int, float>>();

    public Dictionary<string, GameObject> bones = new Dictionary<string, GameObject>();

    public GameObject FindJoint(string name)
    {
        if (gameObject.name.Contains(name))
            return gameObject;
        else
        {
            for (int i = 0; i < transform.childCount; i++)
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

    public void UpdateAllBone()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            BVHJoint child = transform.GetChild(i).GetComponent<BVHJoint>();
            if (child != null)
                UpdateBone(child.name);
        }
    }

    public void AddFrames(int frameNumber, int channelIndex, float value)
    {
        if (frameNumber >= frames.Count)
            frames.Add(new Dictionary<int, float>());
        frames[frameNumber][channelIndex] = value;
    }

    public void UpdateToFrame(int frameNumber)
    {
        if (frameNumber >= frames.Count)
            return;
        Dictionary<int, float> frameData = frames[frameNumber];
        Vector3 position = transform.localPosition;
        Vector3 rotation = transform.localRotation.eulerAngles;
        foreach (KeyValuePair<int, float> pair in frameData)
        {
            string channel = channels[pair.Key];
            if (channel == "Xposition")
                position.x = pair.Value;
            else if (channel == "Yposition")
                position.y = pair.Value;
            else if (channel == "Zposition")
                position.z = pair.Value;
            else if (channel == "Zrotation")
                rotation.z = pair.Value;
            else if (channel == "Xrotation")
                rotation.x = pair.Value;
            else if (channel == "Yrotation")
                rotation.y = pair.Value;
        }
        transform.localPosition = position;
        transform.localRotation = Quaternion.Euler(rotation);
    }
}
