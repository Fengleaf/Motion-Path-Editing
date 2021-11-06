using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject UIPanel;
    public Button visBut;
    private bool isPanelShow;
    // Start is called before the first frame update
    void Start()
    {
        isPanelShow = true;
        visBut.onClick.AddListener(PanelVisManager);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PanelVisManager()
    {
        if (isPanelShow)
        {
            // Hide
            //transform. = -100;

        }
    }
}
