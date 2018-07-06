using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;
using NUnit3Gui.Interfaces;
using ReactiveUI;
using ITest = NUnit3Gui.Interfaces.ITest;

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
                test.StringStatus = null;
                await Task.Delay(25);

                var process = new RunProcess(test.AssemblyPath, test.TestName);
                var result = await process.Run(ct);
                await Task.Delay(25);

                test.StringStatus = process.StandardOutput.ToString();
                test.Status = result ? TestStatus.Passed : TestStatus.Failed;
            }
            catch (Exception e)
            {
                test.Status = TestStatus.Failed;
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