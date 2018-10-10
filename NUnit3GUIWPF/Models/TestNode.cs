using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit;
using NUnit.Engine;
using ReactiveUI;

namespace NUnit3GUIWPF.Models
{
    public class TestNode : ReactiveObject
    {
        private List<TestNode> _children;
        private TimeSpan _duration;
        private TestState _testAction;

        public TestNode(XmlNode xmlNode)
        {
            Xml = xmlNode;
            IsSuite = Xml.Name == "test-suite" || Xml.Name == "test-run";
            Id = Xml.GetAttribute("id");
            Name = Xml.GetAttribute("name");
            FullName = Xml.GetAttribute("fullname");
            Type = IsSuite ? GetAttribute("type") : "TestCase";
            TestCount = IsSuite ? GetAttribute("testcasecount", 0) : 1;
            RunState = Xml.GetRunState();
        }

        public TestNode(string xmlText) : this(XmlHelper.CreateXmlNode(xmlText))
        {
        }

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

        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public string FullName { get; }

        public string Id { get; }

        public bool IsSuite { get; }

        public string Name { get; }

        public RunState RunState { get; }

        public TestState TestAction
        {
            get => _testAction;
            set => this.RaiseAndSetIfChanged(ref _testAction, value);
        }

        public int TestCount { get; }

        public string Type { get; }

        public XmlNode Xml { get; }

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

        public TestFilter GetTestFilter()
        {
            return new TestFilter(string.Format("<filter><id>{0}</id></filter>", this.Id));
        }

        public override string ToString()
        {
            return $"{Name} [{Id}]";
        }

       
    }
}