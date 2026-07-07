using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class InputRouter : MonoBehaviour
{
    enum Mode { Idle, Rotating, Drawing }

    CameraRig rig;
    Camera cam;

    Mode mode = Mode.Idle;
    IronDome activeDome;
    List<Vector3> currentPath;
    Vector2 lastScreenPos;

    bool pinching;
    float lastPinchDistance;
    Vector2 lastPinchCenter;
    Vector2 lastRightDragPos;

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
        rig.Zoom(scrollY * GameConfig.ScrollDollyStep);
    }

    // Paths are drawn on the ground plane (y = 0) so the line lands exactly
    // under the cursor/finger; the interceptor lifts the path to engage
    // height when it flies it.
    bool RaycastGround(Vector2 screenPos, out Vector3 worldPoint)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
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
        if (RaycastGround(screenPos, out Vector3 groundPoint))
        {
            var dome = FindNearbyDome(groundPoint);
            if (dome != null && dome.IsReady)
            {
                mode = Mode.Drawing;
                activeDome = dome;
                currentPath = new List<Vector3> { dome.transform.position };
                activeDome.ShowPreview(currentPath);
                lastScreenPos = screenPos;
                return;
            }
        }

        mode = Mode.Rotating;
        lastScreenPos = screenPos;
    }

    void UpdateDrag(Vector2 screenPos)
    {
        if (mode == Mode.Drawing)
        {
            if (RaycastGround(screenPos, out Vector3 point))
            {
                point.y = 0f;

                // Keep the path inside the dome's reach: points past the
                // range circle are pulled back onto its edge.
                Vector3 fromDome = point - activeDome.transform.position;
                fromDome.y = 0f;
                if (fromDome.magnitude > GameConfig.DomeRange)
                    point = activeDome.transform.position + fromDome.normalized * GameConfig.DomeRange;

                if (Vector3.Distance(point, currentPath[currentPath.Count - 1]) >= GameConfig.MinPathPointDistance)
                {
                    currentPath.Add(point);
                    activeDome.ShowPreview(currentPath);
                }
            }
        }
        else if (mode == Mode.Rotating)
        {
            float deltaX = screenPos.x - lastScreenPos.x;
            rig.Rotate(-deltaX * GameConfig.DragRotateSpeed);
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

        // Right-drag orbits freely: horizontal rotates, vertical tilts.
        if (mouse.rightButton.wasPressedThisFrame)
        {
            lastRightDragPos = pos;
        }
        else if (mouse.rightButton.isPressed && mode != Mode.Drawing)
        {
            Vector2 delta = pos - lastRightDragPos;
            rig.Rotate(-delta.x * GameConfig.DragRotateSpeed);
            rig.Tilt(delta.y * GameConfig.DragTiltSpeed);
            lastRightDragPos = pos;
        }
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

            // Two fingers: spread to zoom, move both up/down to tilt.
            float distance = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
            Vector2 center = (touches[0].screenPosition + touches[1].screenPosition) * 0.5f;
            if (!pinching)
            {
                pinching = true;
                lastPinchDistance = distance;
                lastPinchCenter = center;
            }
            else
            {
                rig.Zoom((distance - lastPinchDistance) * GameConfig.PinchDollySpeed);
                rig.Tilt((center.y - lastPinchCenter.y) * GameConfig.DragTiltSpeed);
                lastPinchDistance = distance;
                lastPinchCenter = center;
            }
        }
        else
        {
            pinching = false;
        }
    }
}
