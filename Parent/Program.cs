// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
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
    /// The program.
    /// </summary>
    internal class Program
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

            manager.ProcessStateChanged += new ChildProcessManager.ProcessStateChangedEventHandler(manager_ProcessStateChanged);

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

            Console.WriteLine("Parent begins with ProcessWatchdog Loop");
            for (int i = 0; i < 20; ++ i)
            {
                Thread.Sleep(1000);
                manager.ProcessWatchdog();
            }

            Console.WriteLine("Parent signals Child to Shutdown");

            // childProcess.ParentChildIpc.Shutdown();
            Console.WriteLine("Parent begins with ProcessWatchdog Loop");
            for (int i = 0; i < 20; ++i)
            {
                Thread.Sleep(1000);
                manager.ProcessWatchdog();
            }

            manager.Dispose();
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
        private static void manager_ProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            var manager = (CustomizedChildProcessManager)sender;
            Console.WriteLine("Parent Process State Changed: " + e.Action);
            switch (e.Action)
            {
                case ProcessStateChangedAction.StandardOutputMessage:
                    Console.WriteLine("* Msg: " + e.Data);
                    break;

                case ProcessStateChangedAction.StandardErrorMessage:
                    Console.WriteLine("* Err: " + e.Data);
                    break;
                case ProcessStateChangedAction.IpcChannelAvail:
                    Console.WriteLine("* IPC Channel Available");
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