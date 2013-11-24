﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//   
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcess
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using ChildProcesses;

    /// <summary>
    /// The program.
    /// </summary>
    internal class ChildProgram
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
            var instance = new CustomizedChildProcessInstance();
            instance.ProcessStateChanged += new ChildProcessInstance.ProcessStateChangedEventHandler(instance_ProcessStateChanged);
            Console.WriteLine("Child begins with ProcessWatchdog Loop");
            Console.WriteLine("IPC Channel Bla: " + Environment.GetEnvironmentVariable("ChildProcessesIpcChannelPrefix"));
            while (! instance.Shutdown && ! instance.IpcChannelAvailable)
            {
                Thread.Sleep(1000);
            }

            if (instance.IpcChannelAvailable)
            {
                var ipcChannel = (IExtendedChildParentIpc)instance.ChildParentIpc;
                Console.WriteLine("Sending Custom Message at:" + DateTime.Now);
                ipcChannel.SendCustomMessageToParent("Hello From Child");
                Console.WriteLine("Finish Sending Custom Message at:" + DateTime.Now);
            }

            while (! instance.Shutdown)
            {
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// The instance_ process state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void instance_ProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            var instance = (CustomizedChildProcessInstance)sender;
            switch (e.Action)
            {
                case ProcessStateChangedEnum.IpcChannelAvail:
                    Console.WriteLine("Child Process IpcChannelAvail");
                    break;
                default:
                    Console.WriteLine("Child Process State Changed: " + e.Action);
                    break;
            }
        }

        #endregion
    }
}