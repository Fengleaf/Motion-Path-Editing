using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BVHUnityChan : MonoBehaviour
{
    [Header("========== 原始的資料 =========")]
    public List<GameObject> Joints = new List<GameObject>();
    public List<float> MotionPos = new List<float>();
    
    private bool IsSetOffsetY = false;
    private float OffsetY;

    public static BVHUnityChan Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateMotionPos(GameObject people)
    {
        int CurrentIndex = 0;
        BVH bvhPeople = people.GetComponent<BVH>();
        BVHJoint[] joints = bvhPeople.joints.ToArray();
        
        for(int i = 0; i < joints.Length; i++)
        {
            Transform TempBonesTran = SearchHumanBoneTransformByName(joints[i].name);
            if (TempBonesTran == null)
            {
                Debug.Log("No match: " + joints[i].name);
                continue;
            }

            // 把 Motion 套上去
            Quaternion org = new Quaternion(MotionPos[CurrentIndex * 4],
                MotionPos[CurrentIndex * 4 + 1],
                MotionPos[CurrentIndex * 4 + 2],
                MotionPos[CurrentIndex * 4 + 3]);
            
            Joints[CurrentIndex++].transform.rotation = joints[i].transform.rotation * org;
        }

        // 位移
        Vector3 pos = joints[0].transform.localPosition;
        //pos.x /= bvhPeople.InitMotionHeight;
        //pos.y /= bvhPeople.InitMotionHeight;
        //pos.z /= bvhPeople.InitMotionHeight;

        if (!IsSetOffsetY && pos != Vector3.zero)
        {
            OffsetY = pos.y;
            IsSetOffsetY = true;
        }


        pos.y -= OffsetY;
        this.transform.localPosition = pos;
    }

    public void GetDefaultData(string[] MotionStringList)
    {
        string lastName = "";
        string[] PartInfo = null;

        // 重新設定 Motion
        ResetMotion();

        for (int i = 0; i < MotionStringList.Length; i++)
        {
            PartInfo = MotionStringList[i].Split(' ');
            if (lastName != "" && lastName != PartInfo[0])
            {
                SaveDefaultValue(lastName);
                lastName = PartInfo[0];
            }

            // First Case
            if (lastName == "")
                lastName = PartInfo[0];
        }
        SaveDefaultValue(lastName);
    }
    private void ResetMotion()
    {
        this.transform.localPosition = Vector3.zero;
        for (int i = 0; i < Joints.Count; i++)
        {
            // 把 Motion 套上去
            Quaternion org = new Quaternion(MotionPos[i * 4],
                MotionPos[i * 4 + 1],
                MotionPos[i * 4 + 2],
                MotionPos[i * 4 + 3]);

            Joints[i].transform.rotation = org;
        } 

        MotionPos.Clear();
        Joints.Clear();
        IsSetOffsetY = false;

    }
    private void SaveDefaultValue(string name)
    {
        Transform BodyBonesTran = SearchHumanBoneTransformByName(name);
        if (BodyBonesTran == null)
        {
            Debug.Log("Bone not found: " + name);
            return;
        }

        // 把關節加進去
        Joints.Add(BodyBonesTran.gameObject);
        
        MotionPos.Add(BodyBonesTran.transform.rotation.x);
        MotionPos.Add(BodyBonesTran.transform.rotation.y);
        MotionPos.Add(BodyBonesTran.transform.rotation.z);
        MotionPos.Add(BodyBonesTran.transform.rotation.w);
    }
   
    private Transform SearchHumanBoneTransformByName(string name)
    {
        HumanBodyBones temp = new HumanBodyBones();
        switch(name)
        {
            case "Hips":
                temp = HumanBodyBones.Hips;
                break;
            #region 左下半邊 ( 跟 Motion 的資料顛倒 )
            case "LeftUpLeg":
            case "LeftHip":
                temp = HumanBodyBones.RightUpperLeg;
                break;
            case "LeftLowLeg":
            case "LeftKnee":
                temp = HumanBodyBones.RightLowerLeg;
                break;
            case "LeftFoot":
            case "LeftAnkle":
                temp = HumanBodyBones.RightFoot;
                break;
            #endregion
            #region 右下半邊
            case "RightUpLeg":
            case "RightHip":
                temp = HumanBodyBones.LeftUpperLeg;
                break;
            case "RightLowLeg":
            case "RightKnee":
                temp = HumanBodyBones.LeftLowerLeg;
                break;
            case "RightFoot":
            case "RightAnkle":
                temp = HumanBodyBones.LeftFoot;
                break;
            #endregion
            #region 上半身
            case "Chest":
                temp = HumanBodyBones.Chest;
                break;
            case "LeftCollar":
            case "LeftShoulder":
                temp = HumanBodyBones.RightShoulder;
                break;
            case "LeftUpArm":
                temp = HumanBodyBones.RightUpperArm;
                break;
            case "LeftLowArm":
            case "LeftElbow":
                temp = HumanBodyBones.RightLowerArm;
                break;
            case "LeftHand":
            case "LeftWrist":
                temp = HumanBodyBones.RightHand;
                break;

            case "RightCollar":
            case "RightShoulder":
                temp = HumanBodyBones.LeftShoulder;
                break;
            case "RightUpArm":
                temp = HumanBodyBones.LeftUpperArm;
                break;
            case "RightLowArm":
            case "RightElbow":
                temp = HumanBodyBones.LeftLowerArm;
                break;
            case "RightHand":
            case "RightWrist":
                temp = HumanBodyBones.LeftHand;
                break;
            #endregion
            #region 頭部
            case "Neck":
                temp = HumanBodyBones.Neck;
                break;
            case "Head":
                temp = HumanBodyBones.Head;
                break;
            #endregion
            default:
                return null;
        }
        return this.GetComponent<Animator>().GetBoneTransform(temp);
    }
}
