using UnityEngine;

[AddComponentMenu("Camera-Control/Camera Script")]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float speed = 1.0f;
    private float rotationSpeed = 2.0f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update()
    {
        Vector3 newAxisValue = transform.position;
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && speed >= 0.0)
            speed -= 0.1f;
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            speed += 0.1f;

        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            if (Input.GetKey(KeyCode.W))
                newAxisValue += transform.forward * speed;

            if (Input.GetKey(KeyCode.S))
                newAxisValue -= transform.forward * speed;

            if (Input.GetKey(KeyCode.A))
                newAxisValue -= transform.right * speed;

            if (Input.GetKey(KeyCode.D))
                newAxisValue += transform.right * speed;

            if (Input.GetKey(KeyCode.Q))
                newAxisValue -= transform.up * speed;

            if (Input.GetKey(KeyCode.E))
                newAxisValue += transform.up * speed;

            yaw += rotationSpeed * Input.GetAxis("Mouse X");
            pitch -= rotationSpeed * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
        else
            Cursor.visible = true;
        transform.position = newAxisValue;
    }
}