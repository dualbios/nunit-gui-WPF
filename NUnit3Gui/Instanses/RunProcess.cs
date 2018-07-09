using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses
{
    public class RunProcess : IRunProcess
    {
        private const string nu3cPath = "nunit3-console\\nunit3-console.exe";
        public RunProcess(string assemblyPath, string args)
        {
            AssemblyPath = assemblyPath;
            Args = $"\"{AssemblyPath}\" --test={args}";
        }

        public string Args { get; }

        public string AssemblyPath { get; }

        public int ExitCode { get; private set; }

        public StringBuilder StandardOutput { get; private set; } = new StringBuilder();
        public StringBuilder StandardError { get; private set; } = new StringBuilder();

        public Task<bool> Run()
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(() =>
            {
                try
                {
                    using (Process localProcess = new Process())
                    {
                        localProcess.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nu3cPath);
                        localProcess.StartInfo.Arguments = Args;

                        localProcess.StartInfo.CreateNoWindow = true;
                        localProcess.StartInfo.RedirectStandardError = true;
                        localProcess.StartInfo.RedirectStandardOutput = true;
                        localProcess.StartInfo.RedirectStandardInput = true;
                        localProcess.StartInfo.UseShellExecute = false;

                        localProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(AssemblyPath);

                        localProcess.OutputDataReceived += InternalProcessOnOutputDataReceived;
                        localProcess.ErrorDataReceived += InternalProcessOnErrorDataReceived;

                        localProcess.Start();
                        localProcess.WaitForExit();

                        localProcess.BeginOutputReadLine();
                        localProcess.BeginErrorReadLine();

                        tcs.SetResult(localProcess.ExitCode == 0);
                    }
                }
                catch (Exception)
                {
                    tcs.SetResult(false);
                }
            });

            return tcs.Task;
        }

        public Task<bool> Run(CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(() =>
            {
                try
                {
                    using (Process localProcess = new Process())
                    {
                        localProcess.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nu3cPath);
                        localProcess.StartInfo.Arguments = Args;

                        localProcess.StartInfo.CreateNoWindow = true;
                        localProcess.StartInfo.RedirectStandardError = true;
                        localProcess.StartInfo.RedirectStandardOutput = true;
                        localProcess.StartInfo.RedirectStandardInput = true;
                        localProcess.StartInfo.UseShellExecute = false;

                        localProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(AssemblyPath);

                        localProcess.OutputDataReceived += InternalProcessOnOutputDataReceived;
                        localProcess.ErrorDataReceived += InternalProcessOnErrorDataReceived;

                        localProcess.Start();
                        localProcess.WaitForExit();

                        localProcess.BeginOutputReadLine();
                        localProcess.BeginErrorReadLine();

                        tcs.SetResult(localProcess.ExitCode == 0);
                    }
                }
                catch (Exception)
                {
                    tcs.SetResult(false);
                }
            },
                ct);

            return tcs.Task;
        }

        private void InternalProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e.Data!=null)
                StandardError.AppendLine(e.Data);
        }

        private void InternalProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                StandardOutput.AppendLine(e.Data);
        }
    }
}