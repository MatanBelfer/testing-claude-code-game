using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRig : MonoBehaviour
{
    public Camera Cam { get; private set; }

    void Awake()
    {
        Cam = GetComponentInChildren<Camera>();
        if (Cam != null)
            Cam.orthographicSize = GameConfig.DefaultZoom;
    }

    void Update()
    {
        HandleKeyboardPan();
    }

    void HandleKeyboardPan()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f) return;

        Vector3 flatUp = Cam != null ? Cam.transform.up : transform.forward;
        flatUp.y = 0; flatUp.Normalize();
        Vector3 flatRight = Cam != null ? Cam.transform.right : transform.right;
        flatRight.y = 0; flatRight.Normalize();

        Vector3 move = (flatRight * h + flatUp * v) * GameConfig.KeyboardPanSpeed * Time.deltaTime;
        Pan(move);
    }

    public void Pan(Vector3 worldDelta)
    {
        Vector3 newPos = transform.position + worldDelta;
        newPos.x = Mathf.Clamp(newPos.x, -GameConfig.MapHalfWidth, GameConfig.MapHalfWidth);
        newPos.z = Mathf.Clamp(newPos.z, -GameConfig.MapHalfDepth, GameConfig.MapHalfDepth);
        transform.position = newPos;
    }

    public void Zoom(float delta)
    {
        if (Cam == null) return;
        Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - delta, GameConfig.MinZoom, GameConfig.MaxZoom);
    }
}
