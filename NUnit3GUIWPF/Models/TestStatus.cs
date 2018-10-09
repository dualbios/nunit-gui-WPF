namespace NUnit3GUIWPF.Models
{
    public enum TestStatus
    {
        /// <summary>
        /// The test was inconclusive
        /// </summary>
        Inconclusive,

        /// <summary>
        /// The test has skipped 
        /// </summary>
        Skipped,

        /// <summary>
        /// The test succeeded
        /// </summary>
        Passed,

        /// <summary>
        /// There was a warning
        /// </summary>
        Warning,

        /// <summary>
        /// The test failed
        /// </summary>
        Failed
    }

}