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

        public IFileParser CurrentFileParser => FileParsers.First(_ => _.Alias == "CmdParser");

        [ImportMany]
        public IEnumerable<IFileParser> FileParsers { get; private set; }
    }
}
