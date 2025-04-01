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
                    // Strict validation of movement values
                    const int MaxMovement = 100;
                    
                    // Validate movement is within safe limits
                    if (Math.Abs(deltaX) > MaxMovement)
                    {
                        // Clamp X movement to max value while preserving direction
                        deltaX = deltaX > 0 ? MaxMovement : -MaxMovement;
                    }
                    
                    if (Math.Abs(deltaY) > MaxMovement)
                    {
                        // Clamp Y movement to max value while preserving direction
                        deltaY = deltaY > 0 ? MaxMovement : -MaxMovement;
                    }
                    
                    // Additional safety check - zero movement should return immediately
                    if (deltaX == 0 && deltaY == 0)
                    {
                        return true; // No need to call SendInput
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

                    uint result = SendInput(1, ref input, Marshal.SizeOf(input));
                    return result == 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to simulate mouse movement: {ex.Message}");
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

            try
            {
                // Validate strength strictly within range
                if (strength < 1)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid jitter strength: {strength}, using minimum value (1)");
                    strength = 1;
                }
                else if (strength > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid jitter strength: {strength}, using maximum value (20)");
                    strength = 20;
                }

                // Validate pattern values
                if (Math.Abs(pattern.dx) > 30 || Math.Abs(pattern.dy) > 30)
                {
                    System.Diagnostics.Debug.WriteLine($"Pattern values too large: ({pattern.dx}, {pattern.dy}), clamping to safe values");
                    pattern.dx = Math.Clamp(pattern.dx, -30, 30);
                    pattern.dy = Math.Clamp(pattern.dy, -30, 30);
                }

                // Calculate movement with enhanced safety
                double safeScaling = Math.Min(strength / 7.0, 2.85); // Cap the scaling factor
                int scaledDx = (int)(pattern.dx * safeScaling);
                int scaledDy = (int)(pattern.dy * safeScaling);

                // Additional overflow check
                if (Math.Abs(scaledDx) > 100 || Math.Abs(scaledDy) > 100)
                {
                    System.Diagnostics.Debug.WriteLine($"Scaled values too large: ({scaledDx}, {scaledDy}), clamping to safe values");
                    scaledDx = Math.Clamp(scaledDx, -100, 100);
                    scaledDy = Math.Clamp(scaledDy, -100, 100);
                }

                return SimulateMouseMovement(scaledDx, scaledDy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to simulate jitter movement: {ex.Message}");
                return false;
            }
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

            try
            {
                // Validate strength strictly within range
                if (strength < 1)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid recoil reduction strength: {strength}, using minimum value (1)");
                    strength = 1;
                }
                else if (strength > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid recoil reduction strength: {strength}, using maximum value (20)");
                    strength = 20;
                }

                int verticalMovement;
                const double BASE_RECOIL_STRENGTH = 0.75;

                // Low level recoil reduction (1-6)
                if (strength <= 6)
                {
                    // lowest level of recoil reduction (1)
                    if (strength == 1)
                    {
                        verticalMovement = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * 0.3));
                    }
                    // Progressive scaling for 2-6 with more distinct steps
                    else
                    {
                        // Use stepped scaling to create more noticeable differences
                        double stepMultiplier = strength switch
                        {
                            2 => 1.0,
                            3 => 1.5,
                            4 => 1.9,
                            5 => 2.2,
                            6 => 3.0,
                            _ => 1.5
                        };
                        verticalMovement = Math.Max(1, (int)Math.Round(BASE_RECOIL_STRENGTH * stepMultiplier * 2.0));
                    }
                }
                // Medium level recoil reduction (7-16)
                else if (strength <= 16)
                {
                    // Smoother scaling with reduced multiplier
                    double baseValue = BASE_RECOIL_STRENGTH * 1.1;
                    double scalingFactor = 1 + ((strength - 6) * 0.08);
                    verticalMovement = Math.Max(1, (int)Math.Round(baseValue * strength * scalingFactor));
                }
                // High level recoil reduction (17-20)
                else
                {
                    double baseValue = BASE_RECOIL_STRENGTH * 20.0;
                    double scalingFactor = 1.18; // Reduced from 1.19 for smoother scaling
                    double exponentialBoost = 1.08; // Reduced from 1.1 for smoother scaling
                    verticalMovement = Math.Max(1, (int)Math.Round(
                        baseValue *
                        Math.Pow(scalingFactor, strength - 13) *
                        Math.Pow(exponentialBoost, strength - 13)
                    ));
                }

                // Safety check to prevent extreme movements
                if (verticalMovement > 100)
                {
                    System.Diagnostics.Debug.WriteLine($"Excessive recoil reduction movement detected: {verticalMovement}, clamping to maximum");
                    verticalMovement = 100;
                }

                return SimulateMouseMovement(0, verticalMovement);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to simulate recoil reduction: {ex.Message}");
                return false;
            }
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

            try
            {
                const int MaxMovement = 100;
                
                // Strict validation of movement values with boundary enforcement
                if (Math.Abs(dx) > MaxMovement || Math.Abs(dy) > MaxMovement)
                {
                    System.Diagnostics.Debug.WriteLine($"Movement values ({dx}, {dy}) exceeded safe limits, clamping");
                    
                    // Clamp movements to safe values
                    dx = Math.Clamp(dx, -MaxMovement, MaxMovement);
                    dy = Math.Clamp(dy, -MaxMovement, MaxMovement);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to move mouse: {ex.Message}");
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the InputSimulator and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here, if any
                }

                // Clean up any unmanaged resources here
                // Nothing specific needed for SendInput API

                disposed = true;
            }
        }

        /// <summary>
        /// Disposes of resources used by the InputSimulator.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the InputSimulator class.
        /// </summary>
        ~InputSimulator()
        {
            Dispose(false);
        }
    }
} 