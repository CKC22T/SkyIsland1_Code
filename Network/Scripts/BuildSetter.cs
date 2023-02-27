#if UNITY_EDITOR

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildSetter : EditorWindow
{
    [MenuItem("Build/Build Server")]
    public static void BuildServer()
    {
        string[] buildScenes = new string[]
        {
            "Assets/Network/Test/TestServerScene.unity",
            "Assets/Network/Test/PhysicsTest/PhysicsMapScene.unity",
        };

        var flags = BuildOptions.Development;

        string serverBuildPath = @"..\Build\ServerBuild\ServerBuild.exe";
        string currentDirectory = Directory.GetCurrentDirectory();

        string buildPath = Path.GetFullPath(serverBuildPath, currentDirectory);

        BuildPipeline.BuildPlayer(buildScenes, buildPath, BuildTarget.StandaloneWindows, flags);
    }

    [MenuItem("Build/Open Protobuf Directory")]
    public static void OpenProtobufDirectory()
    {
        // 프로토 버퍼 위치 초기화
        string protobufBatFilePath = @"Assets\Network\Protobuf";

        Process.Start(protobufBatFilePath);
    }

    //  [MenuItem("Build/Build Protobuf")]
    //  public static void BuildProtobuf()
    //  {
    //      // 프로토 버퍼 위치 초기화
    //      string protobufBatFilePath = @"Assets\Network\Protobuf";
    //      string protobufFileName = @"run_protoc.bat";

    //      Process cmd = new Process();
    //      cmd.StartInfo.FileName = "cmd.exe";

    //      cmd.StartInfo.RedirectStandardInput = true;
    //      cmd.StartInfo.RedirectStandardOutput = true;
    //      cmd.StartInfo.CreateNoWindow = false;
    //      cmd.StartInfo.UseShellExecute = false;

    //      cmd.Start();

    //      using (StreamWriter sw = cmd.StandardInput)
    //{
    //          sw.Write("cd ");
    //          sw.WriteLine(protobufBatFilePath);
    //          sw.WriteLine(protobufFileName);
    //}

    //      cmd.WaitForExit();

    //      UnityEngine.Debug.Log("프로토 버퍼 실행됨 결과 출력");
    //      UnityEngine.Debug.Log(cmd.StandardOutput.ReadToEnd());
    //  }
}

#endif