using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BVH : MonoBehaviour
{
    public BVHJoint jointPrefab;
    public GameObject bonePrefab;

    public BVHJoint root;

    private List<BVHJoint> joints;

    private int frameNumber;
    private float frameTime;

    private void Awake()
    {
        joints = new List<BVHJoint>();
        frameNumber = 0;
        frameTime = 0;
    }

    public void AddRoot(string name)
    {
        root = Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
        root.gameObject.name = name;
        root.transform.SetParent(transform);
        joints.Add(root);
    }

    public void AddJoint(string name, string parent)
    {
        GameObject parentJoint = root.FindJoint(parent);
        BVHJoint joint = Instantiate(jointPrefab, parentJoint.transform);
        joint.gameObject.name = name;
        joint.parentJoint = parentJoint.GetComponent<BVHJoint>();
        if (!name.Contains("End Site"))
            joints.Add(joint);
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

    public void SetFrameTime(float time)
    {
        frameTime = time;
    }

    public void AddMotionFrame(List<string> value)
    {
        List<float> data = new List<float>();
        for (int i = 0; i < value.Count; i++)
            data.Add(Convert.ToSingle(value[i]));
        int index = 0;
        for (int i = 0; i < joints.Count; i++)
        {
            for (int j = 0; j < joints[i].channels.Count; j++)
            {
                joints[i].AddFrames(frameNumber, j, data[index]);
                index++;
            }
        }
        frameNumber++;
    }

    public void SetPath()
    {

    }

    public void Run()
    {
        StartCoroutine(RunCoroutine());
    }

    private IEnumerator RunCoroutine()
    {
        while (true)
        {
            Debug.Log(PathManager.Instance);
            // 每一個 frame
            for (int i = 0; i < frameNumber; i++)
            {
                GameManager.Instance.frameText.text = (i + 1).ToString();
                // 每一個 joint
                for (int j = 0; j < joints.Count; j++)
                {
                    joints[j].UpdateToFrame(i);
                }
                for (int j = 0; j < joints.Count; j++)
                {
                    joints[j].UpdateAllBone();
                }
                yield return new WaitForSeconds(frameTime);
            }
            yield return null;
        }
    }
}
