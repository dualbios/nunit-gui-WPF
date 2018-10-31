using System;
using NUnit.Engine;

namespace NUnit3GUIWPF.Models
{
    public class DefaultRuntimeFramework : IRuntimeFramework
    {
        public Version ClrVersion { get; } = new Version();

        public string DisplayName { get; } = "Default";

        public Version FrameworkVersion { get; } = new Version();

        public string Id { get; } = "Default";

        public string Profile { get; } = string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}