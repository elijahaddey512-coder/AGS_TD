using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    public float distance = 6f;
    public float height = 2f;
    public float sensitivity = 3f;
    public float smoothSpeed = 10f;

    private float mouseX;
    private float mouseY;

    
    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        mouseX = angles.y;
        mouseY = angles.x;

    }
    
    void LateUpdate()
    {
        if (player == null) return;

        switch (Input.GetMouseButton(1))
        {
            case true:
                mouseX += Input.GetAxis("Mouse X") * sensitivity;
                mouseY -= Input.GetAxis("Mouse Y") * sensitivity;

                mouseY = Mathf.Clamp(mouseY, -20, 30f);
                break;
            case false:
            break;
        }

        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0f);

        Vector3 targetPosition = player.position + Vector3.up * height;

        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(targetPosition);
    }
}
