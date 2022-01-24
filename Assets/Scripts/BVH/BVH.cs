using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BVH : MonoBehaviour
{
    public PathManager pathManager;

    public List<string> motionString = new List<string>();

    public BVHJoint jointPrefab;
    public GameObject bonePrefab;

    public int MoveSpeed = 5;

    public BVHJoint root;

    public List<BVHJoint> joints;

    private int frameNumber;
    private float frameTime;

    public List<Vector3> originPathPointOrientation;
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
        originPathPointOrientation = pathManager.GetOriOrientations();

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
            UpdatePosition(pathIndex % orientationPoints.Count);
            BVHUnityChan.Instance.UpdateMotionPos(gameObject);
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
        if (pathIndex == frameNumber - 1)
            return;
        Dictionary<string, float> frame = root.GetFrameData(pathIndex);
        Dictionary<string, float> nextFrame = root.GetFrameData(pathIndex + 1);

        //Vector3 ori = originPathPoint[pathIndex];
        //Vector3 oriNext = originPathPoint[pathIndex + 1];
        //Vector3 oriTangent = oriNext - ori;

        //Vector3 now = new Vector3(frame[BVHJoint.XPosition], frame[BVHJoint.YPosition], frame[BVHJoint.ZPosition]);
        //Vector3 next = new Vector3(nextFrame[BVHJoint.XPosition], nextFrame[BVHJoint.YPosition], nextFrame[BVHJoint.ZPosition]);
        //Vector3 newTangent = next - now;

        float theta = Vector3.SignedAngle(originPathPointOrientation[pathIndex], orientationPoints[pathIndex], Vector3.up);
        //if (m < 0)
        //    theta *= -1;
        root.transform.RotateAround(root.transform.position, Vector3.up, theta);

        //float testTheta = (oriTangent - newTangent) / (1 + oriTangent * newTangent);
        Debug.DrawLine(root.transform.position, root.transform.position + originPathPointOrientation[pathIndex] * 40, Color.yellow);
        Debug.DrawLine(root.transform.position, root.transform.position + orientationPoints[pathIndex] * 40, Color.red);
        Debug.DrawLine(root.transform.position, root.transform.position + Vector3.up * 100, Color.green);
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

    public void Concatenate(BVH target)
    {
        int startIndex = frameNumber - 1;

        Dictionary<string, Vector3> lastPosition = GetFramePosition(startIndex);
        Dictionary<string, Vector3> newPosition = target.GetFramePosition(0);

        Dictionary<string, Vector3> offsetPosition = new Dictionary<string, Vector3>();
        for (int i = 0; i < lastPosition.Count && i < newPosition.Count; i++)
            offsetPosition[joints[i].name] = lastPosition[joints[i].name] - newPosition[joints[i].name];

        // 找最接近的兩個frame
        float min = Mathf.Infinity;
        int indexJ = 0;
        Dictionary<string, Vector3> firstPos = GetFramePosition(startIndex);
        Dictionary<string, Vector3> firstRot = GetFrameRotation(startIndex);
        for (int j = 0; j < 5; j++)
        {
            Dictionary<string, Vector3> secondPos = target.GetFramePosition(j);
            Dictionary<string, Vector3> secondRot = target.GetFrameRotation(j);
            float minValue = 0;
            foreach (string key in firstPos.Keys)
                minValue += Vector3.Distance(secondPos[key], firstPos[key]);
            foreach (string key in firstRot.Keys)
                minValue += Vector3.Distance(secondRot[key], firstRot[key]);
            if (minValue < min)
            {
                min = minValue;
                indexJ = j;
            }

        }

        Dictionary<string, Vector3> myPos = GetFramePosition(startIndex);
        Dictionary<string, Vector3> myRot = GetFrameRotation(startIndex);
        Dictionary<string, Vector3> taPos = target.GetFramePosition(indexJ);
        Dictionary<string, Vector3> taRot = target.GetFrameRotation(indexJ);

        List<Dictionary<string, Vector3>> interPos = new List<Dictionary<string, Vector3>>();
        List<Dictionary<string, Vector3>> interRot = new List<Dictionary<string, Vector3>>();
        // 插值
        for (float t = 0.1f; t < 1; t += 0.1f)
        {
            interPos.Add(new Dictionary<string, Vector3>());
            interRot.Add(new Dictionary<string, Vector3>());
            foreach (string key in myPos.Keys)
                interPos[interPos.Count - 1].Add(key, myPos[key] + (taPos[key] - myPos[key]) * t);
            foreach (string key in myRot.Keys)
                interRot[interRot.Count - 1].Add(key, myRot[key] + (taRot[key] - myRot[key]) * t);
        }

        for (int i = 0; i < interRot.Count; i++)
        {
            for (int j = 0; j < target.joints.Count; j++)
            {
                Dictionary<string, float> datas = new Dictionary<string, float>();
                BVHJoint joint = target.joints.Find(x => x.name == joints[j].name);
                if (joint == null)
                    continue;
                if (j == 0)
                {
                    Vector3 newPoint = joint.GetPosition(0);
                    Vector3 offset = offsetPosition[joints[j].name];
                    datas[BVHJoint.XPosition] = (newPoint + offset).x;
                    datas[BVHJoint.YPosition] = (newPoint + offset).y;
                    datas[BVHJoint.ZPosition] = (newPoint + offset).z;
                }
                datas[BVHJoint.XRotation] = interRot[i][joint.name].x;
                datas[BVHJoint.YRotation] = interRot[i][joint.name].y;
                datas[BVHJoint.ZRotation] = interRot[i][joint.name].z;
                joints[j].AddFrameData(datas);
            }
            frameNumber++;
        }

        for (int i = indexJ; i < target.frameNumber; i++)
        {
            for (int j = 0; j < target.joints.Count; j++)
            {
                Dictionary<string, float> datas = new Dictionary<string, float>();
                BVHJoint joint = target.joints.Find(x => x.name == joints[j].name);
                if (joint == null)
                    continue;
                if (j == 0)
                {
                    Vector3 newPoint = joint.GetPosition(i);
                    Vector3 offset = offsetPosition[joints[j].name];
                    datas[BVHJoint.XPosition] = (newPoint + offset).x;
                    datas[BVHJoint.YPosition] = (newPoint + offset).y;
                    datas[BVHJoint.ZPosition] = (newPoint + offset).z;
                }
                Vector3 newRotation = joint.GetRotationData(i);
                datas[BVHJoint.XRotation] = newRotation.x;
                datas[BVHJoint.YRotation] = newRotation.y;
                datas[BVHJoint.ZRotation] = newRotation.z;
                joints[j].AddFrameData(datas);
            }
        }
        frameNumber += target.frameNumber - indexJ;
        Debug.Log(string.Join("\n", GetAllPath()));

        pathManager.SetBezierFitPath(GetAllPath());
    }

    public void Blend(BVH target, float weight)
    {
        for (int i = 0; i < frameNumber && i < target.frameNumber; i++)
        {
            //Dictionary<string, Vector3> myPosition = GetFramePosition(i);
            //Dictionary<string, Vector3> targetPosition = target.GetFramePosition(i);

            //Dictionary<string, Vector3> myRotation= GetFrameRotation(i);
            //Dictionary<string, Vector3> targetRotation= target.GetFrameRotation(i);

            for (int j = 0; j < joints.Count; j++)
            {
                BVHJoint targetJoint = target.joints.Find(x => x.name == joints[j].name);
                if (targetJoint == null)
                    continue;
                Vector3 me;
                Vector3 ta;
                Vector3 interpolation;
                if (j == 0)
                {
                    me = joints[j].GetPosition(i);
                    ta = targetJoint.GetPosition(i);
                    interpolation = me * (1 - weight) + ta * weight;
                    joints[j].ChangeFrameData(i, BVHJoint.XPosition, interpolation.x);
                    joints[j].ChangeFrameData(i, BVHJoint.YPosition, interpolation.y);
                    joints[j].ChangeFrameData(i, BVHJoint.ZPosition, interpolation.z);
                }
                me = joints[j].GetRotationData(i);
                ta = targetJoint.GetRotationData(i);
                interpolation = me * (1 - weight) + ta * weight;
                joints[j].ChangeFrameData(i, BVHJoint.XRotation, interpolation.x);
                joints[j].ChangeFrameData(i, BVHJoint.YRotation, interpolation.y);
                joints[j].ChangeFrameData(i, BVHJoint.ZRotation, interpolation.z);
            }
        }
        pathManager.SetBezierFitPath(GetAllPath());
        LoadPath(pathManager.GetPath());
    }

    public Dictionary<string, Vector3> GetFramePosition(int frameIndex)
    {
        Dictionary<string, Vector3> position = new Dictionary<string, Vector3>();
        foreach (BVHJoint joint in joints)
            position[joint.name] = joint.GetPosition(frameIndex);
        return position;
    }

    public Dictionary<string, Vector3> GetFrameRotation(int frameIndex)
    {
        Dictionary<string, Vector3> rotation = new Dictionary<string, Vector3>();
        foreach (BVHJoint joint in joints)
            rotation[joint.name] = joint.GetRotationData(frameIndex);
        return rotation;
    }
}
