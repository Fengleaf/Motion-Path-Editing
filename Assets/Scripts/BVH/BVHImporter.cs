using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class BVHImporter : MonoBehaviour
{
    public PathManager pathManagerPrefab;

    public BVH bvhPrefab;

    public GameObject BonePrefab;
    public GameObject JointPrefab;

    public void ImportBVH()
    {
        string[] fileName = StandaloneFileBrowser.OpenFilePanel("匯入BVH", Path.Combine(Application.streamingAssetsPath, "BVHs"), "bvh", false);
        if (fileName.Length == 0 || fileName[0] == "")
            return;
        StartCoroutine(ParseBVH(fileName[0]));
    }

    private IEnumerator ParseBVH(string fileName)
    {
        using (StreamReader reader = new StreamReader(fileName))
        {
            string line = reader.ReadLine();
            string[] inputs = line.Split(' ');
            string jointName = "";
            if (inputs[0] == "HIERARCHY")
            {
                BVH bvh = Instantiate(bvhPrefab, Vector3.zero, Quaternion.identity);
                // 用來儲存遞迴結構
                List<string> jointNames = new List<string>();
                while (inputs[0] != "MOTION")
                {
                    line = reader.ReadLine();
                    line = line.Trim();
                    line = Regex.Replace(line, @"\s+", " ");
                    inputs = line.Split(' ');
                    if (inputs[0] == "ROOT")
                    {
                        jointName = line.Split(' ')[1];
                        bvh.AddRoot(inputs[1]);
                        yield return null;
                    }
                    // 每讀到一個左括號，新增一層
                    else if (inputs[0] == "{")
                        jointNames.Add(jointName);
                    // 偏移量
                    else if (inputs[0] == "OFFSET")
                    {
                        for (int i = 1; i < inputs.Length; i++)
                        {
                            if (!IsFloat(inputs[i]))
                            {
                                Debug.Log("BVH ERROR!");
                                yield break;
                            }
                        }
                        Vector3 offset = new Vector3(Convert.ToSingle(inputs[1]), Convert.ToSingle(inputs[2]), Convert.ToSingle(inputs[3]));
                        bvh.SetJointOffset(jointNames[jointNames.Count - 1], offset);
                    }
                    // 頻道
                    else if (inputs[0] == "CHANNELS")
                    {
                        List<string> channels = new List<string>();
                        for (int i = 0; i < Convert.ToInt32(inputs[1]); i++)
                        {
                            channels.Add(inputs[i + 2]);
                            bvh.motionString.Add(jointName + " " + inputs[i + 2]);
                        }
                        bvh.SetJointChannels(jointNames[jointNames.Count - 1], channels);
                    }
                    // 新的關節
                    else if (inputs[0] == "JOINT")
                    {
                        // 堆疊最上層為父關節
                        jointName = inputs[1];
                        bvh.AddJoint(inputs[1], jointNames[jointNames.Count - 1]);
                        yield return null;
                    }
                    else if (inputs[0] == "End")
                    {
                        jointName = jointNames[jointNames.Count - 1] + " End Site";
                        bvh.AddJoint(jointName, jointNames[jointNames.Count - 1]);
                    }
                    // 右括號，減少一層
                    else if (inputs[0] == "}")
                        jointNames.RemoveAt(jointNames.Count - 1);
                    else if (inputs[0] == "MOTION")
                        ;
                    else
                    {
                        Debug.Log("BVH ERROR! " + inputs[0]);
                        yield break;
                    }
                }
                // Motion
                // Frames
                line = reader.ReadLine();
                line = line.Trim();
                line = Regex.Replace(line, @"\s+", " ");
                inputs = line.Split(' ');
                int frames = Convert.ToInt32(inputs[1]);
                // Frame Time
                line = reader.ReadLine();
                line = line.Trim();
                line = Regex.Replace(line, @"\s+", " ");
                inputs = line.Split(' ');
                bvh.SetFrameTime(Convert.ToSingle(inputs[2]));
                // Detail frames
                for (int i = 0; i < frames; i++)
                {
                    line = reader.ReadLine();
                    line = line.Trim();
                    line = Regex.Replace(line, @"\s+", " ");
                    inputs = line.Split(' ');
                    bvh.AddMotionFrame(new List<string>(inputs));
                }
                bvh.name = fileName.Substring(fileName.LastIndexOf('\\') + 1, fileName.IndexOf(".bvh") - fileName.LastIndexOf('\\') - 1);
                // Hips 的所有路徑
                List<Vector3> path = bvh.GetAllPath();
                //bvh.originPathPoint = path;
                PathManager aPathManager = Instantiate(pathManagerPrefab);
                yield return null;
                aPathManager.SetBezierFitPath(path);
                bvh.pathManager = aPathManager;
                BVHUIManager.Instance.AddNewBVH(bvh);
            }
            else
            {
                Debug.LogError("Not fount HIERARCHY!!");
                yield break;
            }
        }
    }

    private bool IsFloat(string str)
    {
        return float.TryParse(str, out _);
    }
}
