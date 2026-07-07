using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        HandleScrollZoom();

        if (Input.touchSupported && Input.touchCount > 0)
            HandleTouch();
        else
            HandleMouse();
    }

    void HandleScrollZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
            rig.Zoom(scroll * GameConfig.ZoomSpeed);
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
        if (Input.GetMouseButtonDown(0)) BeginDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0) && mode != Mode.Idle) UpdateDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) EndDrag();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            pinching = false;
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    BeginDrag(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (mode != Mode.Idle) UpdateDrag(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndDrag();
                    break;
            }
        }
        else if (Input.touchCount == 2)
        {
            if (mode == Mode.Drawing) activeDome.ClearPreview();
            mode = Mode.Idle;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float distance = Vector2.Distance(t0.position, t1.position);
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
