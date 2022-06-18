using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FlatKit {
[CustomEditor(typeof(FlatKitReadme))]
public class ReadmeEditor : Editor {
    private static readonly GUID UnityPackageUrpGuid =
        new GUID("41e59f562b69648719f2424c438758f3");

    private static readonly GUID UnityPackageBuiltInGuid =
        new GUID("f4227764308e84f89a765fbf315e2945");

    private static readonly GUID
        UrpPipelineAssetGuid = new GUID("ecbd363870e07455ea237f5753668d30");

    private FlatKitReadme _readme;
    private bool _showingVersionMessage;
    private string _versionLatest;

    private void OnEnable() {
        _readme = serializedObject.targetObject as FlatKitReadme;
        if (_readme == null) {
            Debug.LogError("[Flat Kit] Readme error.");
            return;
        }

        _readme.Refresh();
        _showingVersionMessage = false;
        _versionLatest = null;
    }

    public override void OnInspectorGUI() {
        {
            EditorGUILayout.LabelField("Flat Kit", EditorStyles.boldLabel);
            DrawUILine(Color.gray, 1, 0);
            EditorGUILayout.LabelField($"Version {_readme.FlatKitVersion}", EditorStyles.miniLabel);
            EditorGUILayout.Separator();
        }

        if (GUILayout.Button("Documentation")) {
            OpenDocumentation();
        }

        {
            if (_showingVersionMessage) {
                EditorGUILayout.Space(20);

                if (_versionLatest == null) {
                    EditorGUILayout.HelpBox($"Checking the latest version...", MessageType.None);
                } else {
                    var local = Version.Parse(_readme.FlatKitVersion);
                    var remote = Version.Parse(_versionLatest);
                    if (local >= remote) {
                        EditorGUILayout.HelpBox($"You have the latest version! {_versionLatest}.",
                            MessageType.Info);
                    } else {
                        EditorGUILayout.HelpBox(
                            $"Update needed. " +
                            $"The latest version is {_versionLatest}, but you have {_readme.FlatKitVersion}.",
                            MessageType.Warning);

#if !UNITY_2020_3_OR_NEWER
                        EditorGUILayout.HelpBox(
                            $"Please update Unity to 2020.3 or newer to get the latest " +
                            $"version of Flat Kit.",
                            MessageType.Error);
#endif
                    }
                }
            }

            if (GUILayout.Button("Check for updates")) {
                _showingVersionMessage = true;
                _versionLatest = null;
                CheckVersion();
            }

            if (_showingVersionMessage) {
                EditorGUILayout.Space(20);
            }
        }

        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Open support ticket")) {
                OpenSupportTicket();
            }

            if (GUILayout.Button("Copy debug info")) {
                CopyDebugInfoToClipboard();
            }

            GUILayout.EndHorizontal();
        }

        {
            if (!_readme.FlatKitInstalled) {
                EditorGUILayout.Separator();
                DrawUILine(Color.yellow, 1, 0);

                EditorGUILayout.HelpBox(
                    "Before using Flat Kit you need to unpack it depending on your " +
                    "project's Render Pipeline.",
                    MessageType.Warning);

                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Unpack Flat Kit for", EditorStyles.label);
                if (GUILayout.Button("URP")) {
                    UnpackFlatKitUrp();
                    ConfigureUrp();
                }

                if (GUILayout.Button("Built-in RP")) {
                    UnpackFlatKitBuiltInRP();
                    ConfigureBuiltIn();
                }

                GUILayout.EndHorizontal();
                DrawUILine(Color.yellow, 1, 0);

                return;
            }
        }

        {
            if (!string.IsNullOrEmpty(_readme.PackageManagerError)) {
                EditorGUILayout.Separator();
                DrawUILine(Color.yellow, 1, 0);
                EditorGUILayout.HelpBox(
                    $"Package Manager error: {_readme.PackageManagerError}",
                    MessageType.Warning);
                DrawUILine(Color.yellow, 1, 0);
            }
        }

        {
            DrawUILine(Color.gray, 1, 20);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Configure project for", EditorStyles.label);

            if (GUILayout.Button("URP", EditorStyles.miniButtonLeft)) {
                ConfigureUrp();
            }

            if (GUILayout.Button("Built-in RP", EditorStyles.miniButtonLeft)) {
                ConfigureBuiltIn();
            }

            GUILayout.EndHorizontal();
        }

        {
            DrawUILine(Color.gray, 1, 20);
            EditorGUILayout.LabelField("Debug info", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"    Unity {_readme.UnityVersion}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"    URP installed: {_readme.UrpInstalled}, version {_readme.UrpVersionInstalled}",
                EditorStyles.miniLabel);
            EditorGUILayout.Separator();
        }
    }

    private void UnpackFlatKitUrp() {
        string path = AssetDatabase.GUIDToAssetPath(UnityPackageUrpGuid.ToString());
        if (path == null) {
            Debug.LogError("[Flat Kit] Could not find the URP package.");
        } else {
            AssetDatabase.ImportPackage(path, false);
        }
    }

    private void UnpackFlatKitBuiltInRP() {
        string path = AssetDatabase.GUIDToAssetPath(UnityPackageBuiltInGuid.ToString());
        if (path == null) {
            Debug.LogError("[Flat Kit] Could not find the Built-in RP package.");
        } else {
            AssetDatabase.ImportPackage(path, false);
        }
    }

    private void ConfigureUrp() {
        string path = AssetDatabase.GUIDToAssetPath(UrpPipelineAssetGuid.ToString());
        if (path == null) {
            Debug.LogError("[Flat Kit] Couldn't find the URP pipeline asset. " +
                           "Have you unpacked the URP package?");
            return;
        }

        var pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
        if (pipelineAsset == null) {
            Debug.LogError("[Flat Kit] Couldn't load the URP pipeline asset.");
            return;
        }

        Debug.Log("<b>[Flat Kit]</b> Set the render pipeline asset in the Graphics settings " +
                  "to the Flat Kit example.");
        GraphicsSettings.renderPipelineAsset = pipelineAsset;
        GraphicsSettings.defaultRenderPipeline = pipelineAsset;

        ChangePipelineAssetAllQualityLevels(pipelineAsset);
    }

    private void ConfigureBuiltIn() {
        GraphicsSettings.renderPipelineAsset = null;
        Debug.Log("<b>[Flat Kit]</b> Cleared the render pipeline asset in the " +
                  "Graphics settings.");

        ChangePipelineAssetAllQualityLevels(null);
    }

    private void ChangePipelineAssetAllQualityLevels(RenderPipelineAsset pipelineAsset) {
        var originalQualityLevel = QualitySettings.GetQualityLevel();

        var logString = "<b>[Flat Kit]</b> Set the render pipeline asset for the quality levels:";

        for (int i = 0; i < QualitySettings.names.Length; i++) {
            logString += $"\n\t{QualitySettings.names[i]}";
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipelineAsset;
        }

        Debug.Log(logString);

        QualitySettings.SetQualityLevel(originalQualityLevel, false);
    }

    private void CheckVersion() {
        NetworkManager.GetVersion(version => { _versionLatest = version; });
    }

    private void CopyDebugInfoToClipboard() {
        EditorGUIUtility.systemCopyBuffer =
            $"Flat Kit: {_readme.FlatKitVersion}, " +
            $"URP: {_readme.UrpVersionInstalled}, " +
            $"Unity: {_readme.UnityVersion}";
        Debug.Log("<b>Flat Kit</b> info copied to clipboard.");
    }

    private void OpenSupportTicket() {
        Application.OpenURL("https://github.com/Dustyroom/flat-kit-doc/issues/new/choose");
    }

    private void OpenDocumentation() {
        Application.OpenURL("https://flatkit.dustyroom.com");
    }

    private static void DrawUILine(Color color, int thickness = 2, int padding = 10) {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2f;
        r.x -= 2;
        EditorGUI.DrawRect(r, color);
    }
}
}