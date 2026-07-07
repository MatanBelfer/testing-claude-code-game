using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        BuildGround();
        BuildLighting();

        var gm = gameObject.AddComponent<GameManager>();

        foreach (var pos in GameConfig.CityPositions)
        {
            var cityGo = new GameObject("City");
            cityGo.transform.position = pos;
            var city = cityGo.AddComponent<City>();
            gm.RegisterCity(city);
        }

        foreach (var pos in GameConfig.DomePositions)
        {
            var domeGo = new GameObject("IronDome");
            domeGo.transform.position = pos;
            var dome = domeGo.AddComponent<IronDome>();
            gm.RegisterDome(dome);
        }

        SetupCameraRig();

        gameObject.AddComponent<HUDController>();
    }

    void BuildGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(
            GameConfig.MapHalfWidth * 2f / 10f, 1f, GameConfig.MapHalfDepth * 2f / 10f);

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.65f, 0.6f, 0.45f);
        ground.GetComponent<MeshRenderer>().material = mat;
    }

    void BuildLighting()
    {
        RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);
    }

    void SetupCameraRig()
    {
        var mainCam = Camera.main;
        if (mainCam == null) return;

        var rigGo = new GameObject("CameraRig");
        rigGo.transform.position = Vector3.zero;

        Transform camTransform = mainCam.transform;
        camTransform.SetParent(rigGo.transform, false);
        camTransform.localPosition = new Vector3(0f, 16f, -16f);
        camTransform.localRotation = Quaternion.Euler(35.264f, 45f, 0f);
        mainCam.orthographic = true;
        mainCam.orthographicSize = GameConfig.DefaultZoom;

        rigGo.AddComponent<CameraRig>();
        rigGo.AddComponent<InputRouter>();
    }
}
