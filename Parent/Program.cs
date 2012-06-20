using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParentProcess
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using ChildProcesses;

    class Program
    {
        static void Main(string[] args)
        {
            var manager = new CustomizedChildProcessManager();

            manager.ProcessStateChanged += new ChildProcessManager.ProcessStateChangedEventHandler(manager_ProcessStateChanged);

            string childProcessExePath = @"..\..\..\Child\bin";

#if DEBUG
            childProcessExePath += @"\Debug\Child.exe";
#else
            childProcessExePath += @"\Release\Child.exe";
#endif

            var currentDir = Directory.GetCurrentDirectory();
            childProcessExePath = Path.Combine(currentDir, childProcessExePath);
            childProcessExePath = Path.GetFullPath(childProcessExePath);

            ProcessStartInfo processInfo = new ProcessStartInfo(childProcessExePath);
            var childProcess = new CustomizedChildProcess();
            manager.StartChildProcess(processInfo, childProcess);

            Console.WriteLine("Parent begins with ProcessWatchdog Loop");
            for(int i = 0; i < 20; ++ i)
            {
                Thread.Sleep(1000);
                manager.ProcessWatchdog();
            }

            Console.WriteLine("Parent signals Child to Shutdown");
            childProcess.ParentChildIpc.Shutdown();

            Console.WriteLine("Parent begins with ProcessWatchdog Loop");
            for (int i = 0; i < 20; ++i)
            {
                Thread.Sleep(1000);
                manager.ProcessWatchdog();
            }
        }

        static void manager_ProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            var manager = (CustomizedChildProcessManager) sender;
            Console.WriteLine("Parent Process State Changed: " + e.Action);
            switch(e.Action)
            {
                case ProcessStateChangedAction.StandardOutputMessage:
                    Console.WriteLine("* Msg: " + e.Data);
                    break;

                    case ProcessStateChangedAction.StandardErrorMessage:
                    Console.WriteLine("* Err: " + e.Data);
                    break;
                    case ProcessStateChangedAction.IpcChannelAvail:
                    Console.WriteLine("* IPC Channel Available");
                    var ipcChannel = (IExtendedParentChildIpc) e.ChildProcess.ParentChildIpc;
                    Console.WriteLine("Sending Custom Message at:" + DateTime.Now);
                    ipcChannel.SendCustomMessageToChild("Hello From Parent");
                    Console.WriteLine("Finish Sending Custom Message at:" + DateTime.Now);
                    break;
            }
        }
    }
}
