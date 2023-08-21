using System.Reflection;
using Microsoft.Extensions.Logging;
using Wrapper;

namespace DotNetTTS.Vosk
{
    /// <summary>
    /// Wrapper for Vosk Speech Synthesizer https://github.com/alphacep/vosk-tts
    /// </summary>
    public class VoskSpeaker : IDisposable
    {
        private readonly string _modelName;
        private readonly ScriptWorker _scriptWorker;

        public VoskSpeaker(string modelName, ILoggerFactory loggerFactory = default, LogLevel stderrLogLevel = LogLevel.Trace, string pythonPath = "python.exe",
            string? workDir = null)
        {
            _scriptWorker = new ScriptWorker(loggerFactory?.CreateLogger<ScriptWorker>(), stderrLogLevel, pythonPath, workDir);
            _modelName = modelName;
        }

        /// <summary>
        /// Initialize python process
        /// </summary>
        public async Task InitializeAsync(CancellationToken token = default)
        {
            await using var scriptStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("DotNetTTS.Vosk.External.vosk-script.py")!;
            if (scriptStream == null) throw new ArgumentNullException(nameof(scriptStream));
            await _scriptWorker.InitializeAsync(scriptStream, $"\"{_modelName}\"", token);
        }

        /// <summary>
        /// Synthesize and play audio
        /// </summary>
        public Task SpeakAsync(string text, CancellationToken token = default)
        {
            return _scriptWorker.SpeakAsync(text, token);
        }

        /// <summary>
        /// Create wav file
        /// </summary>
        /// <returns>Full path to file</returns>
        public Task<string> CreateWavAsync(string text, CancellationToken token = default)
        {
            return _scriptWorker.CreateWavAsync(text, token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _scriptWorker.Dispose();
        }
    }
}