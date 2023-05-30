using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //public float moveSpeed = 10f;
    //public float zoomSpeed = 5f;
    //private Vector3 _targetPosition;

    //private void Start()
    //{
    //    float halfHeight = (1 * _tileRenderer.transform.localScale.y) / 2.0f + padding;
    //    float halfWidth = (1 * _tileRenderer.transform.localScale.x) / 2.0f + padding;
    //    float size = Mathf.Max(halfHeight, halfWidth);
    //    GetComponent<Camera>().orthographicSize = size;
    //}
    [SerializeField]
    private float m_zoomSpeed = 1.5f;
    [SerializeField]
    private float m_panSpeed = 0.7f;

    private Vector3 m_lastMousePosition; // The last known position of the mouse, used for panning the camera

    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            m_lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            Vector3 delta = transform.InverseTransformDirection(Camera.main.ScreenToWorldPoint(m_lastMousePosition) - Camera.main.ScreenToWorldPoint(Input.mousePosition));
            transform.position += delta * m_panSpeed;
            m_lastMousePosition = Input.mousePosition;
        }

        float deltaZoom = Input.mouseScrollDelta.y * m_zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - deltaZoom, 1, 80);
    }
}
