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
            var instance = new CustomizedChildProcessInstance();
            instance.ProcessStateChanged += new ChildProcessInstance.ProcessStateChangedEventHandler(instance_ProcessStateChanged);
            Console.WriteLine("Child begins with ProcessWatchdog Loop");
            while( ! instance.Shutdown )
            {
                instance.ProcessWatchdog();
                Thread.Sleep(1000);
            }

        }

        static void instance_ProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            var instance = (CustomizedChildProcessInstance) sender;
            Console.WriteLine("Child Process State Changed: " + e.Action );
        }

    }
}
