using System;
using System.Diagnostics;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Base class for wrapping local progam usage 
/// </summary>
public abstract class BaseProgramWrapper {
    /// <summary>
    /// Check if the wrapped program is installed an accessible on the host machine
    /// </summary>
    /// <returns>true if the program can be invoked</returns>
    public abstract bool IsInstalled();

    /// <summary>
    /// Execute a command on the local machine
    /// </summary>
    /// <param name="directory">directory to execute command in</param>
    /// <param name="command">command to execute</param>
    /// <param name="stdout">results of standard output</param>
    /// <param name="stderr">results of standard error</param>
    /// <returns>true if command executed with no errors</returns>
    protected bool TryExecuteCommand(string directory, string command, string[] args, out string stdout, out string stderr) {
        try {
            Process cmd = new Process();
            cmd.StartInfo.FileName = command;
            if (args != null && args.Length > 0) {
                foreach (var arg in args) {
                    cmd.StartInfo.ArgumentList.Add(arg);
                }
            }
            cmd.StartInfo.WorkingDirectory = directory;

            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.WaitForExit();
            stdout = cmd.StandardOutput.ReadToEnd();
            stderr = cmd.StandardError.ReadToEnd();

            if (stderr.Length > 0)
                return false;
            else 
                return true;
        } catch (Exception e) {
            stdout = string.Empty;
            stderr = e.Message;
            return false;
        }
    }
}

}