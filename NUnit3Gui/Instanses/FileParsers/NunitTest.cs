namespace NUnit3Gui.Instanses.FileParsers
{
    public class NunitTest : Test
    {
        public string Id { get; }

        public NunitTest(string id, string filePath, string testName) 
            : base(filePath, testName)
        {
            Id = id;
        }
    }
}