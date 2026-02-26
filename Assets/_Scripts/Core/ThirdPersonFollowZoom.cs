using UnityEngine;
using Unity.Cinemachine;

public class ThirdPersonFollowZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cmCamera;//ссылка на камеру Cinemachine в сцене, которую нужно настроить в инспекторе
    [SerializeField] private float minCameraDistance = 2f;//минимальное расстояние камеры от цели
    [SerializeField] private float maxCameraDistance = 10f;//максимальное расстояние камеры от цели
    [SerializeField] private float zoomSpeed = 2f;//скорость изменения расстояния камеры при прокрутке колеса мыши

    private CinemachineThirdPersonFollow follow;

    private void Awake()
    {
        if (cmCamera == null)
            cmCamera = GetComponent<CinemachineCamera>();

        if (cmCamera != null)
            follow = cmCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineThirdPersonFollow;
    }

    private void Update()
    {
        if (follow == null || InputManager.Instance == null)
            return;

        float scroll = InputManager.Instance.GetZoomInput();
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            follow.CameraDistance = Mathf.Clamp(
                follow.CameraDistance - scroll * zoomSpeed,
                minCameraDistance,
                maxCameraDistance
            );
        }
    }
}

