using System;
using System.Threading;
using System.Windows.Forms;

namespace NotesTasks
{
    internal static class Program
    {
        private static Mutex mutex = new Mutex(true, "NotesTasksGlobalMutex");

        [STAThread]
        static void Main()
        {
            try
            {
                // Try to get mutex ownership
                if (!mutex.WaitOne(TimeSpan.Zero, true))
                {
                    MessageBox.Show("Another instance of Mouse Macro is already running.", "Mouse Macro", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
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
                MessageBox.Show($"An error occurred: {ex.Message}", "Mouse Macro Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                mutex.Dispose();
            }
        }
    }
}
