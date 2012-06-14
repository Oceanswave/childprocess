using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChildProcess
{
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;

    using ChildProcesses;

    class Program
    {
        static void Main(string[] args)
        {
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(1000);
            }

            var instance = new CustomizedChildProcessInstance();
            while( ! instance.Shutdown )
            {
                instance.ProcessWatchdog();
                Thread.Sleep(1000);
                Console.WriteLine("Child: Here we are");
            }

        }

    }
}
