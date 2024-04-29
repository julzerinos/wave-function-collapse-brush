using System;
using UnityEngine;

public class SceneCamera : MonoBehaviour
{
    private Vector3 _previousMousePosition;
    private Camera _camera;

    private void Awake()
    {
        Cursor.visible = false;
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        var mousePosition = Input.mousePosition;

        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - Input.mouseScrollDelta.y, 5, 30);

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