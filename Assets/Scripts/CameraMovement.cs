using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FollowState
{
    None,
    Top,
    Left,
    Right,
    Back,
    Front
}

public class CameraMovement : MonoBehaviour
{
    public float speed = 5.0f;

    // 相機旋轉
    private Vector2 cameraRotation;
    //滑鼠敏度  
    public static float mousesSensity = 3.0f;

    public float followParameter = 200;

    private ControllPntManager controllPntManager;

    private FollowState state;
    private BVH target;

    private void Awake()
    {
        controllPntManager = GetComponent<ControllPntManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // 左右
        float horizontal = Input.GetAxis("Horizontal");
        // 前進
        float zoom = Input.GetAxis("Zoom");
        // 上下
        float vertical = Input.GetAxis("Vertical");
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        transform.Translate(new Vector3(horizontal, vertical, zoom) * speed * Time.deltaTime);
        if (wheel != 0)
        {
            Vector3 vector = transform.position;
            vector += transform.forward * wheel * speed * 5;
            transform.position = vector;
            state = FollowState.None;
        }
        if (Input.GetMouseButton(2))
        {
            transform.position -= transform.right * mouseX * speed;
            transform.position -= transform.up * mouseY * speed;
            state = FollowState.None;
        }
        if (Input.GetMouseButton(1) && !controllPntManager.IsMoving)
        {
            //根據滑鼠的移動,獲取相機旋轉的角度
            cameraRotation.x = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mousesSensity;
            cameraRotation.y += Input.GetAxis("Mouse Y") * mousesSensity;
            //相機角度隨著滑鼠旋轉  
            transform.localEulerAngles = new Vector3(-cameraRotation.y, cameraRotation.x, 0);
            state = FollowState.None;
        }

        switch (state)
        {
            case FollowState.Top:
                transform.position = target.root.transform.position + Vector3.up * followParameter;
                transform.LookAt(target.root.transform);
                break;
            case FollowState.Left:
                transform.position = target.root.transform.position - Vector3.right * followParameter;
                transform.LookAt(target.root.transform);
                break;
            case FollowState.Right:
                transform.position = target.root.transform.position + Vector3.right * followParameter;
                transform.LookAt(target.root.transform);
                break;
            case FollowState.Back:
                transform.position = target.root.transform.position - Vector3.forward * followParameter;
                transform.LookAt(target.root.transform);
                break;
            case FollowState.Front:
                transform.position = target.root.transform.position + Vector3.forward * followParameter;
                transform.LookAt(target.root.transform);
                break;
            default:
                break;
        }

    }

    public void Follow(BVH bvh, FollowState state)
    {
        this.state = state;
        target = bvh;
    }
}