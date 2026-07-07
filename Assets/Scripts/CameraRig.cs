using UnityEngine;
using UnityEngine.InputSystem;

// Orbit rig: the map is fixed in place; this rig sits at the map center and
// yaws around it, while the camera hangs off it at an adjustable pitch.
// Zoom is orthographic size. There is no panning.
public class CameraRig : MonoBehaviour
{
    public Camera Cam { get; private set; }

    const float CameraDistance = 80f;

    float yaw = 45f;
    float pitch;

    void Awake()
    {
        Cam = GetComponentInChildren<Camera>();
        pitch = GameConfig.DefaultPitch;
        if (Cam != null)
            Cam.orthographicSize = GameConfig.DefaultZoom;
        Apply();
    }

    void Update()
    {
        HandleKeyboard();
    }

    void HandleKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float rot = (kb.eKey.isPressed || kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                  - (kb.qKey.isPressed || kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        if (rot != 0f)
            Rotate(rot * GameConfig.KeyboardRotateSpeed * Time.deltaTime);

        float tilt = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                   - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
        if (tilt != 0f)
            Tilt(tilt * GameConfig.KeyboardTiltSpeed * Time.deltaTime);
    }

    public void Rotate(float degrees)
    {
        yaw += degrees;
        Apply();
    }

    public void Tilt(float degrees)
    {
        pitch = Mathf.Clamp(pitch + degrees, GameConfig.MinPitch, GameConfig.MaxPitch);
        Apply();
    }

    void Apply()
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (Cam == null) return;

        Quaternion camRot = Quaternion.Euler(pitch, 0f, 0f);
        Cam.transform.localRotation = camRot;
        Cam.transform.localPosition = -(camRot * Vector3.forward) * CameraDistance;
    }

    public void Zoom(float delta)
    {
        if (Cam == null) return;
        Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - delta, GameConfig.MinZoom, GameConfig.MaxZoom);
    }
}
