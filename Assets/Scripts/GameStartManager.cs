using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameStartManager : MonoBehaviour
{
    public GameObject loadingUI; // Assign a loading UI panel in the Inspector (optional)
    public float retryDelay = 5f; // Seconds to wait before retrying location fetch

    private void Start()
    {
        StartCoroutine(WaitForLocationAndSkyboxThenEnableStart());
    }

    private IEnumerator WaitForLocationAndSkyboxThenEnableStart()
    {
        if (loadingUI != null)
            loadingUI.SetActive(true);

        Debug.Log("[GameStartManager] Waiting for location and skybox image to be ready...");
        int retryCount = 0;

#if UNITY_ANDROID || UNITY_IOS
        // On mobile, explicitly request location permission if not already granted
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("[GameStartManager] Location services are disabled. Prompting user to enable location.");
            // Optionally, show a UI prompt to the user here
#if UNITY_ANDROID
            // On Android, open location settings
            Application.OpenURL("android.settings.LOCATION_SOURCE_SETTINGS");
#endif
            // Wait and retry until enabled
            while (!Input.location.isEnabledByUser)
            {
                yield return new WaitForSeconds(1f);
            }
            Debug.Log("[GameStartManager] User enabled location services.");
        }
#endif

        while (true)
        {
            if (LocationData.Status == LocationData.FetchStatus.Success && LocationData.SkyTexture != null)
            {
                break;
            }
            if (LocationData.Status == LocationData.FetchStatus.Failed)
            {
                retryCount++;
                Debug.LogWarning($"[GameStartManager] Location fetch failed (attempt {retryCount}). Retrying in {retryDelay} seconds...");

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                if (retryCount == 1)
                {
                    Debug.LogWarning("[GameStartManager] If location repeatedly fails, ensure the app has Location Services permission in System Preferences > Security & Privacy > Location Services.");
                }
                ShowMacLocationErrorUI();
#endif

                LocationFetcher fetcher = FindObjectOfType<LocationFetcher>();
                if (fetcher != null)
                {
                    fetcher.StopAllCoroutines();
                    fetcher.StartCoroutine("FetchLocationAndTextureCoroutine");
                }
                else
                {
                    Debug.LogError("[GameStartManager] No LocationFetcher found in scene to retry location fetch.");
                }
                yield return new WaitForSeconds(retryDelay);
            }
            else
            {
                yield return null;
            }
        }
        Debug.Log("[GameStartManager] Location and skybox image ready. Game can be started.");

        if (loadingUI != null)
            loadingUI.SetActive(false);

        // Optionally, enable the Start button here if you want to gate it
        // Example: startButton.interactable = true;
    }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    // Optionally, show a UI popup or log for macOS location errors
    private void ShowMacLocationErrorUI()
    {
        Debug.LogWarning("[GameStartManager] macOS location error: If you see 'TUINSRemoteViewController' or location failures, try restarting the app and check System Preferences > Security & Privacy > Location Services. If the problem persists, reboot your Mac.");
        // You can also display a UI dialog here if desired.
    }
#endif

    public void StartGame()
    {
        // Only allow starting if ready
        if (LocationData.Status == LocationData.FetchStatus.Success && LocationData.SkyTexture != null)
        {
            SceneManager.LoadScene("Game");
        }
        else
        {
            Debug.LogWarning("[GameStartManager] Tried to start game before location/skybox ready.");
        }
    }
}
