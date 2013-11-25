// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitTest.cs" company="">
//   
// </copyright>
// <summary>
//   The unit test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VisualStudioUnitTestParent
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using ChildProcesses;
    using ChildProcesses.VisualStudioDebug;

    using NUnit.Framework;

    /// <summary>
    /// The unit test.
    /// </summary>
    [TestFixture]
    public class UnitTest
    {
        #region Fields

        /// <summary>
        /// The manager.
        /// </summary>
        private CustomizedChildProcessManager manager;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The cleanup.
        /// </summary>
        [TestFixtureTearDown]
        public void Cleanup()
        {
            this.manager.Dispose();
        }

        /// <summary>
        /// The initialize.
        /// </summary>
        [TestFixtureSetUp]
        public void Initialize()
        {
            VisualStudioDebugHelper.Register();

            this.manager = new CustomizedChildProcessManager();
            this.manager.ProcessStateChanged += this.ManagerOnProcessStateChanged;

            string childProcessExePath = @"..\..\..\Child\bin";
#if DEBUG
            childProcessExePath += @"\Debug\Child.exe";
#else
            childProcessExePath += @"\Release\Child.exe";
#endif

            string currentDir = Directory.GetCurrentDirectory();
            childProcessExePath = Path.Combine(currentDir, childProcessExePath);
            childProcessExePath = Path.GetFullPath(childProcessExePath);

            var processInfo = new ProcessStartInfo(childProcessExePath);
            var childProcess = new CustomizedChildProcess();
            this.manager.StartChildProcess(processInfo, childProcess);
        }

        /// <summary>
        /// The test method 1.
        /// </summary>
        [Test]
        public void TestMethod1()
        {
            Thread.Sleep(2000);
        }

        /// <summary>
        /// The test method 2.
        /// </summary>
        [Test]
        public void TestMethod2()
        {
            Thread.Sleep(2000);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The manager on process state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="processStateChangedEventArgs">
        /// The process state changed event args.
        /// </param>
        private void ManagerOnProcessStateChanged(object sender, ProcessStateChangedEventArgs processStateChangedEventArgs)
        {
            switch (processStateChangedEventArgs.Action)
            {
                case ProcessStateChangedEnum.StandardErrorMessage:
                    Console.WriteLine(processStateChangedEventArgs.Data);
                    break;
                case ProcessStateChangedEnum.StandardOutputMessage:
                    Console.WriteLine(processStateChangedEventArgs.Data);
                    break;
            }
        }

        #endregion
    }
}