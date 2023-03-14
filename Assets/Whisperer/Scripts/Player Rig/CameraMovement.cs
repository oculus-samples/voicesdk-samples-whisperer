using UnityEngine;

namespace Whisperer
{
    public class CameraMovement : MonoBehaviour
    {

        public float sensitivity = 5.0f;

        private Vector3 _lastMousePosition;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                transform.Rotate(Vector3.up * (delta.x * sensitivity), Space.World);
                transform.Rotate(Vector3.right * (-delta.y * sensitivity), Space.Self);
                _lastMousePosition = Input.mousePosition;
            }
        }
    }
}
