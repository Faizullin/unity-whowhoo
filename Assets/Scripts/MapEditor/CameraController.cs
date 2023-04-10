using UnityEngine;

namespace TileMapEditorScene
{
    public class CameraController : MonoBehaviour
    {
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
}