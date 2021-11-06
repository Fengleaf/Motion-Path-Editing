using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BVH : MonoBehaviour
{
    public BVHJoint jointPrefab;
    public GameObject bonePrefab;

    public int MoveSpeed = 5;

    private BVHJoint root;

    private List<BVHJoint> joints;

    private int frameNumber;
    private float frameTime;

    private List<Vector3> pathPoints;

    private Coroutine runFrameCoroutine;
    private Coroutine runPositionCoroutine;

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

    public void Run()
    {
        if (runFrameCoroutine != null)
            StopCoroutine(runFrameCoroutine);
        if (runPositionCoroutine != null)
            StopCoroutine(runPositionCoroutine);
        pathPoints = new List<Vector3>();
        foreach (LineSegment segment in PathManager.Instance.segments)
        {
            for (int i = 0; i < segment.segmentRenderer.positionCount; i++)
                pathPoints.Add(segment.segmentRenderer.GetPosition(i));
        }
        runFrameCoroutine = StartCoroutine(RunFrameCoroutine());
        runPositionCoroutine = StartCoroutine(RunPositionCoroutine());
    }

    private IEnumerator RunFrameCoroutine()
    {
        int pathIndex = 0;
        int frameIndex = 0;
        float timeDelta = frameTime / Time.deltaTime;
        float time = 0;
        while (true)
        {
            UpdateFrame(frameIndex % frameNumber, time);
            //UpdatePosition(pathIndex % pathPoints.Count);
            //yield return new WaitForSeconds(frameTime);
            time += 1 / (frameTime / Time.deltaTime);
            Debug.Log(time);
            if (time >= 1)
            {
                time = 0;
                frameIndex++;
                //pathIndex++;
            }
            yield return null;
        }
    }

    private IEnumerator RunPositionCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                while (Vector3.Distance(pathPoints[i + 1], transform.position) >= MoveSpeed * Time.deltaTime)
                {
                    transform.position += (pathPoints[i + 1] - transform.position).normalized * MoveSpeed * Time.deltaTime;
                    UpdatePosition(i);
                    yield return null;
                }
            }
            transform.position = pathPoints[0];
        }
    }

    private void UpdateFrame(int frameIndex, float time)
    {
        // 每一個 joint
        for (int j = 0; j < joints.Count; j++)
        {
            joints[j].UpdateToFrame(frameIndex, time);
        }
        // Hips 不要動
        joints[0].transform.localPosition = Vector3.zero;
        for (int j = 0; j < joints.Count; j++)
        {
            joints[j].UpdateAllBone();
        }
    }

    private void UpdatePosition(int pathIndex)
    {
        // 面向切線方向
        if (pathIndex < pathPoints.Count - 1)
        {
            // A 點到 B 點的向量
            Vector3 vector = pathPoints[pathIndex + 1] - pathPoints[pathIndex];
            // 外積
            Quaternion rotation = Quaternion.Euler(0, 45, 0);
            Vector3 rotateVector = rotation * vector;
            Vector3 cross = Vector3.Cross(vector, rotateVector);
            transform.LookAt(transform.position + vector, cross);
        }
    }
}
