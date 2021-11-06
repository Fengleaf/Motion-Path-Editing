using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHJoint : MonoBehaviour
{
    public const string XPosition = "Xposition";
    public const string YPosition = "Yposition";
    public const string ZPosition = "Zposition";

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

    public void UpdateToFrame(int frameNumber, float time)
    {
        if (frameNumber >= frames.Count)
            return;
        Dictionary<int, float> frameData = frames[frameNumber];
        Vector3 next = GetRotation(frameNumber, frameData).eulerAngles;
        Vector3 position = GetPosition(frameNumber, frameData);
        Vector3 interpolated = next;
        // 線性插值
        if (frameNumber > 0)
        {
            Vector3 now = GetRotation(frameNumber - 1, frameData).eulerAngles;
            interpolated = now + time * (next - now);
        }
        transform.localPosition = position;
        transform.localRotation = Quaternion.Euler(interpolated);
    }

    public Vector3 GetPosition(int frameNumber, Dictionary<int, float> frameData)
    {
        Vector3 position = transform.localPosition;
        foreach (KeyValuePair<int, float> pair in frameData)
        {
            string channel = channels[pair.Key];
            if (channel == "Xposition")
                position.x = pair.Value;
            else if (channel == "Yposition")
                position.y = pair.Value;
            else if (channel == "Zposition")
                position.z = pair.Value;
        }
        return position;
    }

    public Vector3 GetPosition(int frameNumber)
    {
        Vector3 position = transform.localPosition;
        foreach (KeyValuePair<int, float> pair in frames[frameNumber])
        {
            string channel = channels[pair.Key];
            if (channel == "Xposition")
                position.x = pair.Value;
            else if (channel == "Yposition")
                position.y = pair.Value;
            else if (channel == "Zposition")
                position.z = pair.Value;
        }
        return position;
    }

    public Quaternion GetRotation(int frameNumber, Dictionary<int, float> frameData)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i < channels.Count; i++)
        {
            string channel = channels[i];
            float value = frameData[i];
            if (i == 0)
            {
                if (channel == "Zrotation")
                    //rotation.z = value;
                    rotation = Quaternion.Euler(0, 0, value);
                else if (channel == "Xrotation")
                    //rotation.x = value;
                    rotation = Quaternion.Euler(value, 0, 0);
                else if (channel == "Yrotation")
                    //rotation.y = value;
                    rotation = Quaternion.Euler(0, value, 0);
            }
            else
            {
                if (channel == "Zrotation")
                    //rotation.z = value;
                    rotation *= Quaternion.Euler(0, 0, value);
                else if (channel == "Xrotation")
                    //rotation.x = value;
                    rotation *= Quaternion.Euler(value, 0, 0);
                else if (channel == "Yrotation")
                    //rotation.y = value;
                    rotation *= Quaternion.Euler(0, value, 0);
            }
        }
        return rotation;
    }

    public void ChangeFrameData(int frameNumber, string channelName, float value)
    {
        int channelIndex = channels.IndexOf(channelName);
        frames[frameNumber][channelIndex] = value;
    }
}
