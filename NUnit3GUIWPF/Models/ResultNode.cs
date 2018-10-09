using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit3GUIWPF.Utils;

namespace NUnit3GUIWPF.Models
{
    public class ResultNode : TestNode
    {
        #region Constructors

        public ResultNode(XmlNode xmlNode) : base(xmlNode)
        {
            Status = GetStatus();
            Label = GetAttribute("label");
            Outcome = new ResultState(Status, Label);
            AssertCount = GetAttribute("asserts", 0);
            var duration = GetAttribute("duration");
            Duration = duration != null
                ? double.Parse(duration, CultureInfo.InvariantCulture)
                : 0.0;
        }

        public ResultNode(string xmlText) : this(XmlHelper.CreateXmlNode(xmlText)) { }

        #endregion

        #region Public Properties

        public TestStatus Status { get; }
        public string Label { get; }
        public ResultState Outcome { get; }
        public int AssertCount { get; }
        public double Duration { get; }

        #endregion

        #region Helper Methods

        private TestStatus GetStatus()
        {
            string status = GetAttribute("result");
            switch (status)
            {
                case "Passed":
                default:
                    return TestStatus.Passed;
                case "Inconclusive":
                    return TestStatus.Inconclusive;
                case "Failed":
                    return TestStatus.Failed;
                case "Warning":
                    return TestStatus.Warning;
                case "Skipped":
                    return TestStatus.Skipped;
            }
        }

        #endregion
    }
}
