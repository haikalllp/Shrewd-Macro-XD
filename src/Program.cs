using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Principal;

namespace NotesAndTasks
{
    internal static class Program
    {
        private static Mutex mutex = new Mutex(true, "NotesTasksGlobalMutex");

        [STAThread]
        static void Main()
        {
            try
            {
                // Check for administrative privileges
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

                if (!isAdmin)
                {
                    // Restart the application with admin rights
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.FileName = Application.ExecutablePath;
                    startInfo.Verb = "runas";

                    try
                    {
                        Process.Start(startInfo);
                        return;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("This application requires administrative privileges to run.", "Notes&Tasks",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Try to get mutex ownership
                if (!mutex.WaitOne(TimeSpan.Zero, true))
                {
                    MessageBox.Show("Another instance of Notes&Tasks is already running.", "Notes&Tasks", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Optimize process settings for background operation alongside games
                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    // Set process priority to BelowNormal to avoid competing with games
                    currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;

                    // Set CPU affinity to use last core
                    // This keeps the macro off the primary cores that games typically use
                    if (Environment.ProcessorCount > 1)
                    {
                        int lastCore = Environment.ProcessorCount - 1;
                        currentProcess.ProcessorAffinity = (IntPtr)(1 << lastCore);
                    }

                    // Enable Windows 11 Efficiency Mode
                    // This helps reduce resource competition with games
                    try
                    {
                        if (Environment.OSVersion.Version.Build >= 22621) // Windows 11 22H2 build
                        {
                            currentProcess.ProcessorAffinity = currentProcess.ProcessorAffinity;
                        }
                    }
                    catch (Exception) { /* Ignore if not supported */ }
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                try
                {
                    Application.Run(new MacroForm());
                }
                finally
                {
                    // Release mutex when application exits
                    mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Notes&Tasks Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                mutex.Dispose();
            }
        }
    }
}
