using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InitButton : MonoBehaviour
{
    public GameObject lastSelect;
    // Start is called before the first frame update
    void Start()
    {
        lastSelect = new GameObject();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelect = EventSystem.current.currentSelectedGameObject;
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(lastSelect);
        }
    }
}
