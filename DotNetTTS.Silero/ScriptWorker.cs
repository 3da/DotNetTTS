using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NAudio.Wave;

namespace Wrapper
{
    internal class ScriptWorker : IDisposable
    {
        private readonly LogLevel _stderrLogLevel;
        private string? _workDir;
        private SemaphoreSlim _semaphore = new(0, 1);
        private readonly string _pythonFullPath;
        private Process? _process;

        private static int _index = 0;
        private bool _deleteWorkDir;
        private readonly ILogger _logger;

        public ScriptWorker(ILogger? logger = null, LogLevel stderrLogLevel = LogLevel.Trace, string pythonPath = "python.exe",
            string? workDir = null)
        {
            _logger = logger ?? NullLogger<ScriptWorker>.Instance;
            _stderrLogLevel = stderrLogLevel;
            _workDir = workDir;
            _pythonFullPath = pythonPath;
        }

        public async Task InitializeAsync(Stream scriptStream, string arguments, CancellationToken token = default)
        {
            if (_workDir == null)
            {
                _workDir = Directory.CreateDirectory("TempDir_" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + "_" + Interlocked.Increment(ref _index)).FullName;
                _deleteWorkDir = true;
            }

            var scriptPath = Path.Combine(_workDir, "script.py");
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);

            {
                await using var fileStream = File.OpenWrite(scriptPath);
                await scriptStream.CopyToAsync(fileStream, token);

                var encoding = Encoding.UTF8;

                _process = Process.Start(new ProcessStartInfo(_pythonFullPath,
                    $"script.py {arguments}")
                {
                    WorkingDirectory = _workDir,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    StandardInputEncoding = encoding,
                    StandardErrorEncoding = encoding,
                    StandardOutputEncoding = encoding
                });

                if (_process == null)
                    throw new Exception("Cannot start process");
            }

            string? line;
            do
            {
                line = await _process.StandardOutput.ReadLineAsync(token);
            } while (line != "[READY]" && line != null);

            if (line == null)
            {
                var errorOutput = await _process.StandardError.ReadToEndAsync(token);
                throw new Exception("Cannot initialize script:\n" + errorOutput);
            }

            _process.BeginErrorReadLine();

            _process.ErrorDataReceived += _process_ErrorDataReceived;

            _semaphore.Release();
        }

        private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _logger.Log(_stderrLogLevel, e.Data);
            }
        }

        public async Task SpeakAsync(string text, CancellationToken token = default)
        {
            string fullFileName = await CreateWavAsync(text, token);

            var tcs = new TaskCompletionSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await using var reader = new WaveFileReader(fullFileName);

                    using WaveOutEvent player = new WaveOutEvent();

                    player.Init(reader);

                    player.Volume = 1;

                    player.Play();

                    player.PlaybackStopped += (_, _) => { tcs.SetResult(); };

                    await using var _ = token.Register(() => player.Stop());

                    await tcs.Task;

                    reader.Close();

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Cannot play audio");
                }
                finally
                {
                    File.Delete(fullFileName);
                }
            }, token);

            await tcs.Task;
        }

        public async Task<string> CreateWavAsync(string text, CancellationToken token = default)
        {
            if (_process == null || _workDir == null)
                throw new Exception("Initialization required");
            await _semaphore.WaitAsync(token);

            try
            {
                await _process.StandardInput.WriteLineAsync(text.Replace("\n", " ").Replace("\r", ""));
                var fileName = await _process.StandardOutput.ReadLineAsync(token);
                if (fileName == null)
                {
                    var errorOutput = await _process.StandardError.ReadToEndAsync(token);
                    throw new Exception("Cannot process text:\n" + errorOutput);
                }
                var fullFileName = Path.Combine(_workDir, fileName);
                return fullFileName;

            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();

            if (_deleteWorkDir)
            {
                if (_process == null || _process.HasExited)
                    Directory.Delete(_workDir!, true);
                else
                {
                    _process.Exited += (sender, args) =>
                    {
                        Directory.Delete(_workDir!, true);
                    };
                }
            }

            _process?.Dispose();

        }
    }
}
