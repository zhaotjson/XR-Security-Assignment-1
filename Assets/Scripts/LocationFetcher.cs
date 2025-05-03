using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class LocationFetcher : MonoBehaviour
{
    // AstronomyAPI endpoint for sky images
    private string apiUrlTemplate = "https://api.astronomyapi.com/api/v2/studio/star-chart?latitude={0}&longitude={1}&style=default&orientation=landscape&view=full&constellations=true";
    // Use your AstronomyAPI Application ID and Application Secret (not AWS keys)
    // The dashes **are** part of the Application ID and Secret as shown in your AstronomyAPI dashboard.
    // Example: "88b9f45c-6215-4100-a5c2-c2f2e2e9df67"
    private string applicationId = "88b9f45c-6215-4100-a5c2-c2f2e2e9df67";
    private string applicationSecret = "0153adb3cffdb1ef6eb6d6a77a8072c5c06aa4c6aede770567ba9684c71de0ae7d1eaa2ecd5d0d897ef7f2d58313b606f4e5cedd37121c62cf350a7d848a27f1e30226ddb896dbcc17fe46c176e8e3ac0a9ae06f1722426bd4a0aec23a14e06a1eb9fc13b152c74fd6b2409801469e3a";

    void Start()
    {
        if (LocationData.Status == LocationData.FetchStatus.Idle || LocationData.Status == LocationData.FetchStatus.Failed)
        {
            Debug.Log("[LocationFetcher] Starting location fetch process.");
            StartCoroutine(FetchLocationAndTextureCoroutine());
        }
        else
        {
            Debug.Log($"[LocationFetcher] Location status is already {LocationData.Status}. Skipping fetch.");
        }
    }

    IEnumerator FetchLocationAndTextureCoroutine()
    {
        LocationData.SetFetching();

        // 1. Get Location
        if (!Input.location.isEnabledByUser)
        {
            LocationData.SetFailed("Location service not enabled by user.");
            yield break;
        }
        Input.location.Start(10f, 10f);
        Debug.Log("[LocationFetcher] Input.location.Start(10f, 10f) called.");
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log($"[LocationFetcher] Waiting for location... {maxWait}s left");
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }
        Debug.Log($"[LocationFetcher] Finished waiting for location. Final Status: {Input.location.status}");

        float latitude = 0f;
        float longitude = 0f;

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            LocationData.SetFailed("Location service failed to start.");
            Input.location.Stop();
            yield break;
        }
        else if (Input.location.status == LocationServiceStatus.Running)
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            Debug.Log($"[LocationFetcher] Location obtained: Lat={latitude}, Lon={longitude}");
        }
        else
        {
            LocationData.SetFailed($"Unable to determine device location. Status: {Input.location.status}");
            Input.location.Stop();
            yield break;
        }
        Input.location.Stop();
        Debug.Log("[LocationFetcher] Location service stopped.");

        // 2. Check Network
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            LocationData.SetFailed("Network not reachable. Cannot fetch sky image.");
            yield break;
        }
        Debug.Log($"[LocationFetcher] Network is reachable ({Application.internetReachability}). Proceeding with API call.");

        // 3. Fetch Sky Image from AstronomyAPI
        // --- AstronomyAPI expects observer parameters and correct endpoint ---
        // For /studio/star-chart, the API expects a POST request with a JSON body.
        // The observer object REQUIRES a "date" property.
        // The "view.type" must be "area" or "constellation" (not "landscape").
        // See: https://docs.astronomyapi.com/docs/api-reference/studio/star-chart

        string url = "https://api.astronomyapi.com/api/v2/studio/star-chart";
        float elevation = 0f;
        DateTime now = DateTime.UtcNow;
        string date = now.ToString("yyyy-MM-dd");
        string time = now.ToString("HH:mm:ss");

        // Build JSON body as per docs (fix: add observer.date, use view.type = "area")
        string jsonBody = $@"
        {{
            ""observer"": {{
                ""latitude"": {latitude},
                ""longitude"": {longitude},
                ""elevation"": {elevation},
                ""date"": ""{date}""
            }},
            ""view"": {{
                ""type"": ""area"",
                ""parameters"": {{
                    ""position"": {{
                        ""equatorial"": {{
                            ""rightAscension"": 0,
                            ""declination"": 0
                        }}
                    }},
                    ""zoom"": 1
                }}
            }},
            ""style"": ""default"",
            ""constellations"": true
        }}";

        Debug.Log($"[LocationFetcher] AstronomyAPI POST body: {jsonBody}");

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        // AstronomyAPI Basic Auth
        string rawAuth = $"{applicationId}:{applicationSecret}";
        string authString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rawAuth));
        req.SetRequestHeader("Authorization", "Basic " + authString);

        Debug.Log($"[LocationFetcher] Authorization header: Basic {authString}");

        req.timeout = 30;
        yield return req.SendWebRequest();
        Debug.Log($"[LocationFetcher] AstronomyAPI request finished. Result: {req.result}");
        Debug.Log($"[LocationFetcher] API Response Body: {req.downloadHandler?.text}");

        string imageUrl = null;
        if (req.result != UnityWebRequest.Result.Success)
        {
            // Yes, there was an error if you see this log and Status is "Fetching" or "Failed".
            // Check the previous log: [LocationFetcher] API Response Body: ... for the error details.
            LocationData.SetFailed($"Failed to fetch sky image API: {req.error} (Code: {req.responseCode})");
            Debug.LogError($"[LocationFetcher] API Response Body: {req.downloadHandler?.text}");
            req.Dispose();
            yield break;
        }
        else
        {
            // No error, request succeeded.
            string json = req.downloadHandler.text;
            Debug.Log($"[LocationFetcher] API JSON Response: {json}");
            imageUrl = ExtractImageUrlFromJson(json);
            req.Dispose();

            if (string.IsNullOrEmpty(imageUrl))
            {
                LocationData.SetFailed("Could not extract sky image URL from AstronomyAPI response.");
                yield break;
            }
            Debug.Log($"[LocationFetcher] Extracted Image URL: {imageUrl}");
        }

        // 4. Download the Image Texture
        Debug.Log("[LocationFetcher] Requesting sky image download...");
        UnityWebRequest imgReq = UnityWebRequestTexture.GetTexture(imageUrl);
        imgReq.timeout = 60;
        yield return imgReq.SendWebRequest();
        Debug.Log($"[LocationFetcher] Image download finished. Result: {imgReq.result}");

        if (imgReq.result != UnityWebRequest.Result.Success)
        {
            LocationData.SetFailed($"Failed to download sky image: {imgReq.error} (Code: {imgReq.responseCode})");
            imgReq.Dispose();
            yield break;
        }
        else
        {
            Texture2D skyTex = DownloadHandlerTexture.GetContent(imgReq);
            skyTex.name = "AstronomyAPISkyTexture";
            imgReq.Dispose();
            Debug.Log("[LocationFetcher] Image download successful.");

            // 5. Set Success with all data
            LocationData.SetSuccess(latitude, longitude, skyTex);
        }
    }

    private string ExtractImageUrlFromJson(string json)
    {
        int idx = json.IndexOf("\"imageUrl\"");
        if (idx == -1) return null;
        int start = json.IndexOf("http", idx);
        int end = json.IndexOf("\"", start);
        if (start == -1 || end == -1) return null;
        return json.Substring(start, end - start);
    }
}
