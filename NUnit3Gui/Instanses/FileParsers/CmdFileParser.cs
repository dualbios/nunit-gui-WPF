using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit.Framework;
using NUnit3Gui.Enums;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileParsers
{
    [Export(typeof(IFileParser))]
    public class CmdFileParser : IFileParser
    {
        private readonly string TestAttributeName = typeof(TestAttribute).Name;
        private readonly string TestFixtureAttributeName = typeof(TestFixtureAttribute).Name;

        public string Alias => "CmdParser";

        public string Name => "Load assembly parser";

        public Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct)
        {
            Assembly assembly = Assembly.LoadFrom(fileName);

            return Task.FromResult(assembly.GetTypes()
                .Where(type =>
                    type.GetCustomAttributes(typeof(Attribute), true).Any(_ =>
                        _.GetType().Name == TestFixtureAttributeName))
                .SelectMany(_ => _.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(Attribute), true)
                    .Any(_ => _.GetType().Name == TestAttributeName))
                .Select(methodInfo => methodInfo.DeclaringType.FullName + "." + methodInfo.Name)
                .Select(test => new Test(fileName, test))
                .OfType<ITest>()
                .ToList()
                .AsEnumerable());
        }

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