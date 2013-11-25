To enable child process debugging

Add to your parent process main following line:

VisualStudioDebugHelper.Register();

before you create the CustomizedChildProcessManager

Th set breakpoints in the child process code:
Uncheck "Enable Just My Code" in the debugger settings.


