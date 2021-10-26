using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosController : MonoBehaviour
{
    public GameObject pivot;
    private GameObject ingamePivot;
    private bool isMoving;
    // Start is called before the first frame update
    void Start()
    {
        isMoving = false;
        ingamePivot = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            // Not moving any object.
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;
                    Debug.Log(objectHit.gameObject.name);
                    GameObject thePivot = Instantiate(pivot, Vector3.zero, Quaternion.identity);
                    thePivot.transform.SetParent(objectHit);
                    isMoving = true;
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                // Continuous pressing mouse
                // TODO - Move Object
                if (ingamePivot == null)
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        Transform objectHit = hit.transform;
                        Debug.Log(objectHit.gameObject.name);
                        GameObject thePivot = Instantiate(pivot, Vector3.zero, Quaternion.identity);
                        thePivot.transform.SetParent(objectHit);
                        isMoving = true;
                    }

                }
            }
        }
    }
}
