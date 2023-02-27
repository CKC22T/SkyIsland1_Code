using Network.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Utils;

public class SingleServerController : MonoSingleton<SingleServerController>
{
    public readonly string ProcessExeFileName = "./LocalServerBuild/CKC2022.exe";
    //public readonly string ProcessExeFileName = "../Build/LocalServerBuild/CKC2022.exe";
    private Process sigleServer = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnApplicationQuit()
    {
        KillProcess();
    }

    public void StartProcess(ServerMode mode, int port)
    {
        if (sigleServer != null) return;

        ProcessStartInfo info = new ProcessStartInfo(ProcessExeFileName);
        info.Arguments = $"{(int)mode} {port}";
        info.UseShellExecute = false;
        info.CreateNoWindow = true;

        sigleServer = new Process();
        sigleServer.StartInfo = info;
        sigleServer.EnableRaisingEvents = true;
        sigleServer.Exited += ProcessExited;

        sigleServer.Start();

        UnityEngine.Debug.Log($"프로세스 실행됨 [{port}]");
        //UnityEngine.Debug.Log($"프로세스 실행됨 [{sigleServer.ProcessName} : {sigleServer.Id}]");
    }

    public void KillProcess()
    {
        if (sigleServer == null) return;

        sigleServer.Kill();
        sigleServer = null;
    }

    private void ProcessExited(object sender, EventArgs e)
    {
        var currentProcess = sender as Process;

        UnityEngine.Debug.Log("프로세스 종료됨");
        UnityEngine.Debug.Log($"Process ID : {currentProcess.Id}");
        UnityEngine.Debug.Log($"Message : {e}");

        sigleServer = null;
    }
}
