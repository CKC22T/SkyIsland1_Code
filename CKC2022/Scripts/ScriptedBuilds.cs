#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

class ScriptedBuilds
{
    // Invoked via command line only
    static void PerformHeadlessWindowsBuild()
    {
        // As a fallback use <project root>/BUILD as output path
        var buildPath = Path.Combine(Application.dataPath, "BUILD");

        // read in command line arguments e.g. add "-buildPath some/Path" if you want a different output path 
        var args = Environment.GetCommandLineArgs();

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildPath")
            {
                buildPath = args[i + 1];
            }
        }

        // if the output folder doesn't exist create it now
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        List<string> scenes = new List<string>();

        foreach(var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            scenes.Add(scene.path);
        }

        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,

            // pass on the output folder
            buildPath,

            // Build for windows 64 bit
            BuildTarget.StandaloneWindows64,
            BuildOptions.EnableHeadlessMode | BuildOptions.StrictMode
        );

     //   BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
     //   buildPlayerOptions.scenes = scenes.ToArray();
     //   buildPlayerOptions.locationPathName = buildPath;
     //   buildPlayerOptions.target = BuildTarget.StandaloneWindows;
     //   buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
     //   buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;
     //   buildPlayerOptions.options = BuildOptions.StrictMode;
     //
     //   BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
#endif