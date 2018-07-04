﻿using System.Collections.Generic;

namespace NUnit3Gui.Interfaces
{
    public interface IFileItem
    {
        string FilePath { get; }

        int TestCount { get; }

        IEnumerable<string> Tests { get; set; }
    }
}