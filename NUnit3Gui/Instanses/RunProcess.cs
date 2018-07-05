﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses
{
    public class RunProcess : IRunProcess
    {
        public RunProcess(string workingDirectory, string assemblyPath, string args)
        {
            WorkingDirectory = workingDirectory;
            AssemblyPath = assemblyPath;
            Args = $"\"{AssemblyPath}\" --test={args}";
        }

        public string Args { get; }

        public string AssemblyPath { get; }

        public int ExitCode { get; private set; }

        public string WorkingDirectory { get; }

        public IList<string> StandardOutput { get; private set; } = new List<string>();
        public IList<string> StandardError { get; private set; } = new List<string>();

        public Task<bool> Run()
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(() =>
            {
                try
                {
                    Process localProcess = new Process();
                    localProcess.StartInfo.FileName = "nunit3-console\\nunit3-console.exe";
                    localProcess.StartInfo.Arguments = Args;

                    localProcess.StartInfo.CreateNoWindow = true;
                    localProcess.StartInfo.RedirectStandardError = true;
                    localProcess.StartInfo.RedirectStandardOutput = true;
                    localProcess.StartInfo.RedirectStandardInput = true;
                    localProcess.StartInfo.UseShellExecute = false;

                    //localProcess.StartInfo.WorkingDirectory = WorkingDirectory;

                    localProcess.OutputDataReceived += InternalProcessOnOutputDataReceived;
                    localProcess.ErrorDataReceived += InternalProcessOnErrorDataReceived;

                    localProcess.Start();
                    localProcess.WaitForExit();

                    localProcess.BeginOutputReadLine();
                    localProcess.BeginErrorReadLine();

                    tcs.SetResult(localProcess.ExitCode == 0);
                }
                catch (Exception e)
                {
                    tcs.SetResult(false);
                }
            });

            return tcs.Task;
        }

        private void InternalProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e.Data!=null)
                StandardError.Add(e.Data);
        }

        private void InternalProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                StandardOutput.Add(e.Data);
        }
    }
}