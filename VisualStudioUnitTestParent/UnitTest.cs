using System;

namespace VisualStudioUnitTestParent
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using ChildProcesses;
    using ChildProcesses.VisualStudioDebug;




    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class UnitTest
    {
        private CustomizedChildProcessManager manager;

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
        public void Initialize()
        {
            VisualStudioDebugHelper.Register();

            manager = new CustomizedChildProcessManager();
            manager.ProcessStateChanged += ManagerOnProcessStateChanged;

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
            manager.StartChildProcess(processInfo, childProcess);

        }

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


        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanup]
        public void Cleanup()
        {
            manager.Dispose();
        }


        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestMethod1()
        {
            Thread.Sleep(2000);
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestMethod2()
        {
            Thread.Sleep(2000);
        }
    }
}
