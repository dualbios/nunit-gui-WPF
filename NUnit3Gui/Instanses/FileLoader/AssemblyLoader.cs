using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoader))]
    public class AssemblyLoader : IFileLoader
    {
        public string Extension { get; } = ".dll";

        public Task<IEnumerable<IFileItem>> LoadAsync(string filePath)
        {
            FileItem fileItem = new FileItem(filePath);
            Task.Run(() => CollectTests(fileItem, filePath));

            return Task.FromResult(new[] { fileItem }.OfType<IFileItem>().AsEnumerable());
        }

        private async Task CollectTests(FileItem fileItem, string filePath)
        {
            fileItem.StringState = "loading ...";
            await Task.Delay(500);

            try
            {
                Assembly assembly = Assembly.LoadFrom(filePath);
                var sss = assembly.GetTypes();
                var ddd = sss.Where(type => type.GetCustomAttributes(typeof(Attribute), true).Any(_=> _.GetType().Name == typeof(TestFixtureAttribute).Name));

                fileItem.Tests = ddd
                    .Select(type => type.FullName)
                    .ToList();

                fileItem.StringState = $"{fileItem.TestCount} classes(s)";
            }
            catch (Exception)
            {
                fileItem.StringState = "error loading.";
            }
        }
    }
}