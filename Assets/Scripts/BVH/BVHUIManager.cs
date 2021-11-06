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

    private Dictionary<int, BVH> bvhDict = new Dictionary<int, BVH>();
    private int nowIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        visibilityToggle.onValueChanged.AddListener(x => ChangeVisibility(x));
    }

    public void AddNewBVH(BVH bVH)
    {
        Button button = Instantiate(bvhButtonPrefab, bvhButtonItemContainer);
        button.GetComponentInChildren<Text>().text = bVH.name;
        int index = bvhButtonItemContainer.childCount - 1;
        bvhDict[index] = bVH;
        button.onClick.AddListener(() => OnBVHButtonClick(index));
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
}
