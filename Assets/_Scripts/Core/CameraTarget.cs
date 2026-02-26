using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 0.3f;
    [SerializeField] private float minVerticalAngle = -30f; // âíèç
    [SerializeField] private float maxVerticalAngle = 60f;  // ââåðõ

    [Header("State")]
    [SerializeField] private float currentYaw = 0f;    // Y
    [SerializeField] private float currentPitch = 20f; // X

    private void Awake()
    {
        // Áåð¸ì ñòàðòîâûé ïîâîðîò èç èíñïåêòîðà (åñëè âûñòàâëÿëè ðóêàìè)
        Vector3 euler = transform.localRotation.eulerAngles;
        currentYaw = euler.y;
        currentPitch = NormalizeAngle(euler.x);
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    private void Update()
    {
        if (InputManager.Instance == null)
            return;

        Vector2 lookInput = InputManager.Instance.GetLookInput();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY; // èíâåðñèÿ Y äëÿ “êàê â áîëüøèíñòâå 3rd-person”
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
    }

    public float GetMouseSensitivity() => mouseSensitivity;

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
