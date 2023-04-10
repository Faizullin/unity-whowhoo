using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public int Rows = 1;
    public int Cols = 1;
    public float padding = 1.0f;
    //public GameObject tilePrefab;
    [SerializeField] private SpriteRenderer _tileRenderer;

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
    public float zoomSpeed = 0.5f; // The speed at which the camera will zoom in and out
    public float panSpeed = 0.7f; // The speed at which the camera will pan across the map

    private Vector3 lastMousePosition; // The last known position of the mouse, used for panning the camera

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // If the user presses the left mouse button, record the current mouse position for use in panning the camera
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            // If the user is holding down the left mouse button, pan the camera across the map based on the difference between the current and last mouse positions
            Vector3 delta = transform.InverseTransformDirection(Camera.main.ScreenToWorldPoint(lastMousePosition) - Camera.main.ScreenToWorldPoint(Input.mousePosition));
            transform.position += delta * panSpeed;
            lastMousePosition = Input.mousePosition;
        }

        // Zoom the camera in and out using the mouse scroll wheel
        float deltaZoom = Input.mouseScrollDelta.y * zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - deltaZoom, 1, 10);
    }
}
