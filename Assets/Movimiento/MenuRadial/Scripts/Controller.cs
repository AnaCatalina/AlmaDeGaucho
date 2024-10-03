using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

    public GameObject armasRing;
    public bool enMenuRadial;
  
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            enMenuRadial = true;
            armasRing.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        if(Input.GetKeyUp(KeyCode.Tab))
        {
            enMenuRadial = false;
            armasRing.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
    }
}
