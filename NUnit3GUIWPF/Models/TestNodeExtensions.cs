using System;
using System.Globalization;
using System.Xml;
using NUnit;

namespace NUnit3GUIWPF.Models
{
    public static class TestNodeExtensions
    {
        public static RunState GetRunState(this XmlNode node)
        {
            string state = node.GetAttribute("runstate");
            switch (state)
            {
                case "Runnable":
                    return RunState.Runnable;

                case "NotRunnable":
                    return RunState.NotRunnable;

                case "Ignored":
                    return RunState.Ignored;

                case "Explicit":
                    return RunState.Explicit;

                case "Skipped":
                    return RunState.Skipped;

                default:
                    return RunState.Unknown;
            }
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