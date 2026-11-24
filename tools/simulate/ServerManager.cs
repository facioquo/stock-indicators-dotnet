using System.ComponentModel;
using System.Diagnostics;

namespace Test.Simulation;

internal static class ServerManager
{
    internal static Process? StartServer(int port)
    {
        try
        {
            // Find the repository root by looking for Stock.Indicators.sln
            string? repoRoot = FindRepositoryRoot();
            if (repoRoot is null)
            {
                Console.WriteLine("[ServerManager] Cannot find repository root. Server not started.");
                return null;
            }

            string serverProjectPath = Path.Combine(repoRoot, "tools", "sse-server");
            string serverExePath = Path.Combine(
                serverProjectPath,
                "bin",
                "Debug",
                "net11.0",
                "Test.SseServer.exe");

            // If exe doesn't exist, try running with dotnet run
            if (!File.Exists(serverExePath))
            {
                Console.WriteLine($"[ServerManager] Server executable not found at {serverExePath}. Attempting dotnet run...");
                return StartServerWithDotNetRun(serverProjectPath, port);
            }

            ProcessStartInfo startInfo = new() {
                FileName = serverExePath,
                Arguments = $"--urls http://localhost:{port}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process? process = Process.Start(startInfo);
            if (process is not null)
            {
                Console.WriteLine($"[ServerManager] SSE server started on port {port} (PID: {process.Id})");

                // Give the process a moment and check if it's still running
                System.Threading.Thread.Sleep(500);
                if (process.HasExited)
                {
                    Console.WriteLine($"[ServerManager] WARNING: Server process exited immediately with code {process.ExitCode}");
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        Console.WriteLine($"[ServerManager] STDOUT: {stdout}");
                    }

                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        Console.WriteLine($"[ServerManager] STDERR: {stderr}");
                    }
                }
            }

            return process;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[ServerManager] I/O error starting server: {ex.Message}");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[ServerManager] Invalid operation: {ex.Message}");
            return null;
        }
    }

    private static Process? StartServerWithDotNetRun(string projectPath, int port)
    {
        try
        {
            ProcessStartInfo startInfo = new() {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" -- --urls http://localhost:{port}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process? process = Process.Start(startInfo);
            if (process is not null)
            {
                Console.WriteLine($"[ServerManager] SSE server started with 'dotnet run' on port {port} (PID: {process.Id})");
            }

            return process;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[ServerManager] I/O error starting server with dotnet run: {ex.Message}");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[ServerManager] Invalid operation starting server: {ex.Message}");
            return null;
        }
    }

    private static string? FindRepositoryRoot()
    {
        string currentDir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (File.Exists(Path.Combine(currentDir, "Stock.Indicators.sln")))
            {
                return currentDir;
            }

            DirectoryInfo? parent = Directory.GetParent(currentDir);
            if (parent is null)
            {
                break;
            }

            currentDir = parent.FullName;
        }

        return null;
    }

    internal static void StopServer(Process? process)
    {
        if (process?.HasExited == false)
        {
            try
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(3000);
                Console.WriteLine("[ServerManager] SSE server stopped");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[ServerManager] Error stopping server: {ex.Message}");
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine($"[ServerManager] Error stopping server: {ex.Message}");
            }
        }
    }
}
