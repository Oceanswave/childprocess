// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildProcessInstance.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   The Child Process Instance represents the child process itself. It is instantiated on the child.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    /// <summary>
    ///     The Child Process Instance represents the child process itself. It is instantiated on the child.
    /// </summary>
    public class ChildProcessInstance : ProcessInstance
    {
        /// <summary>
        /// A dynamic URL part for the IPC channel, transfered from server to client
        /// </summary>
        private string ipcChannelDynamicUrlPart;

        #region Static Fields

        /// <summary>
        ///     The child process singlelton.
        /// </summary>
        protected static ChildProcessInstance childProcessSinglelton;

        #endregion

        #region Fields

        /// <summary>
        ///     The ipc channel.
        /// </summary>
        private IChildParentIpc ipcChannel;

        /// <summary>
        ///     The ipc channel available.
        /// </summary>
        private bool ipcChannelAvailable;

        /// <summary>
        ///     The ipc channel available msg send.
        /// </summary>
        private bool ipcChannelAvailableMsgSend;

        /// <summary>
        ///     The ipc channel factory.
        /// </summary>
        private ChannelFactory ipcChannelFactory;

        /// <summary>
        ///     The ipc channel lock.
        /// </summary>
        private object ipcChannelLock = new object();

        /// <summary>
        ///     The last time alive.
        /// </summary>
        private DateTime lastTimeAlive;

        /// <summary>
        ///     The parent process exited.
        /// </summary>
        private bool parentProcessExited;

        /// <summary>
        ///     The watchdog timeout.
        /// </summary>
        private bool watchdogTimeout;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChildProcessInstance" /> class.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public ChildProcessInstance()
        {
            if (bool.Parse(Environment.GetEnvironmentVariable("ChildProcesses.AutoAttachDebugger")))
            {
                for (int i = 0; i < 30; ++i)
                {
                    // wait 3 seconds to attach the debugger
                    Thread.Sleep(100);
                    if (Debugger.IsAttached)
                    {
                        break;
                    }
                }

                Thread.Sleep(200);
            }

            ipcChannelDynamicUrlPart = Environment.GetEnvironmentVariable("ChildProcesses.IpcDynamicUrlPart");
            if (string.IsNullOrEmpty(ipcChannelDynamicUrlPart)) throw new InvalidOperationException("Dynamic URL Part is not transfered to client");

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
                lock (this.ipcChannelLock)
                {
                    this.ipcChannel.ChildIpcInit(this.CurrentProcess.Id);
                }
            }
            catch (Exception e)
            {
                // TODO Implement handling
                throw;
            }

            this.StartWatchdog();
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The process state changed event handler.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        public delegate void ProcessStateChangedEventHandler(object sender, ProcessStateChangedEventArgs e);

        #endregion

        #region Public Events

        /// <summary>
        ///     The process state changed.
        /// </summary>
        public event ProcessStateChangedEventHandler ProcessStateChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets Current.
        /// </summary>
        public static ChildProcessInstance Current
        {
            get
            {
                return childProcessSinglelton;
            }
        }

        /// <summary>
        ///     Gets the child parent ipc.
        /// </summary>
        public IChildParentIpc ChildParentIpc
        {
            get
            {
                lock (this.ipcChannelLock)
                {
                    return this.ipcChannel;
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether ipc channel available.
        /// </summary>
        public bool IpcChannelAvailable
        {
            get
            {
                return this.ipcChannelAvailableMsgSend;
            }
        }

        /// <summary>
        ///     Gets ParentCallbackEndpoint.
        /// </summary>
        public ParentChildIpc ParentCallbackEndpoint { get; private set; }

        /// <summary>
        ///     Gets or sets ParentProcess.
        /// </summary>
        public Process ParentProcess { get; set; }

        /// <summary>
        ///     Gets a value indicating whether Shutdown.
        /// </summary>
        public bool Shutdown { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether ShutdownOnParentExit.
        /// </summary>
        public bool ShutdownOnParentExit { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether TriggerShutdown.
        /// </summary>
        public bool TriggerShutdown { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The on parent ipc init.
        /// </summary>
        public void OnParentIpcInit()
        {
            this.OnParentAlive();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on parent alive.
        /// </summary>
        protected internal virtual void OnParentAlive()
        {
            this.watchdogTimeout = false;
            this.lastTimeAlive = DateTime.Now;
            if (! this.ipcChannelAvailable)
            {
                this.ipcChannelAvailable = true;
                this.TriggerWatchdog();
            }
        }

        /// <summary>
        ///     The on shutdown.
        /// </summary>
        protected internal virtual void OnShutdown()
        {
            this.TriggerShutdown = true;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (this.ipcChannel != null)
            {
                ((IChannel)this.ipcChannel).Abort();
                this.ipcChannel = null;
            }

            if (this.ipcChannelFactory != null)
            {
                this.ipcChannelFactory.Abort();
                this.ipcChannelFactory = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     The get i child parent ipc type.
        /// </summary>
        /// <returns>
        ///     The <see cref="Type" />.
        /// </returns>
        protected virtual Type GetIChildParentIpcType()
        {
            return typeof(IChildParentIpc);
        }

        /// <summary>
        ///     The get parent child ipc type.
        /// </summary>
        /// <returns>
        ///     The <see cref="Type" />.
        /// </returns>
        protected virtual Type GetParentChildIpcType()
        {
            return typeof(ParentChildIpc);
        }

        /// <summary>
        /// The on watchdog.
        /// </summary>
        protected override void OnWatchdog()
        {
            if (this.ParentProcess.HasExited && ! this.parentProcessExited)
            {
                this.parentProcessExited = true;
                this.RaiseProcessStateChangedEvent(ProcessStateChangedEnum.ParentExited, null);
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
                    this.RaiseProcessStateChangedEvent(ProcessStateChangedEnum.WatchdogTimeout, null);
                }
            }

            if (this.TriggerShutdown && ! this.Shutdown)
            {
                this.Shutdown = true;
                this.RaiseProcessStateChangedEvent(ProcessStateChangedEnum.ChildShutdown, null);
            }

            if (this.ipcChannelAvailable && !this.ipcChannelAvailableMsgSend)
            {
                this.ipcChannelAvailableMsgSend = true;
                this.RaiseProcessStateChangedEvent(ProcessStateChangedEnum.IpcChannelAvail, null);
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
        protected virtual void RaiseProcessStateChangedEvent(ProcessStateChangedEnum action, string data)
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
        /// The <see cref="IChildParentIpc"/>.
        /// </returns>
        private IChildParentIpc CreateChannel(ChannelFactory factory)
        {
            MethodInfo createchannel = factory.GetType().GetMethod("CreateChannel", new Type[0]);
            object channel = createchannel.Invoke(factory, null);
            return (IChildParentIpc)channel;
        }

        /// <summary>
        ///     The create channel factory.
        /// </summary>
        /// <returns>
        ///     The <see cref="ChannelFactory" />.
        /// </returns>
        private ChannelFactory CreateChannelFactory()
        {
            Type channelFactoryType = typeof(DuplexChannelFactory<>);
            channelFactoryType = channelFactoryType.MakeGenericType(this.GetIChildParentIpcType());

            this.ParentCallbackEndpoint = (ParentChildIpc)Activator.CreateInstance(this.GetParentChildIpcType());
            this.ParentCallbackEndpoint.ChildProcessInstance = this;

            var binding = new NetNamedPipeBinding();
            binding.Security.Mode = NetNamedPipeSecurityMode.None;

            return (ChannelFactory)Activator.CreateInstance(channelFactoryType, this.ParentCallbackEndpoint, binding, new EndpointAddress("net.pipe://localhost/" + this.GetIpcUrlPrefix() + ipcChannelDynamicUrlPart + "/" + this.ParentProcess.Id + "/ParentChildIpc"));
        }

        /// <summary>
        ///     The reset ipc channel.
        /// </summary>
        private void ResetIpcChannel()
        {
            lock (this.ipcChannelLock)
            {
                if (this.ParentProcess != null)
                {
                    if (this.ipcChannel != null)
                    {
                        ((IChannel)this.ipcChannel).Abort();
                        this.ipcChannel = null;
                    }

                    if (this.ipcChannelFactory != null)
                    {
                        this.ipcChannelFactory.Abort();
                        this.ipcChannelFactory = null;
                    }

                    this.ipcChannelFactory = this.CreateChannelFactory();
                    this.ipcChannel = this.CreateChannel(this.ipcChannelFactory);
                }
            }
        }

        #endregion
    }
}