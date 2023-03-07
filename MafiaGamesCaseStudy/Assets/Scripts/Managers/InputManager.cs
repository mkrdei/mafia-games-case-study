using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance;
        public static event Action<Transform> OnSelect;
        public static event Action<Transform> OnRelease;
        private Transform lastSelectedObject;
        private bool disabled;
        // Start is called before the first frame update
        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        // Update is called once per frame
        void Update()
        {
            if (!disabled)
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray,out hit, 100))
                    {
                        if (hit.transform.tag == "Item")
                        {
                            lastSelectedObject = hit.transform;
                            OnSelect?.Invoke(lastSelectedObject);
                        }
                    }
                } 
                else if(Input.GetKeyUp(KeyCode.Mouse0))
                {
                    if (lastSelectedObject != null)
                        OnRelease?.Invoke(lastSelectedObject);
                }
        }
        public void DisableInput()
        {
            disabled = true;
        }
        public void EnableInput()
        {
            disabled = false;
        }
    }
}
