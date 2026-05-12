namespace QuietNoize.QuakeLikeLightFlicker.Demo
{
    using UnityEngine;

    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float m_moveSpeed = 3f;
        [SerializeField] private float m_rotationSpeed = 180f;

        void Update()
        {
            UpdateCursor();
            UpdatePosition();
            UpdateRotation();
        }

        private void UpdateCursor()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetMouseButtonUp(1))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void UpdatePosition()
        {
            Vector3 input = new Vector3(
                Input.GetAxis("Horizontal"),
                0f,
                Input.GetAxis("Vertical")
            );

            transform.position += transform.TransformDirection(input * m_moveSpeed * Time.deltaTime);
        }

        private void UpdateRotation()
        {
            if (!Input.GetMouseButton(1)) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            transform.Rotate(
                Vector3.up,
                mouseX * m_rotationSpeed * Time.deltaTime,
                Space.World
            );

            transform.Rotate(
                Vector3.right,
                -mouseY * m_rotationSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }
}
