// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildProcessManager.cs" company="Maierhofer Software, Germany">
//   
// </copyright>
// <summary>
//   The child process manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;

    /// <summary>
    /// The child process manager.
    /// </summary>
    public class ChildProcessManager : ProcessInstance, IEnumerable
    {
        #region Constants and Fields

        /// <summary>
        ///   The parent process singlelton.
        /// </summary>
        protected static ChildProcessManager parentProcessSinglelton;

        /// <summary>
        ///   The child processes.
        /// </summary>
        private Dictionary<int, ChildProcess> childProcesses;

        /// <summary>
        ///   The child processes lock.
        /// </summary>
        private object childProcessesLock;

        /// <summary>
        ///   The ipc host.
        /// </summary>
        private ServiceHost ipcHost;

        /// <summary>
        ///   The ipc host lock.
        /// </summary>
        private object ipcHostLock = new object();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="ChildProcessManager" /> class.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public ChildProcessManager()
        {
            if (parentProcessSinglelton != null)
            {
                throw new InvalidOperationException("ChildProcessManager Singleton already instantiated");
            }

            parentProcessSinglelton = this;
            this.childProcesses = new Dictionary<int, ChildProcess>();
            this.childProcessesLock = new object();
            this.ResetClientIpcHost();
        }

        #endregion

        #region Delegates

        /// <summary>
        /// The process state changed event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The e. 
        /// </param>
        public delegate void ProcessStateChangedEventHandler(object sender, ProcessStateChangedEventArgs e);

        #endregion

        #region Public Events

        /// <summary>
        ///   The process state changed.
        /// </summary>
        public event ProcessStateChangedEventHandler ProcessStateChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets Current.
        /// </summary>
        public static ChildProcessManager Current
        {
            get
            {
                return parentProcessSinglelton;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The get child parent ipc type.
        /// </summary>
        /// <returns>
        /// </returns>
        protected virtual Type GetChildParentIpcType()
        {
            return typeof(ChildParentIpc);
        }

        /// <summary>
        /// The get i child parent ipc type.
        /// </summary>
        /// <returns>
        /// </returns>
        protected virtual Type GetIChildParentIpcType()
        {
            return typeof(IChildParentIpc);
        }

        protected virtual Type GetIParentChildIpcType()
        {
            return typeof(IParentChildIpc);
        }


        /// <summary>
        /// The process watchdog.
        /// </summary>
        public void ProcessWatchdog()
        {
            List<ChildProcess> exitedProcesses = new List<ChildProcess>();
            ChildProcess[] currentChildProcesses;
            lock (this.childProcessesLock)
            {
                currentChildProcesses = this.childProcesses.Values.ToArray();
            }

            bool sendAliveMessages = this.LastAliveMessage + this.AliveMessageFrquency < DateTime.Now;
            if (sendAliveMessages)
            {
                this.LastAliveMessage = DateTime.Now;
            }

            foreach (var childProcess in currentChildProcesses)
            {
                if (childProcess.Process.HasExited)
                {
                    exitedProcesses.Add(childProcess);
                }
                else
                {
                    if (!childProcess.ipcChannelAvail && childProcess.ParentChildIpc != null)
                    {
                        try
                        {
                            childProcess.ParentChildIpc.ParentIpcInit();
                            childProcess.ipcChannelAvail = true;
                            this.RaiseProcessStateChangedEvent(childProcess, ProcessStateChangedAction.IpcChannelAvail, null);
                        }
                        catch (Exception)
                        {
                            childProcess.ParentChildIpc = null;
                        }
                    }

                    if (sendAliveMessages)
                    {
                        try
                        {
                            if (childProcess.ParentChildIpc != null)
                            {
                                childProcess.ParentChildIpc.ParentAlive();
                            }
                        }
                        catch (Exception)
                        {
                            childProcess.ParentChildIpc = null;  
                        }
                    }

                    if (!childProcess.ipcChannelAvail && childProcess.ParentChildIpc != null)
                    {
                        childProcess.ipcChannelAvail = true;
                        this.RaiseProcessStateChangedEvent(childProcess, ProcessStateChangedAction.IpcChannelAvail, null);
                    }

                    if (!childProcess.watchdogTimeout && childProcess.lastTimeAlive + this.WatchdogTimeout < DateTime.Now)
                    {
                        childProcess.watchdogTimeout = true;
                        this.RaiseProcessStateChangedEvent(childProcess, ProcessStateChangedAction.WatchdogTimeout, null);
                    }
                }
            }

            foreach (var exitedProcess in exitedProcesses)
            {
                this.RaiseProcessStateChangedEvent(exitedProcess, ProcessStateChangedAction.ChildExited, null);
            }

            lock (this.childProcessesLock)
            {
                foreach (var exitedProcess in exitedProcesses)
                {
                    this.childProcesses.Remove(exitedProcess.Process.Id);
                }
            }
        }

        /// <summary>
        /// The start child process.
        /// </summary>
        /// <param name="processInfo">
        /// The process info. 
        /// </param>
        /// <returns>
        /// </returns>
        public ChildProcess StartChildProcess(ProcessStartInfo processInfo)
        {
            var childProcess = new ChildProcess();
            this.StartChildProcess(processInfo, childProcess);
            return childProcess;
        }

        /// <summary>
        /// The start child process.
        /// </summary>
        /// <param name="processInfo">
        /// The process info. 
        /// </param>
        /// <param name="childProcess">
        /// The child process. 
        /// </param>
        public void StartChildProcess(ProcessStartInfo processInfo, ChildProcess childProcess)
        {
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;
            var process = new Process();
            process.StartInfo = processInfo;
            childProcess.PreStartInit(this, process);
            process.Start();
            try
            {
                childProcess.PostStartInit();
                lock (this.childProcessesLock)
                {
                    this.childProcesses.Add(process.Id, childProcess);
                }
            }
            catch (Exception)
            {
                process.Kill();
                throw;
            }
        }

        /// <summary>
        /// The try get child process.
        /// </summary>
        /// <param name="id">
        /// The id. 
        /// </param>
        /// <returns>
        /// </returns>
        public ChildProcess TryGetChildProcess(int id)
        {
            lock (this.childProcessesLock)
            {
                ChildProcess childProcess = null;
                this.childProcesses.TryGetValue(id, out childProcess);
                return childProcess;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The raise process state changed event.
        /// </summary>
        /// <param name="childProcess">
        /// The child process. 
        /// </param>
        /// <param name="action">
        /// The action. 
        /// </param>
        /// <param name="data">
        /// The data. 
        /// </param>
        protected internal virtual void RaiseProcessStateChangedEvent(ChildProcess childProcess, ProcessStateChangedAction action, string data)
        {
            // Raise the event by using the () operator.
            if (this.ProcessStateChanged != null)
            {
                this.ProcessStateChanged(this, new ProcessStateChangedEventArgs(childProcess, action, data));
            }
        }

        protected internal virtual void OnChildIpcInit(int childProcessId)
        {
            this.OnChildAlive(childProcessId);
        }


        /// <summary>
        /// The on child alive.
        /// </summary>
        /// <param name="childProcessId">
        /// The child process id. 
        /// </param>
        protected internal virtual void OnChildAlive(int childProcessId)
        {


            ChildProcess child = this.TryGetChildProcess(childProcessId);
            if (child != null)
            {
                if (child.ParentChildIpc == null)
                {
                    var operationContext = OperationContext.Current;
                    var operationContextType = operationContext.GetType();
                    var getCallbackChannelMethod = operationContextType.GetMethod("GetCallbackChannel");

                    getCallbackChannelMethod = getCallbackChannelMethod.MakeGenericMethod(this.GetIParentChildIpcType());
                    var callback = (IParentChildIpc)getCallbackChannelMethod.Invoke(operationContext, null);
                    child.ParentChildIpc = callback;
                }

                child.watchdogTimeout = false;
                child.lastTimeAlive = DateTime.Now;
            }
        }

        /// <summary>
        /// The ipc host_ faulted.
        /// </summary>
        /// <param name="sender">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The e. 
        /// </param>
        private void IpcHost_Faulted(object sender, EventArgs e)
        {
            lock (this.ipcHostLock)
            {
                if (this.ipcHost != null)
                {
                    this.ResetClientIpcHost();
                }
            }
        }

        /// <summary>
        /// The reset client ipc host.
        /// </summary>
        private void ResetClientIpcHost()
        {
            lock (this.ipcHostLock)
            {
                if (this.ipcHost != null)
                {
                    this.ipcHost.Close();
                    this.ipcHost = null;
                }
                var ipcEndpint = (ChildParentIpc) Activator.CreateInstance(this.GetChildParentIpcType());
                ipcEndpint.Manager = this;
                var newHost = new ServiceHost(ipcEndpint, new[] { new Uri("net.pipe://localhost/" + this.GetIpcUrlPrefix() + "/" + this.CurrentProcess.Id) });
                newHost.AddServiceEndpoint(this.GetIChildParentIpcType(), new NetNamedPipeBinding(), "ParentChildIpc");
                newHost.Faulted += new EventHandler(this.IpcHost_Faulted);
                newHost.Open();
                this.ipcHost = newHost;
            }
        }

        #endregion

        public IEnumerator GetEnumerator()
        {
            List<ChildProcess> currentChildProcesses;
            lock (this.childProcessesLock)
            {
                currentChildProcesses = this.childProcesses.Values.ToList();
            }

            return currentChildProcesses.GetEnumerator();
        }
    }
}