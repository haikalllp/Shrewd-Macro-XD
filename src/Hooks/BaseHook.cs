using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NotesAndTasks.Hooks
{
    /// <summary>
    /// Base class for Windows low-level hooks implementation.
    /// </summary>
    public abstract class BaseHook : IDisposable
    {
        private IntPtr hookID = IntPtr.Zero;
        private readonly NativeMethods.LowLevelHookProc hookCallback;
        private bool disposed;

        /// <summary>
        /// Gets the Windows hook identifier.
        /// </summary>
        public IntPtr HookID => hookID;

        /// <summary>
        /// Gets the Windows hook type (e.g., WH_KEYBOARD_LL or WH_MOUSE_LL).
        /// </summary>
        protected abstract int HookType { get; }

        /// <summary>
        /// Initializes a new instance of the BaseHook class.
        /// </summary>
        protected BaseHook()
        {
            hookCallback = HookCallback;
        }

        /// <summary>
        /// Sets up the hook with the specified module handle.
        /// </summary>
        /// <param name="moduleHandle">The handle to the module containing the hook procedure.</param>
        public void SetHook(IntPtr moduleHandle)
        {
            if (hookID != IntPtr.Zero)
                throw new InvalidOperationException("Hook is already set");

            hookID = NativeMethods.SetWindowsHookEx(HookType, hookCallback, moduleHandle, 0);
            if (hookID == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to set {GetType().Name}");
        }

        /// <summary>
        /// Starts monitoring events.
        /// </summary>
        public void Start()
        {
            if (hookID == IntPtr.Zero)
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                if (curModule == null)
                    throw new InvalidOperationException("Failed to get current module");

                SetHook(NativeMethods.GetModuleHandle(curModule.ModuleName));
            }
        }

        /// <summary>
        /// Stops monitoring events.
        /// </summary>
        public void Stop()
        {
            if (hookID != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(hookID);
                hookID = IntPtr.Zero;
            }
        }

        /// <summary>
        /// The hook callback that processes low-level input events.
        /// </summary>
        protected abstract IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Releases the unmanaged resources used by the hook.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the hook and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stop();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the BaseHook class.
        /// </summary>
        ~BaseHook()
        {
            Dispose(false);
        }
    }
}