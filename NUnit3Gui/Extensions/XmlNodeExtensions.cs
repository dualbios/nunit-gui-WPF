using System.Xml;

namespace NUnit3Gui.Extensions
{
    public static class XmlNodeExtensions
    {
        public static string GetAttribute(this XmlNode result, string name)
        {
            return result.Attributes[name]?.Value;
        }
    }
}