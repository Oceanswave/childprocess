// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessStateChangedEnum.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public enum ProcessStateChangedEnum
    {
        /// <summary>
        ///     The Child IPC Callback Channel is available
        /// </summary>
        IpcChannelAvail, 

        /// <summary>
        ///     Text is received from Child Process Standard Output
        /// </summary>
        StandardOutputMessage, 

        /// <summary>
        ///     Text is received from Child Process Standard Error
        /// </summary>
        StandardErrorMessage, 

        /// <summary>
        ///     A Child Process has Exited
        /// </summary>
        ChildExited, 

        /// <summary>
        ///     The Parent Process has Exited
        /// </summary>
        ParentExited, 

        /// <summary>
        ///     The Process don't send Watchdog notifications
        /// </summary>
        WatchdogTimeout, 

        /// <summary>
        ///     The Child Process Should Shutdown
        /// </summary>
        ChildShutdown
    }
}