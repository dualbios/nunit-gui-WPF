using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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
    }
}