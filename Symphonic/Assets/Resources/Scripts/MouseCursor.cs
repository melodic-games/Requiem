using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{


    private void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Cursor.visible = false;
        transform.position = Input.mousePosition;

    }
}
