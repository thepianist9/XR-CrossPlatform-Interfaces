#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class BuildApplication
{
    const string MENU = "XR-Interfaces/Build/";

    // Line
    // space by increment of 10 to add an empty line
    //[MenuItem("[ DEVA ]/--------------------------", false, 110)]
    //[MenuItem("[ DEVA ]/---------------------------", false, 210)]

    [MenuItem(MENU + "Reset Minor Version Number", false, 10)]
    public static void Reset_Minor()
    {
        string[] ver = PlayerSettings.bundleVersion.Split(".");
        newVer = $"{ver[0]}.{ver[1]}.0";

        PlayerSettings.bundleVersion = newVer;
    }

    [MenuItem(MENU + "Build Android APK #&c", false, 100)]
    public static void Build_Android_APK()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        Android_BuildNow();
    }
    [MenuItem(MENU + "Build Meta Quest 3 APK #&c", false, 100)]
    public static void Build_Meta_APK()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        MetaQuest_BuildNow();
    }
    [MenuItem(MENU + "Build Android APK #&c", false, 100)]
    public static void Build_Windows()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        //Android_BuildNow();
    }


    [MenuItem(MENU + "Build iOS XCode", false, 200)]
    public static void Build_iOS_XCODE()
    {
        EditorUserBuildSettings.buildAppBundle = true;
        iOS_BuildNow();
    }

    static string baseName = "DEVA";
    static string newVer;
    static int newCode;
    static string[] levels;

    private static void Init(bool incVersion)
    {
        string[] ver = PlayerSettings.bundleVersion.Split(".");
        if (incVersion) newVer = $"{ver[0]}.{ver[1]}.{int.Parse(ver[2]) + 1}";
        else newVer = $"{ver[0]}.{ver[1]}.{int.Parse(ver[2])}";

        //newCode = PlayerSettings.VersionCode ??

        string scenes = "Assets/Thesis/Scenes/XR-Interfaces.scene";
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.Vulkan });
        //PlayerSettings.productName = "";
        PlayerSettings.bundleVersion = newVer;
    }

    public static void Android_BuildNow()
    {
        string type = "";

        //
        // Remember last selected scene object
        //

        if (EditorUserBuildSettings.buildAppBundle)
        {
            Init(false);
            //
            // Increment the bundle nr. for Play Store
            //
            PlayerSettings.Android.bundleVersionCode++;
            type = "aab";
        }
        else
        {
            Init(true);
            type = "apk";
        }

        int release = PlayerSettings.Android.bundleVersionCode;

        //PlayerSettings.SetIl2CppCodeGeneration

        //
        // Get filename/folder
        //
        string path = EditorUtility.SaveFilePanel("Choose Location for Android Built v" + newVer + " R" + release + " Type: " + type,
            "../XR-Interfaces_Unity", baseName + "_build", type);

        if (!string.IsNullOrEmpty(path))
        {
            //
            // Build player
            //
            string finalPath = Path.Combine(Path.GetDirectoryName(path), baseName + "." + type);

            //1. set the graphics API to Vulkan
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3 });

            //2. Set render pipeline asset
            string MetaSRPpath = "Assets/Thesis/ScriptableRenderPipeline/AndroidMobileSRP.asset";

            RenderPipelineAsset pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MetaSRPpath);
            if (pipelineAsset != null)
            {
                GraphicsSettings.renderPipelineAsset = pipelineAsset;
                Debug.Log($"Render Pipeline Asset set from path: {path}");
            }
            else
            {
                Debug.LogError($"Pipeline Asset not found at path: {path}");
            }



            BuildPlayerOptions opt = new BuildPlayerOptions();
            opt.locationPathName = path;
            opt.target = BuildTarget.Android;
            opt.scenes = levels;
            opt.options = BuildOptions.None;

            BuildPipeline.BuildPlayer(opt);

            if (File.Exists(path)) FileUtil.MoveFileOrDirectory(path, finalPath);

            //string pathForSylR = "Dieser PC\\SylR\\Interner Speicher\\Download";
            //if (Directory.Exists(pathForSylR)) FileUtil.CopyFileOrDirectory(finalPath, Path.Combine(pathForSylR, baseName + "." + mode));
        }
    }
    public static void MetaQuest_BuildNow()
    {
        string type = "";

        //
        // Remember last selected scene object
        //

        if (EditorUserBuildSettings.buildAppBundle)
        {
            Init(false);
            //
            // Increment the bundle nr. for Play Store
            //
            PlayerSettings.Android.bundleVersionCode++;
            type = "aab";
        }
        else
        {
            Init(true);
            type = "apk";
        }

        int release = PlayerSettings.Android.bundleVersionCode;

        //PlayerSettings.SetIl2CppCodeGeneration

        //
        // Get filename/folder
        //
        string path = EditorUtility.SaveFilePanel("Choose Location for Android Built v" + newVer + " R" + release + " Type: " + type,
            "../XR-Interfaces_Unity-EXE", baseName + "_build", type);

        if (!string.IsNullOrEmpty(path))
        {
            //
            // Build player
            //
            string finalPath = Path.Combine(Path.GetDirectoryName(path), baseName + "." + type);

            //1. set the graphics API to Vulkan
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.Vulkan });

            //2. Set render pipeline asset
            string MetaSRPpath = "Assets/Thesis/ScriptableRenderPipeline/MetaSRP.asset";

            RenderPipelineAsset pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MetaSRPpath);
            if (pipelineAsset != null)
            {
                GraphicsSettings.renderPipelineAsset = pipelineAsset;
                Debug.Log($"Render Pipeline Asset set from path: {path}");
            }
            else
            {
                Debug.LogError($"Pipeline Asset not found at path: {path}");
            }



            BuildPlayerOptions opt = new BuildPlayerOptions();
            opt.locationPathName = path;
            opt.target = BuildTarget.Android;
            opt.scenes = levels;
            opt.options = BuildOptions.None;

            BuildPipeline.BuildPlayer(opt);

            if (File.Exists(path)) FileUtil.MoveFileOrDirectory(path, finalPath);

            //string pathForSylR = "Dieser PC\\SylR\\Interner Speicher\\Download";
            //if (Directory.Exists(pathForSylR)) FileUtil.CopyFileOrDirectory(finalPath, Path.Combine(pathForSylR, baseName + "." + mode));
        }
    }

    public static void iOS_BuildNow(bool incVersion = true)
    {
        Init(incVersion);

        // Get folder name.
        string path = EditorUtility.SaveFolderPanel("Choose Location for iOS Built v" + newVer + " Mode: IOS",
            "../IMC_CompAir_Unity-EXE", baseName);

        if (!string.IsNullOrEmpty(path))
        {
            //
            // Build player
            //
            BuildPlayerOptions opt = new BuildPlayerOptions();
            opt.locationPathName = path;
            opt.target = BuildTarget.iOS;
            opt.scenes = levels;
            opt.options = BuildOptions.None;

            BuildPipeline.BuildPlayer(opt);
        }
    }


}

#endif