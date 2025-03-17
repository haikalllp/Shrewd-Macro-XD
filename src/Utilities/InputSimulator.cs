using System;
using System.Runtime.InteropServices;
using NotesAndTasks.Hooks;

namespace NotesAndTasks.Utilities
{
    /// <summary>
    /// Provides functionality for simulating mouse input and movement.
    /// This class encapsulates the Windows SendInput API for mouse control.
    /// </summary>
    public class InputSimulator : IDisposable
    {
        private bool disposed = false;
        private readonly object lockObject = new object();

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        private const int INPUT_MOUSE = 0;
        private const int MOUSEEVENTF_MOVE = 0x0001;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        /// <summary>
        /// Simulates mouse movement by the specified delta values.
        /// </summary>
        /// <param name="deltaX">The change in X coordinate.</param>
        /// <param name="deltaY">The change in Y coordinate.</param>
        /// <returns>True if the input was successfully simulated, false otherwise.</returns>
        public bool SimulateMouseMovement(int deltaX, int deltaY)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(InputSimulator));
            }

            try
            {
                lock (lockObject)
                {
                    // Validate movement values
                    if (Math.Abs(deltaX) > 100 || Math.Abs(deltaY) > 100)
                    {
                        return false;
                    }

                    var input = new INPUT
                    {
                        type = INPUT_MOUSE,
                        mi = new MOUSEINPUT
                        {
                            dx = deltaX,
                            dy = deltaY,
                            mouseData = 0,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero,
                            dwFlags = MOUSEEVENTF_MOVE
                        }
                    };

                    return SendInput(1, ref input, Marshal.SizeOf(input)) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Simulates mouse movement with jitter pattern.
        /// </summary>
        /// <param name="pattern">The jitter pattern coordinates (dx, dy).</param>
        /// <param name="strength">The strength multiplier for the pattern (1-20).</param>
        /// <returns>True if the input was successfully simulated, false otherwise.</returns>
        public bool SimulateJitterMovement((int dx, int dy) pattern, int strength)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(InputSimulator));
            }

            if (strength < 1 || strength > 20)
            {
                throw new ArgumentOutOfRangeException(nameof(strength), "Strength must be between 1 and 20.");
            }

            int scaledDx = (int)(pattern.dx * strength / 7.0);
            int scaledDy = (int)(pattern.dy * strength / 7.0);

            return SimulateMouseMovement(scaledDx, scaledDy);
        }

        /// <summary>
        /// Simulates mouse movement for recoil reduction.
        /// </summary>
        /// <param name="strength">The strength of the recoil reduction (1-20).</param>
        /// <returns>True if the input was successfully simulated, false otherwise.</returns>
        public bool SimulateRecoilReduction(int strength)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(InputSimulator));
            }

            if (strength < 1 || strength > 20)
            {
                throw new ArgumentOutOfRangeException(nameof(strength), "Strength must be between 1 and 20.");
            }

            int verticalMovement;
            if (strength <= 6)
            {
                if (strength == 1)
                {
                    verticalMovement = Math.Max(1, (int)Math.Round(0.75 * 0.3));
                }
                else
                {
                    double logBase = 1.5;
                    verticalMovement = Math.Max(1, (int)Math.Round(0.75 * Math.Log(strength + 1, logBase)));
                }
            }
            else if (strength <= 16)
            {
                verticalMovement = Math.Max(1, (int)Math.Round(0.75 * strength * 1.2));
            }
            else
            {
                double baseValue = 0.75 * 20.0;
                double scalingFactor = 1.3;
                double exponentialBoost = 1.2;
                verticalMovement = Math.Max(1, (int)Math.Round(
                    baseValue *
                    Math.Pow(scalingFactor, strength - 13) *
                    Math.Pow(exponentialBoost, (strength - 13) / 2)
                ));
            }

            return SimulateMouseMovement(0, verticalMovement);
        }

        /// <summary>
        /// Moves the mouse to the specified coordinates.
        /// </summary>
        /// <param name="dx">The change in X coordinate.</param>
        /// <param name="dy">The change in Y coordinate.</param>
        public void MoveMouse(int dx, int dy)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(InputSimulator));

            if (Math.Abs(dx) > 100 || Math.Abs(dy) > 100)
            {
                throw new ArgumentOutOfRangeException("Movement values exceeded safe limits");
            }

            var input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };

            SendInput(1, ref input, Marshal.SizeOf(input));
        }

        /// <summary>
        /// Disposes of resources used by the InputSimulator.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
} 