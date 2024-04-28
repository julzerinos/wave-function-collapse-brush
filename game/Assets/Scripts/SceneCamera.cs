using System;
using UnityEngine;

public class SceneCamera : MonoBehaviour
{
    private Vector3 _previousMousePosition;

    private void Awake()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        var mousePosition = Input.mousePosition;

        if (!Input.GetMouseButton(1))
        {
            _previousMousePosition = mousePosition;
            return;
        }

        var delta = mousePosition - _previousMousePosition;

        var position = transform.position;
        transform.position = Vector3.Lerp(position, position + -0.25f * new Vector3(delta.x, 0, delta.y), .3f);
        _previousMousePosition = mousePosition;
    }
}