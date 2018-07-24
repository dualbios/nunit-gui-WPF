using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileParsers
{
    [Export(typeof(IFileParserManager))]
    public class FileParserManager : IFileParserManager
    {
        private readonly string currentParserAlias = "NUnitParser";//"CmdParser"

        public IFileParser CurrentFileParser => FileParsers.First(_ => _.Alias == currentParserAlias);

        [ImportMany]
        public IEnumerable<IFileParser> FileParsers { get; private set; }
    }
}
