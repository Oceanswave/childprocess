// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildProcessInstance.cs" company="Maierhofer Software, Germany">
//   
// </copyright>
// <summary>
//   The child process instance.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    /// <summary>
    /// The child process instance.
    /// </summary>
    public class ChildProcessInstance : ProcessInstance
    {
        public IChildParentIpc ChildParentIpc
        {
            get
            {
                lock(ipcChannelLock)
                {
                    return ipcChannel;
                }
            }
        }
        #region Constants and Fields

        /// <summary>
        ///   The child process singlelton.
        /// </summary>
        protected static ChildProcessInstance childProcessSinglelton;

        /// <summary>
        ///   The ipc channel.
        /// </summary>
        private IChildParentIpc ipcChannel;

        /// <summary>
        ///   The ipc channel factory.
        /// </summary>
        private ChannelFactory ipcChannelFactory;

        /// <summary>
        ///   The ipc channel lock.
        /// </summary>
        private object ipcChannelLock = new object();

        /// <summary>
        ///   The last time alive.
        /// </summary>
        private DateTime lastTimeAlive;



        /// <summary>
        ///   The parent process exited.
        /// </summary>
        private bool parentProcessExited;


        private bool ipcChannelAvailable;
        private bool ipcChannelAvailableMsgSend;

        public bool IpcChannelAvailable
        {
            get
            {
                return ipcChannelAvailableMsgSend;
            }
        }

        /// <summary>
        ///   The watchdog timeout.
        /// </summary>
        private bool watchdogTimeout;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="ChildProcessInstance" /> class.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public ChildProcessInstance()
        {
            if (childProcessSinglelton != null)
            {
                throw new InvalidOperationException("ChildProcessInstance Singleton already instantiated");
            }

            childProcessSinglelton = this;
            this.ShutdownOnParentExit = true;
            this.ParentProcess = this.CurrentProcess.GetParent();
            this.lastTimeAlive = DateTime.Now;
            try
            {
                this.ResetIpcChannel();
                lock( ipcChannelLock )
                {
                    ipcChannel.ChildIpcInit(CurrentProcess.Id);
                }
            }
            catch( Exception e)
            {
                // TODO Implement handling
                throw;
            }
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
        public static ChildProcessInstance Current
        {
            get
            {
                return childProcessSinglelton;
            }
        }

        /// <summary>
        ///   Gets ParentCallbackEndpoint.
        /// </summary>
        public ParentChildIpc ParentCallbackEndpoint { get; private set; }

        /// <summary>
        ///   Gets or sets ParentProcess.
        /// </summary>
        public Process ParentProcess { get; set; }

        /// <summary>
        ///   Gets a value indicating whether Shutdown.
        /// </summary>
        public bool Shutdown { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether ShutdownOnParentExit.
        /// </summary>
        public bool ShutdownOnParentExit { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether TriggerShutdown.
        /// </summary>
        public bool TriggerShutdown { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The get i child parent ipc type.
        /// </summary>
        /// <returns>
        /// </returns>
        protected virtual Type GetIChildParentIpcType()
        {
            return typeof(IChildParentIpc);
        }

        /// <summary>
        /// The get parent child ipc type.
        /// </summary>
        /// <returns>
        /// </returns>
        protected virtual Type GetParentChildIpcType()
        {
            return typeof(ParentChildIpc);
        }

        /// <summary>
        /// The process watchdog.
        /// </summary>
        public void ProcessWatchdog()
        {
            if (! this.parentProcessExited && this.ParentProcess.HasExited)
            {
                this.parentProcessExited = true;
                this.RaiseProcessStateChangedEvent(ProcessStateChangedAction.ParentExited, null);
                if (this.ShutdownOnParentExit)
                {
                    this.TriggerShutdown = true;
                }
            }
            else
            {
                if (!this.watchdogTimeout && this.lastTimeAlive + this.WatchdogTimeout < DateTime.Now)
                {
                    this.watchdogTimeout = true;
                    this.RaiseProcessStateChangedEvent(ProcessStateChangedAction.WatchdogTimeout, null);
                }
            }

            if (this.TriggerShutdown && ! this.Shutdown)
            {
                this.Shutdown = true;
                this.RaiseProcessStateChangedEvent(ProcessStateChangedAction.ChildShutdown, null);
            }

            bool sendAliveMessages = this.LastAliveMessage + this.AliveMessageFrquency < DateTime.Now;
            if (!this.parentProcessExited && sendAliveMessages)
            {
                try
                {
                    this.ipcChannel.ChildAlive(this.CurrentProcess.Id);
                    this.LastAliveMessage = DateTime.Now;
                }
                catch (EndpointNotFoundException)
                {
                    this.ResetIpcChannel();
                }
                catch (CommunicationObjectFaultedException)
                {
                    this.ResetIpcChannel();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            if( ipcChannelAvailable && ! ipcChannelAvailableMsgSend )
            {
                ipcChannelAvailableMsgSend = true;
                this.RaiseProcessStateChangedEvent(ProcessStateChangedAction.IpcChannelAvail, null);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on parent alive.
        /// </summary>
        protected internal virtual void OnParentAlive()
        {
            this.watchdogTimeout = false;
            this.lastTimeAlive = DateTime.Now;
            if( ! ipcChannelAvailable )
            {
                ipcChannelAvailable = true;
            }
        }

        /// <summary>
        /// The on shutdown.
        /// </summary>
        protected internal virtual void OnShutdown()
        {
            this.TriggerShutdown = true;
        }

        /// <summary>
        /// The raise process state changed event.
        /// </summary>
        /// <param name="action">
        /// The action. 
        /// </param>
        /// <param name="data">
        /// The data. 
        /// </param>
        protected virtual void RaiseProcessStateChangedEvent(ProcessStateChangedAction action, string data)
        {
            if (this.ProcessStateChanged != null)
            {
                this.ProcessStateChanged(this, new ProcessStateChangedEventArgs(null, action, data));
            }
        }

        /// <summary>
        /// The create channel.
        /// </summary>
        /// <param name="factory">
        /// The factory. 
        /// </param>
        /// <returns>
        /// </returns>
        private IChildParentIpc CreateChannel(ChannelFactory factory)
        {
            var createchannel = factory.GetType().GetMethod("CreateChannel", new Type[0]);
            var channel = createchannel.Invoke(factory, null);
            return (IChildParentIpc)channel;
        }

        /// <summary>
        /// The create channel factory.
        /// </summary>
        /// <returns>
        /// </returns>
        private ChannelFactory CreateChannelFactory()
        {
            var channelFactoryType = typeof(DuplexChannelFactory<>);
            channelFactoryType = channelFactoryType.MakeGenericType(this.GetIChildParentIpcType());

            this.ParentCallbackEndpoint = (ParentChildIpc)Activator.CreateInstance(this.GetParentChildIpcType());
            this.ParentCallbackEndpoint.ChildProcessInstance = this;

            return (ChannelFactory)Activator.CreateInstance(channelFactoryType, this.ParentCallbackEndpoint, new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/" + this.GetIpcUrlPrefix() + "/" + this.ParentProcess.Id + "/ParentChildIpc"));
        }

        /// <summary>
        /// The reset ipc channel.
        /// </summary>
        private void ResetIpcChannel()
        {
            lock (this.ipcChannelLock)
            {
                if (this.ParentProcess != null)
                {
                    this.ipcChannelFactory = this.CreateChannelFactory();
                    this.ipcChannel = this.CreateChannel(this.ipcChannelFactory);
                }
            }
        }

        #endregion

        public void OnParentIpcInit()
        {
            this.OnParentAlive();
        }
    }
}