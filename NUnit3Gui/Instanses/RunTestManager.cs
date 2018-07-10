using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit3Gui.Enums;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses
{
    [Export(typeof(IRunTestManager))]
    public class RunTestManager : IRunTestManager
    {
        public async Task<Unit> RunTestAsync(ITest test, CancellationToken ct)
        {
            try
            {
                test.IsRunning = true;
                test.Status = TestState.Running;
                test.StringStatus = null;
                await Task.Delay(25);

                var process = new RunProcess(test.AssemblyPath, test.TestName);
                var timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
                var startTime = DateTime.Now;
                timer.Tick += (sender, args) => { test.RunningTime = DateTime.Now - startTime; };
                timer.Start();
                test.Status = await process.Run(ct);
                timer.Stop();
                await Task.Delay(25);

                test.StringStatus = process.StandardOutput.ToString();
            }
            catch (Exception e)
            {
                test.Status = TestState.Failed;
                test.StringStatus = e.Message;
            }
            finally
            {
                test.IsRunning = false;
            }

            await Task.Delay(25);
            return Unit.Default;
        }
    }
}