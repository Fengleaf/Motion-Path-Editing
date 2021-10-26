using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class BVHImporter : MonoBehaviour
{
    public BVH bvhPrefab;

    public GameObject BonePrefab;
    public GameObject JointPrefab;

    public void ImportBVH()
    {
        string fileName = EditorUtility.OpenFilePanel("匯入BVH", Path.Combine(Application.streamingAssetsPath, "BVHs"), "bvh");
        StartCoroutine(ParseBVH(fileName));
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
                        Vector3 offset = new Vector3(Convert.ToSingle(inputs[1]), Convert.ToSingle(inputs[2]), Convert.ToSingle(inputs[3]));
                        bvh.SetJointOffset(jointNames[jointNames.Count - 1], offset);
                    }
                    // 頻道
                    else if (inputs[0] == "CHANNELS")
                    {
                        List<string> channels = new List<string>();
                        for (int i = 0; i < Convert.ToInt32(inputs[1]); i++)
                            channels.Add(inputs[i + 2]);
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
                        jointName = jointNames[jointNames.Count -1] + " End Site";
                        bvh.AddJoint(jointName, jointNames[jointNames.Count - 1]);
                    }
                    // 右括號，減少一層
                    else if (inputs[0] == "}")
                        jointNames.RemoveAt(jointNames.Count - 1);
                }
            }
            else
            {
                Debug.LogError("Not fount HIERARCHY!!");
            }
        }
    }
}
