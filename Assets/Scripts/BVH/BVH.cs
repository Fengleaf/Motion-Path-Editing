using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BVH : MonoBehaviour
{
    public PathManager pathManager;

    public BVHJoint jointPrefab;
    public GameObject bonePrefab;

    public int MoveSpeed = 5;

    public BVHJoint root;

    private List<BVHJoint> joints;

    private int frameNumber;
    private float frameTime;

    public List<Vector3> originPathPoint;
    private List<Vector3> pathPoints;
    private List<Vector3> orientationPoints;

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


        pathPoints = pathManager.GetPath();
        LoadPath(pathPoints);

        orientationPoints = pathManager.GetOrientations();

        runFrameCoroutine = StartCoroutine(RunFrameCoroutine());
        //runPositionCoroutine = StartCoroutine(RunPositionCoroutine());
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
            //UpdatePosition(pathIndex % orientationPoints.Count);
            //yield return new WaitForSeconds(frameTime);
            time += 1 / (frameTime / Time.deltaTime);
            if (time >= 1)
            {
                time = 0;
                frameIndex++;
                pathIndex++;
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
        //joints[0].transform.localPosition = Vector3.zero;
        for (int j = 0; j < joints.Count; j++)
        {
            joints[j].UpdateAllBone();
        }
    }

    private void UpdatePosition(int pathIndex)
    {
        // 面向切線方向
        if (pathIndex < orientationPoints.Count - 1)
        {
            Vector3 now = orientationPoints[pathIndex];// root.GetPosition(pathIndex);
            //Vector3 next = orientationPoints[pathIndex + 1]; //root.GetPosition(pathIndex + 1);
            // A 點到 B 點的向量
            //Vector3 vector = next - now;
            // 外積
            Quaternion rotation = Quaternion.Euler(0, 45, 0);
            Vector3 rotateVector = rotation * now;
            Vector3 cross = Vector3.Cross(now, rotateVector);
            //transform.LookAt(transform.position + now, cross);
            //transform.rotation = Quaternion.identity;
            Debug.Log(now);
            float angle = Vector3.Angle(originPathPoint[pathIndex], pathPoints[pathIndex]);
            transform.rotation = Quaternion.identity;
            transform.Rotate(root.transform.position, angle);
            //transform.rotation.SetFromToRotation(transform.rotation.eulerAngles, now);
        }
    }

    public List<Vector3> GetAllPath()
    {
        List<Vector3> path = new List<Vector3>();
        for (int i = 0; i < frameNumber; i++)
        {
            path.Add(root.GetPosition(i));
        }
        return path;
    }

    public void LoadPath(List<Vector3> newPath)
    {
        for (int i = 0; i < newPath.Count; i++)
        {
            root.ChangeFrameData(i, BVHJoint.XPosition, newPath[i].x);
            root.ChangeFrameData(i, BVHJoint.YPosition, newPath[i].y);
            root.ChangeFrameData(i, BVHJoint.ZPosition, newPath[i].z);
        }
    }

    public void Blend(BVH target)
    {
        int startIndex = frameNumber - 1;
        Dictionary<BVHJoint, Vector3> lastPosition = GetFramePosition(startIndex);
        //List<Vector3> lastRotation = GetFrameRotation(startIndex);
        Dictionary<BVHJoint, Vector3> newPosition = target.GetFramePosition(0);
        //List<Vector3> newRotation = target.GetFrameRotation(target.frameNumber - 1);

        Dictionary<BVHJoint, Vector3> offsetPosition = new Dictionary<BVHJoint, Vector3>();
        for (int i = 0; i < lastPosition.Count && i < newPosition.Count; i++)
            offsetPosition[joints[i]] = lastPosition[joints[i]] - newPosition[target.joints[i]];

        //List<Vector3> offsetRotation = new List<Vector3>();
        //for (int i = 0; i < lastRotation.Count && i < newRotation.Count; i++)
        //    offsetRotation.Add(lastRotation[i] - newRotation[i]);

        for (int i = 0; i < target.frameNumber; i++)
        {
            for (int j = 0; j < target.joints.Count; j++)
            {
                Dictionary<string, float> datas = new Dictionary<string, float>();
                Vector3 newPoint = target.joints[j].GetPosition(i);
                Vector3 offset = offsetPosition[joints[j]];
                datas[BVHJoint.XPosition] = (newPoint + offset).x;
                datas[BVHJoint.YPosition] = (newPoint + offset).y;
                datas[BVHJoint.ZPosition] = (newPoint + offset).z;
                Vector3 newRotation = target.joints[j].GetRotationData(i);
                datas[BVHJoint.XRotation] = newRotation.x;
                datas[BVHJoint.YRotation] = newRotation.y;
                datas[BVHJoint.ZRotation] = newRotation.z;
                joints[j].AddFrameData(datas);
            }
        }
        frameNumber += target.frameNumber;
        pathManager.SetBezierFitPath(GetAllPath());
    }

    public Dictionary<BVHJoint, Vector3> GetFramePosition(int frameIndex)
    {
        Dictionary<BVHJoint, Vector3> position = new Dictionary<BVHJoint, Vector3>();
        foreach (BVHJoint joint in joints)
            position[joint] = joint.GetPosition(frameIndex);
        return position;
    }

    public Dictionary<BVHJoint, Vector3> GetFrameRotation(int frameIndex)
    {
        Dictionary<BVHJoint, Vector3> rotation = new Dictionary<BVHJoint, Vector3>();
        foreach (BVHJoint joint in joints)
            rotation[joint] = joint.GetRotationData(frameIndex);
        return rotation;
    }
}
