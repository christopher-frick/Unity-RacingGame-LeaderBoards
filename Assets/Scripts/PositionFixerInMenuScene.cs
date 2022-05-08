using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionFixerInMenuScene : MonoBehaviour
{
    private void LateUpdate()
    {
        var transform1 = transform;
        transform1.position = new Vector3(0.0f, transform1.position.y, 0.0f);
        
    }
}
