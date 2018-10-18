using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using NUnit;

namespace NUnit3GUIWPF.Models
{
    public static class TestNodeExtensions
    {
        private static Dictionary<string, RunState> RunStateDictionary = new Dictionary<string, RunState>()
        {
            {"Runnable",  RunState.Runnable},
            {"NotRunnable",  RunState.NotRunnable},
            {"Ignored",  RunState.Ignored},
            {"Explicit",  RunState.Explicit},
            {"Skipped",  RunState.Skipped},
        };

        public static RunState GetRunState(this XmlNode node)
        {
            string state = node.GetAttribute("runstate");
            if (string.IsNullOrEmpty(state))
                return RunState.Unknown;

            return (RunStateDictionary.TryGetValue(state, out RunState runState)) ? runState : RunState.Unknown;
        }

        public static double ParseDuration(this XmlNode report)
        {
            string durationText = report.Attributes["duration"]?.Value;
            double.TryParse(durationText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double duration);
            return duration;
        }
    }
}