using UnityEngine;
using UnityEngine.InputSystem;

// Orbit rig: the map is fixed in place; this rig sits at the map center and
// yaws around it. Zoom is orthographic size. There is no panning.
public class CameraRig : MonoBehaviour
{
    public Camera Cam { get; private set; }

    float yaw = 45f;

    void Awake()
    {
        Cam = GetComponentInChildren<Camera>();
        if (Cam != null)
            Cam.orthographicSize = GameConfig.DefaultZoom;
        ApplyYaw();
    }

    void Update()
    {
        HandleKeyboardOrbit();
    }

    void HandleKeyboardOrbit()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float dir = (kb.eKey.isPressed || kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                  - (kb.qKey.isPressed || kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        if (dir != 0f)
            Rotate(dir * GameConfig.KeyboardRotateSpeed * Time.deltaTime);
    }

    public void Rotate(float degrees)
    {
        yaw += degrees;
        ApplyYaw();
    }

    void ApplyYaw()
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    public void Zoom(float delta)
    {
        if (Cam == null) return;
        Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - delta, GameConfig.MinZoom, GameConfig.MaxZoom);
    }
}
