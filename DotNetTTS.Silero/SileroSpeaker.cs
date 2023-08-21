using System.Reflection;
using Microsoft.Extensions.Logging;
using Wrapper;

namespace DotNetTTS.Silero
{
    /// <summary>
    /// Wrapper for Silero Speech Synthesizer https://github.com/snakers4/silero-models
    /// </summary>
    public class SileroSpeaker : IDisposable
    {
        private readonly string _speaker;
        private readonly int _sampleRate;
        private readonly string _fullModelPath;
        private readonly ScriptWorker _scriptWorker;

        public SileroSpeaker(string modelPath, ILoggerFactory? loggerFactory = null, LogLevel stderrLogLevel = LogLevel.Trace, string speaker = "eugene",
            int sampleRate = 48000,
            string pythonPath = "python.exe",
            string? workDir = null)
        {
            _scriptWorker = new ScriptWorker(loggerFactory?.CreateLogger<ScriptWorker>(), stderrLogLevel, pythonPath, workDir);
            _speaker = speaker;
            _sampleRate = sampleRate;
            _fullModelPath = Path.GetFullPath(modelPath);
        }

        /// <summary>
        /// Initialize python process
        /// </summary>
        public async Task InitializeAsync(CancellationToken token = default)
        {
            await using var scriptStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("DotNetTTS.Silero.External.silero-script.py")!;
            if (scriptStream == null) throw new ArgumentNullException(nameof(scriptStream));
            await _scriptWorker.InitializeAsync(scriptStream, $"\"{_speaker}\" {_sampleRate} \"{_fullModelPath}\"", token);
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