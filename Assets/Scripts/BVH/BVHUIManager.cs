using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BVHUIManager : MonoBehaviour
{
    public static BVHUIManager Instance;

    public Button bvhButtonPrefab;
    public Transform bvhButtonItemContainer;

    public Toggle visibilityToggle;

    public Dropdown blendingDropdown;
    public Button blendingButton;

    public Button frontButton;
    public Button backButton;
    public Button topButton;
    public Button leftButton;
    public Button rightButton;

    private Dictionary<int, BVH> bvhDict = new Dictionary<int, BVH>();
    private int nowIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        visibilityToggle.onValueChanged.AddListener(x => ChangeVisibility(x));
        blendingButton.onClick.AddListener(() => BlendMotion());
        frontButton.onClick.AddListener(() => OnCameraFollowClick(FollowState.Front));
        backButton.onClick.AddListener(() => OnCameraFollowClick(FollowState.Back));
        topButton.onClick.AddListener(() => OnCameraFollowClick(FollowState.Top));
        leftButton.onClick.AddListener(() => OnCameraFollowClick(FollowState.Left));
        rightButton.onClick.AddListener(() => OnCameraFollowClick(FollowState.Right));
    }

    public void AddNewBVH(BVH bVH)
    {
        Button button = Instantiate(bvhButtonPrefab, bvhButtonItemContainer);
        button.GetComponentInChildren<Text>().text = bVH.name;
        int index = bvhButtonItemContainer.childCount - 1;
        bvhDict[index] = bVH;
        button.onClick.AddListener(() => OnBVHButtonClick(index));
        blendingDropdown.AddOptions(new List<string>() { bVH.name });
    }

    public void OnBVHButtonClick(int index)
    {
        nowIndex = index;
        visibilityToggle.isOn = bvhDict[nowIndex].gameObject.activeSelf;
    }

    public void ChangeVisibility(bool visible)
    {
        if (bvhDict[nowIndex].gameObject.activeSelf == visible)
            return;
        bvhDict[nowIndex].gameObject.SetActive(visible);
        bvhDict[nowIndex].pathManager.gameObject.SetActive(visible);
    }

    public void BlendMotion()
    {
        int dropdownIndex = blendingDropdown.value;
        BVH now = bvhDict[nowIndex];
        BVH target = bvhDict[dropdownIndex];
        now.Blend(target);
    }

    public void OnCameraFollowClick(FollowState state)
    {
        FindObjectOfType<CameraMovement>().Follow(bvhDict[nowIndex], state);
    }
}
