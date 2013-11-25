// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParentProgram.cs" company="">
//   
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ParentProcess
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using ChildProcesses;
    using ChildProcesses.VisualStudioDebug;

    /// <summary>
    ///     The program.
    /// </summary>
    internal class ParentProgram
    {
        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            VisualStudioDebugHelper.Register();

            var manager = new CustomizedChildProcessManager();

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

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Parent Process waiting");
            Thread.Sleep(5000);
            Console.WriteLine("Parent Process waiting");
            childProcess.ParentChildIpc.Shutdown();
            Thread.Sleep(5000);

            manager.Dispose();

            Thread.Sleep(20000);
        }

        /// <summary>
        /// The manager_ process state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void ManagerOnProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            var manager = (CustomizedChildProcessManager)sender;
            Console.WriteLine("Parent Process State Changed: " + e.Action);
            switch (e.Action)
            {
                case ProcessStateChangedEnum.StandardOutputMessage:
                    Console.WriteLine("Child Msg: " + e.Data);
                    break;

                case ProcessStateChangedEnum.StandardErrorMessage:
                    Console.WriteLine("Child Err: " + e.Data);
                    break;
                case ProcessStateChangedEnum.IpcChannelAvail:
                    Console.WriteLine("Parent IPC Channel Available");
                    var ipcChannel = (IExtendedParentChildIpc)e.ChildProcess.ParentChildIpc;
                    Console.WriteLine("Sending Custom Message at:" + DateTime.Now);
                    ipcChannel.SendCustomMessageToChild("Hello From Parent");
                    Console.WriteLine("Finish Sending Custom Message at:" + DateTime.Now);
                    break;
            }
        }

        #endregion
    }
}