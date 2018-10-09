using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit;
using NUnit.Engine;

namespace NUnit3GUIWPF.Models
{
    public class TestNode 
    {
        #region Constructors

        public TestNode(XmlNode xmlNode)
        {
            Xml = xmlNode;
            IsSuite = Xml.Name == "test-suite" || Xml.Name == "test-run";
            Id = Xml.GetAttribute("id");
            Name = Xml.GetAttribute("name");
            FullName = Xml.GetAttribute("fullname");
            Type = IsSuite ? GetAttribute("type") : "TestCase";
            TestCount = IsSuite ? GetAttribute("testcasecount", 0) : 1;
            RunState = GetRunState();
        }

        public TestNode(string xmlText) : this(XmlHelper.CreateXmlNode(xmlText)) { }

        #endregion

        #region Public Properties

        public XmlNode Xml { get; }
        public bool IsSuite { get; }
        public string Id { get; }
        public string Name { get; }
        public string FullName { get; }
        public string Type { get; }
        public int TestCount { get; }
        public RunState RunState { get; }

        private List<TestNode> _children;
        public IList<TestNode> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<TestNode>();
                    foreach (XmlNode node in Xml.ChildNodes)
                        if (node.Name == "test-case" || node.Name == "test-suite")
                            _children.Add(new TestNode(node));
                }

                return _children;
            }
        }

        #endregion

        #region Public Methods

        public TestFilter GetTestFilter()
        {
            return new TestFilter(string.Format("<filter><id>{0}</id></filter>", this.Id));
        }

        public string GetAttribute(string name)
        {
            return Xml.GetAttribute(name);
        }

        public int GetAttribute(string name, int defaultValue)
        {
            return Xml.GetAttribute(name, defaultValue);
        }

        public string GetProperty(string name)
        {
            var propNode = Xml.SelectSingleNode("properties/property[@name='" + name + "']");

            return (propNode != null)
                ? propNode.GetAttribute("value")
                : null;
        }

        // Get a comma-separated list of all properties having the specified name
        public string GetPropertyList(string name)
        {
            var propList = Xml.SelectNodes("properties/property[@name='" + name + "']");
            if (propList == null || propList.Count == 0) return string.Empty;

            StringBuilder result = new StringBuilder();

            foreach (XmlNode propNode in propList)
            {
                var val = propNode.GetAttribute("value");
                if (result.Length > 0)
                    result.Append(',');
                result.Append(FormatPropertyValue(val));
            }

            return result.ToString();
        }

        public string[] GetAllProperties(bool displayHiddenProperties)
        {
            var items = new List<string>();

            foreach (XmlNode propNode in this.Xml.SelectNodes("properties/property"))
            {
                var name = propNode.GetAttribute("name");
                var val = propNode.GetAttribute("value");
                if (name != null && val != null)
                    if (displayHiddenProperties || !name.StartsWith("_"))
                        items.Add(name + " = " + FormatPropertyValue(val));
            }

            return items.ToArray();
        }

        #endregion

        #region Helper Methods

        private static string FormatPropertyValue(string val)
        {
            return val == null
                ? "<null>"
                : val == string.Empty
                    ? "<empty>"
                    : val;
        }

        private RunState GetRunState()
        {
            switch (GetAttribute("runstate"))
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

        #endregion
    }

}
