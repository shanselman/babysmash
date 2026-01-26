using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Channels;

namespace BabySmash
{
    internal sealed class SpeechQueue : IDisposable
    {
        private readonly Channel<SpeechItem> _channel;
        private readonly Thread _workerThread;
        private readonly CancellationTokenSource _cts = new();
        private SpeechSynthesizer _synth;
        private readonly Dictionary<string, VoiceInfo> _voiceCache = new();
        private readonly object _voiceLock = new();

        private readonly struct SpeechItem
        {
            public SpeechItem(string text, CultureInfo culture)
            {
                Text = text;
                Culture = culture;
            }

            public string Text { get; }
            public CultureInfo Culture { get; }
        }

        public SpeechQueue(int capacity = 5)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            };
            _channel = Channel.CreateBounded<SpeechItem>(options);

            _workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true
            };
            _workerThread.SetApartmentState(ApartmentState.STA);
            _workerThread.Start();
        }

        public void Enqueue(string text, CultureInfo culture, bool priority = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (priority)
            {
                // Make room for a high-priority utterance (e.g., a detected word).
                while (_channel.Reader.TryRead(out _))
                {
                }
            }

            _channel.Writer.TryWrite(new SpeechItem(text, culture));
        }

        private void WorkerLoop()
        {
            _synth = new SpeechSynthesizer
            {
                Rate = -1,
                Volume = 100
            };

            try
            {
                var reader = _channel.Reader;
                while (reader.WaitToReadAsync(_cts.Token).AsTask().GetAwaiter().GetResult())
                {
                    while (reader.TryRead(out var item))
                    {
                        SpeakItem(item);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
            finally
            {
                _synth?.Dispose();
            }
        }

        private void SpeakItem(SpeechItem item)
        {
            string textToSpeak = item.Text;
            var voiceInfo = GetCachedVoiceInfo(item.Culture);
            if (voiceInfo == null)
            {
                textToSpeak = "Unsupported Language";
            }
            else if (!voiceInfo.Enabled)
            {
                textToSpeak = "Voice Disabled";
            }
            else
            {
                try
                {
                    _synth.SelectVoice(voiceInfo.Name);
                }
                catch
                {
                    // Keep default voice.
                }
            }

            try
            {
                _synth.Speak(textToSpeak);
            }
            catch
            {
                // Swallow speech errors to keep the app responsive.
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _channel.Writer.TryComplete();
        }

        private VoiceInfo GetCachedVoiceInfo(CultureInfo culture)
        {
            var key = culture.Name;
            lock (_voiceLock)
            {
                if (_voiceCache.TryGetValue(key, out var cached))
                {
                    return cached;
                }

                var voice = _synth.GetInstalledVoices(culture).FirstOrDefault();
                var voiceInfo = voice?.VoiceInfo;
                _voiceCache[key] = voiceInfo;
                return voiceInfo;
            }
        }
    }
}
