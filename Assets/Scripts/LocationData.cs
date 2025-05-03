using UnityEngine;

public static class LocationData
{
    public enum FetchStatus { Idle, Fetching, Success, Failed }

    public static float Latitude { get; private set; }
    public static float Longitude { get; private set; }
    public static Texture2D SkyTexture { get; private set; } // Added texture property
    public static FetchStatus Status { get; private set; } = FetchStatus.Idle;
    public static string ErrorMessage { get; private set; }

    public static void SetFetching()
    {
        Status = FetchStatus.Fetching;
        ErrorMessage = null;
        SkyTexture = null; // Clear texture when starting fetch
        Debug.Log("[LocationData] Status set to Fetching.");
    }

    // Modified SetSuccess to include texture
    public static void SetSuccess(float lat, float lon, Texture2D texture)
    {
        Latitude = lat;
        Longitude = lon;
        SkyTexture = texture; // Store texture
        Status = FetchStatus.Success;
        ErrorMessage = null;
        Debug.Log($"[LocationData] Status set to Success. Lat: {Latitude}, Lon: {Longitude}, Texture: {SkyTexture?.name}");
    }

    public static void SetFailed(string error)
    {
        Status = FetchStatus.Failed;
        ErrorMessage = error;
        SkyTexture = null; // Clear texture on failure
        Debug.LogError($"[LocationData] Status set to Failed: {ErrorMessage}");
    }

    public static void Reset()
    {
        Status = FetchStatus.Idle;
        ErrorMessage = null;
        Latitude = 0f;
        Longitude = 0f;
        SkyTexture = null; // Clear texture on reset
        Debug.Log("[LocationData] Reset.");
    }
}
