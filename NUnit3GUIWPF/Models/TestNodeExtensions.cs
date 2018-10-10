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

        public static TimeSpan ParseDuration(this XmlNode report)
        {
            string startTimeString = report.Attributes["start-time"]?.Value;
            string endTimeString = report.Attributes["end-time"]?.Value;
            if (string.IsNullOrEmpty(startTimeString) || string.IsNullOrEmpty(endTimeString))
                return TimeSpan.Zero;

            DateTime startTime = DateTime.MinValue;
            DateTime endTime = DateTime.MinValue;
            DateTime.TryParse(startTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces, out startTime);
            DateTime.TryParse(endTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces, out endTime);

            return endTime - startTime;
        }
    }
}