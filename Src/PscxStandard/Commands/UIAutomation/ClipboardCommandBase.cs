//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class for commands interacting with the clipboard.
//
// Creation Date: Dec 12, 2006
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Threading;

using Pscx.Interop;

namespace Pscx.Commands.UIAutomation
{
    public abstract class ClipboardCommandBase : PscxCmdlet
    {
        const int ReadTimeout = 20000;
        const int WriteTimeout = 20000;

        const int RetryCount = 50;
        const int RetryDelay = 400;

        private volatile Exception _exception;

        protected void ExecuteRead(Action read)
        {
            ExecuteOnSTA(ReadTimeout, read);
        }

        protected void ExecuteWrite(Action write)
        {
            ExecuteOnSTA(WriteTimeout, write);
        }

        void ExecuteOnSTA(int timeout, Action action)
        {
            Thread thread = new Thread(delegate()
            {
                try
                {
                    TryExecute(action);
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception exc)
                {
                    _exception = exc;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            if (!thread.Join(timeout))
            {
                WriteError(new ErrorRecord(new TimeoutException(), "ClipboardTimeoutError", ErrorCategory.NotSpecified, null));
            }

            Thread.MemoryBarrier();

            if (_exception != null)
            {
                WriteError(new ErrorRecord(_exception, "ClipboardError", ErrorCategory.NotSpecified, null));
            }
        }

        void TryExecute(Action action)
        {
            TryExecute(action, RetryCount, RetryDelay);
        }

        void TryExecute(Action action, int retryCount, int retryDelay) 
        {
            int remainingCount = retryCount;

            while (remainingCount > 0)
            {
                try
                {
                    action();
                    break;
                }
                catch (ExternalException exc)
                {
                    _exception = new ClipboardException(CurrentClipboardOwner, exc);
                    remainingCount--;

                    Thread.Sleep(retryDelay);
                }
            }
        }

        public static Process CurrentClipboardOwner
        {
            get 
            {
                IntPtr hwnd = NativeMethods.GetOpenClipboardWindow();
                
                if (hwnd != IntPtr.Zero)
                {
                    int processId;
                    int threadId = NativeMethods.GetWindowThreadProcessId(hwnd, out processId);

                    return Process.GetProcessById(processId);
                }

                return null;
            }
        }
    }
}
