using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoSingleton<CameraController>
{
    private Transform target;

    public void Init(Transform target)
    {
        this.target = target;
    }

    private void Update()
    {
        if (!target)
            return;

        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -100);
    }
}
