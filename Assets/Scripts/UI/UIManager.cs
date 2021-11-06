using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RectTransform RightPanel;
    public RectTransform LeftPanel;
    public Button rightVisBut;
    public Button leftVisBut;

    private bool isRightPanelShow;
    private bool isLeftPanelShow;

    // Start is called before the first frame update
    void Start()
    {
        isRightPanelShow = true;
        isLeftPanelShow = true;
        rightVisBut.onClick.AddListener(() => { 
            if (isRightPanelShow)
            {
                RightPanel.anchoredPosition = new Vector2(150, 0);
                rightVisBut.GetComponentInChildren<Text>().text = "＜";
            }
            else
            {
                RightPanel.anchoredPosition = new Vector2(-150, 0);
                rightVisBut.GetComponentInChildren<Text>().text = "＞";
            }
            isRightPanelShow = !isRightPanelShow;
        });
        leftVisBut.onClick.AddListener(() => {
            if (isLeftPanelShow)
            {
                LeftPanel.anchoredPosition = new Vector2(-150, 0);
                leftVisBut.GetComponentInChildren<Text>().text = "＞";
            }
            else
            {
                LeftPanel.anchoredPosition = new Vector2(150, 0);
                leftVisBut.GetComponentInChildren<Text>().text = "＜";
            }
            isLeftPanelShow = !isLeftPanelShow;
        });
    }
}
