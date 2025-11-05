using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerAdjust : MonoBehaviour
{
    public CanvasScaler scaler;
    public Vector2 referenceResolutionMobile = new Vector2(1080, 1920);
    public Vector2 referenceResolutionPC = new Vector2(1280, 720);
    [Range(0f,1f)] public float matchForMobile = 1f; // обычно для портрета match по высоте
    [Range(0f,1f)] public float matchForPC = 0.5f;

    void Awake()
    {
        if (scaler == null) scaler = GetComponent<CanvasScaler>();
        ApplySettings();
    }

    public void ApplySettings()
    {
#if UNITY_ANDROID || UNITY_IOS
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolutionMobile;
        scaler.matchWidthOrHeight = matchForMobile;
#else
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolutionPC;
        scaler.matchWidthOrHeight = matchForPC;
#endif
    }

    // Позволяет переключать настройки в рантайме при необходимости
    public void SetMatch(float match)
    {
        if (scaler != null) scaler.matchWidthOrHeight = Mathf.Clamp01(match);
    }
}
