using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SkyboxFromLocation : MonoBehaviour
{
    [Header("Hemisphere prefab with inverted normals and unlit material")]
    public GameObject hemispherePrefab; // Assign a prefab of a hemisphere mesh with normals facing inward

    // Example: AstronomyAPI or Stellarium Web API endpoint
    private string apiUrlTemplate = "https://api.astronomyapi.com/api/v2/studio/star-chart?latitude={0}&longitude={1}&style=default&orientation=landscape&view=full&constellations=true";

    // Replace with your AstronomyAPI credentials if needed
    private string apiKey = "9fbf89c5-2838-499c-a0c5-b51d6d44ee0e";
    private string apiSecret = "0153adb3cffdb1ef6eb6d6a77a8072c5c06aa4c6aede770567ba9684c71de0ae7d1eaa2ecd5d0d897ef7f2d58313b606f4e5cedd37121c62cf350a7d848a27f1eda6114bfd3fa7715f1cace43a628aba5b6d6db0091ba713e9c05231f453ab6e0b6b57278cfba91bab69b9776a81e4f9";

    void Start()
    {
        StartCoroutine(SetSkyHemisphereFromLocation());
    }

    IEnumerator SetSkyHemisphereFromLocation()
    {
        // 1. Get location
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location service not enabled by user.");
            yield break;
        }
        Input.location.Start();
        int maxWait = 10;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("Unable to determine device location.");
            yield break;
        }
        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;

        // 2. Fetch sky image from API
        string url = string.Format(apiUrlTemplate, latitude, longitude);

        UnityWebRequest req = UnityWebRequest.Get(url);
        // AstronomyAPI requires HTTP Basic Auth
        string auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(apiKey + ":" + apiSecret));
        req.SetRequestHeader("Authorization", "Basic " + auth);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch sky image: " + req.error);
            yield break;
        }

        // AstronomyAPI returns JSON with an image URL, so parse it
        string json = req.downloadHandler.text;
        string imageUrl = ExtractImageUrlFromJson(json);
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("Could not extract sky image URL from API response.");
            yield break;
        }

        // 3. Download the image
        UnityWebRequest imgReq = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return imgReq.SendWebRequest();
        if (imgReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download sky image: " + imgReq.error);
            yield break;
        }
        Texture2D skyTex = DownloadHandlerTexture.GetContent(imgReq);

        // 4. Instantiate hemisphere and apply texture
        if (hemispherePrefab != null)
        {
            GameObject hemi = Instantiate(hemispherePrefab, Camera.main.transform.position, Quaternion.identity);
            hemi.transform.SetParent(Camera.main.transform); // Follow the camera
            hemi.transform.localPosition = new Vector3(0, 0, 0); // Centered on camera
            hemi.transform.localScale = Vector3.one * 10f; // Adjust size as needed

            Renderer rend = hemi.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.mainTexture = skyTex;
                rend.material.SetTexture("_MainTex", skyTex);
                rend.material.SetFloat("_Mode", 2); // Transparent if using Standard shader
                Color c = rend.material.color;
                c.a = 0.8f; // Semi-transparent
                rend.material.color = c;
            }
            Debug.Log("Sky hemisphere updated with real sky image.");
        }
        else
        {
            Debug.LogWarning("Hemisphere prefab not assigned.");
        }
    }

    // Helper to extract image URL from AstronomyAPI JSON response
    private string ExtractImageUrlFromJson(string json)
    {
        // AstronomyAPI returns: { "data": { "imageUrl": "https://..." } }
        int idx = json.IndexOf("\"imageUrl\"");
        if (idx == -1) return null;
        int start = json.IndexOf("http", idx);
        int end = json.IndexOf("\"", start);
        if (start == -1 || end == -1) return null;
        return json.Substring(start, end - start);
    }
}
