using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class InputRouter : MonoBehaviour
{
    enum Mode { Idle, Panning, Drawing }

    CameraRig rig;
    Camera cam;

    Mode mode = Mode.Idle;
    IronDome activeDome;
    List<Vector3> currentPath;
    Vector2 lastScreenPos;

    bool pinching;
    float lastPinchDistance;

    void Awake()
    {
        rig = GetComponent<CameraRig>();
        cam = rig.Cam;
    }

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        HandleScrollZoom();

        if (Touch.activeTouches.Count > 0)
            HandleTouch();
        else
            HandleMouse();
    }

    void HandleScrollZoom()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float scrollY = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scrollY) < 0.001f) return;

        // Wheel notches report ~±120 per step on some platforms and ~±1 on
        // others; trackpads stream small continuous values. Normalize the
        // large-step case so one notch is one zoom step everywhere.
        if (Mathf.Abs(scrollY) > 5f) scrollY /= 120f;
        rig.Zoom(scrollY * GameConfig.ScrollZoomStep);
    }

    bool RaycastPlane(Vector2 screenPos, float height, out Vector3 worldPoint)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, height, 0));
        if (plane.Raycast(ray, out float dist))
        {
            worldPoint = ray.GetPoint(dist);
            return true;
        }
        worldPoint = Vector3.zero;
        return false;
    }

    IronDome FindNearbyDome(Vector3 groundPoint)
    {
        IronDome closest = null;
        float best = GameConfig.DomePickRadius;
        foreach (var dome in GameManager.Instance.Domes)
        {
            float d = Vector3.Distance(new Vector3(groundPoint.x, 0, groundPoint.z), dome.transform.position);
            if (d <= best)
            {
                best = d;
                closest = dome;
            }
        }
        return closest;
    }

    void BeginDrag(Vector2 screenPos)
    {
        if (RaycastPlane(screenPos, 0f, out Vector3 groundPoint))
        {
            var dome = FindNearbyDome(groundPoint);
            if (dome != null && dome.IsReady)
            {
                mode = Mode.Drawing;
                activeDome = dome;
                currentPath = new List<Vector3> { dome.transform.position + Vector3.up * GameConfig.InterceptorEngageHeight };
                activeDome.ShowPreview(currentPath);
                lastScreenPos = screenPos;
                return;
            }
        }

        mode = Mode.Panning;
        lastScreenPos = screenPos;
    }

    void UpdateDrag(Vector2 screenPos)
    {
        if (mode == Mode.Drawing)
        {
            if (RaycastPlane(screenPos, GameConfig.InterceptorEngageHeight, out Vector3 point))
            {
                if (Vector3.Distance(point, currentPath[currentPath.Count - 1]) >= GameConfig.MinPathPointDistance)
                {
                    currentPath.Add(point);
                    activeDome.ShowPreview(currentPath);
                }
            }
        }
        else if (mode == Mode.Panning)
        {
            Vector2 screenDelta = screenPos - lastScreenPos;
            float sizeFactor = rig.Cam.orthographicSize / Screen.height;
            Vector3 right = cam.transform.right; right.y = 0; right.Normalize();
            Vector3 up = cam.transform.up; up.y = 0; up.Normalize();
            Vector3 worldDelta = (-right * screenDelta.x - up * screenDelta.y) * sizeFactor * 2f;
            rig.Pan(worldDelta);
        }
        lastScreenPos = screenPos;
    }

    void EndDrag()
    {
        if (mode == Mode.Drawing && activeDome != null)
        {
            activeDome.Fire(currentPath);
            activeDome.ClearPreview();
        }
        mode = Mode.Idle;
        activeDome = null;
        currentPath = null;
    }

    void HandleMouse()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();
        if (mouse.leftButton.wasPressedThisFrame) BeginDrag(pos);
        else if (mouse.leftButton.isPressed && mode != Mode.Idle) UpdateDrag(pos);
        else if (mouse.leftButton.wasReleasedThisFrame) EndDrag();
    }

    void HandleTouch()
    {
        var touches = Touch.activeTouches;

        if (touches.Count == 1)
        {
            pinching = false;
            Touch touch = touches[0];
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    BeginDrag(touch.screenPosition);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (mode != Mode.Idle) UpdateDrag(touch.screenPosition);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndDrag();
                    break;
            }
        }
        else if (touches.Count >= 2)
        {
            if (mode == Mode.Drawing) activeDome.ClearPreview();
            mode = Mode.Idle;

            float distance = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
            if (!pinching)
            {
                pinching = true;
                lastPinchDistance = distance;
            }
            else
            {
                float delta = distance - lastPinchDistance;
                rig.Zoom(delta * GameConfig.PinchZoomSpeed);
                lastPinchDistance = distance;
            }
        }
        else
        {
            pinching = false;
        }
    }
}
