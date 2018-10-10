namespace NUnit3GUIWPF.Models
{
    public enum TestAction
    {
        TestLoaded,
        TestUnloaded,
        TestReloaded,

        /// <summary>
        /// The test run is starting.
        /// </summary>
        RunStarting,

        /// <summary>
        /// The test run has completed.
        /// </summary>
        RunFinished,

        // TODO: DO we need this or is an exception sufficient?
        /// <summary>
        /// The test run failed to complete due to a catastrophic error.
        /// </summary>
        RunFailed,

        /// <summary>
        /// A suite of tests is about to execute.
        /// </summary>
        SuiteStarting,

        /// <summary>
        /// A suite of tests has completed execution.
        /// </summary>
        SuiteFinished,

        /// <summary>
        /// A single test case is about to execute.
        /// </summary>
        TestStarting,

        /// <summary>
        /// A single test case has completed execution.
        /// </summary>
        TestFinished,

        /// <summary>
        /// An unhandled exception was fired during the execution
        /// of a test run and it isn't possible to determine which
        /// teset caused it.
        /// </summary>
        TestException,

        /// <summary>
        /// A test has created some text output.
        /// </summary>
        TestOutput
    }
}