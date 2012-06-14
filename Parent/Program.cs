using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParentProcess
{
    using System.Diagnostics;
    using System.Threading;

    using ChildProcesses;

    class Program
    {
        static void Main(string[] args)
        {
            var manager = new CustomizedChildProcessManager();
            ProcessStartInfo processInfo = new ProcessStartInfo(@"D:\Test\AppDomainTest\ChildProcess\bin\Debug\Child.exe");
            var childProcess = manager.StartChildProcess(processInfo);

            for(;;)
            {
                Thread.Sleep(1000);
                manager.ProcessWatchdog();
                Console.WriteLine("Parent: Here We Are");

            }
        }
    }
}
