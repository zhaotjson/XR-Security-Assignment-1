#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostBuildProcessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Get the Info.plist path
            string plistPath = Path.Combine(buildPath, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Get root dictionary
            PlistElementDict rootDict = plist.root;

            // Set the location usage description
            string locationUsageDescription = "This app needs your location to display the correct sky view.";
            rootDict.SetString("NSLocationWhenInUseUsageDescription", locationUsageDescription);
            // You might also need NSLocationAlwaysUsageDescription depending on your needs
            // rootDict.SetString("NSLocationAlwaysUsageDescription", locationUsageDescription);

            // Write changes back to the Info.plist file
            plist.WriteToFile(plistPath);

            UnityEngine.Debug.Log("PostBuildProcessor: Added NSLocationWhenInUseUsageDescription to Info.plist");
        }
    }
}
#endif
