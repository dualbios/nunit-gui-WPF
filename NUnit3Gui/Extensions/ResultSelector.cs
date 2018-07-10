using System;

namespace NUnit3Gui.Extensions
{
    public static class ResultSelector
    {
        public static Func<bool, bool, bool> And2Result = (result1, result2) => result1 && result2;
        public static Func<bool, bool, bool, bool> And3Result = (result1, result2, result3) => result1 && result2 && result3;
        public static Func<bool, bool, bool, bool, bool> And4Result = (result1, result2, result3, result4) => result1 && result2 && result3 && result4;
    }
}