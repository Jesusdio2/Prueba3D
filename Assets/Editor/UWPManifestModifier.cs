using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class UWPManifestModifier : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WSAPlayer)
            return;

        string manifestPath = Path.Combine(report.summary.outputPath, "Package.appxmanifest");
        if (File.Exists(manifestPath))
        {
            string xml = File.ReadAllText(manifestPath);
            xml = xml.Replace("Assets\\UWPAssets\\FAES-Logo-BlackWhite.png", "Assets\\UWPAssets\\FAES-Logo-BlackWhite.png"); // If there is no featured image, the splash image is used.
            xml = xml.Replace("<Capabilities>", "<Capabilities>\n    <Capability Name=\"internetClient\" />");
            File.WriteAllText(manifestPath, xml);
        }
    }
}
