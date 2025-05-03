using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkyboxFromLocation : MonoBehaviour
{
    [Header("Skybox Material (Unlit/Texture or similar)")]
    public Material skyboxMaterial; // Assign a skybox material in the Inspector

    [Header("UI RawImage for sky (on Canvas)")]
    public RawImage skyRawImage; // Assign a UI RawImage in the Canvas

    [Range(0f, 1f)]
    public float skyAlpha = 0.5f; // Set transparency

    void Start()
    {
        Debug.Log("[SkyboxFromLocation] Setting skybox material...");
        ApplySkybox();

        Debug.Log("[SkyboxFromLocation] Setting sky image on UI Canvas...");
        ApplySkyImageToCanvas();
    }

    void ApplySkybox()
    {
        Texture2D texToApply = null;
        string texName = "";

        if (LocationData.Status == LocationData.FetchStatus.Success && LocationData.SkyTexture != null)
        {
            texToApply = LocationData.SkyTexture;
            texName = texToApply.name;
            Debug.Log($"[SkyboxFromLocation] Using stored location: Lat={LocationData.Latitude}, Lon={LocationData.Longitude} and Texture: {texName}");
        }
        else
        {
            Debug.LogWarning($"[SkyboxFromLocation] Location/Texture data not available or fetch failed. Status: {LocationData.Status}. Error: {LocationData.ErrorMessage}");
            // Fallback: create a black texture for testing
            texToApply = new Texture2D(2, 2, TextureFormat.RGB24, false);
            Color32[] pixels = new Color32[4] { Color.black, Color.black, Color.black, Color.black };
            texToApply.SetPixels32(pixels);
            texToApply.Apply();
            texName = "FallbackBlackSkybox";
        }

        if (skyboxMaterial != null)
        {
            // This will update the material's main texture at runtime
            skyboxMaterial.mainTexture = texToApply;
            RenderSettings.skybox = skyboxMaterial;
            Debug.Log($"[SkyboxFromLocation] Skybox material set with texture '{texName}'.");
        }
        else
        {
            Debug.LogWarning("[SkyboxFromLocation] No skybox material assigned.");
        }
    }

    void ApplySkyImageToCanvas()
    {
        Texture2D texToApply = null;
        string texName = "";

        if (LocationData.Status == LocationData.FetchStatus.Success && LocationData.SkyTexture != null)
        {
            texToApply = LocationData.SkyTexture;
            texName = texToApply.name;
            Debug.Log($"[SkyboxFromLocation] Using stored location: Lat={LocationData.Latitude}, Lon={LocationData.Longitude} and Texture: {texName}");
        }
        else
        {
            Debug.LogWarning($"[SkyboxFromLocation] Location/Texture data not available or fetch failed. Status: {LocationData.Status}. Error: {LocationData.ErrorMessage}");
            // Fallback: create a black texture for testing
            texToApply = new Texture2D(2, 2, TextureFormat.RGB24, false);
            Color32[] pixels = new Color32[4] { Color.black, Color.black, Color.black, Color.black };
            texToApply.SetPixels32(pixels);
            texToApply.Apply();
            texName = "FallbackBlackSkybox";
        }

        if (skyRawImage != null)
        {
            skyRawImage.texture = texToApply;
            Color c = skyRawImage.color;
            c.a = skyAlpha;
            skyRawImage.color = c;

            // --- Make the RawImage fill the screen ---
            RectTransform rt = skyRawImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Debug.Log($"[SkyboxFromLocation] Sky RawImage set with texture '{texName}' and alpha {skyAlpha}, stretched to fill screen.");
        }
        else
        {
            Debug.LogWarning("[SkyboxFromLocation] No RawImage assigned on Canvas.");
        }
    }
}
