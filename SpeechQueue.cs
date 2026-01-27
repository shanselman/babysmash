using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Dictionary<string, InstalledVoice> _voiceCache = new();
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

            // Signal that new speech is pending (worker will cancel current speech)
            _newItemPending = true;
            _channel.Writer.TryWrite(new SpeechItem(text, culture));
        }

        private volatile bool _newItemPending;

        private void WorkerLoop()
        {
            // SpeechSynthesizer is a COM object that requires STA thread
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA,
                "SpeechSynthesizer requires STA thread");

            _synth = new SpeechSynthesizer
            {
                Rate = -1,  // Slower for baby clarity
                Volume = 100
            };

            try
            {
                var reader = _channel.Reader;
                // Using blocking wait (not await) to ensure we stay on this STA thread.
                // async/await could switch threads after await, breaking STA requirements.
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
            _newItemPending = false;
            
            string textToSpeak = item.Text;
            var voice = GetCachedVoice(item.Culture);
            if (voice == null)
            {
                textToSpeak = "Unsupported Language";
            }
            else if (!voice.Enabled)
            {
                textToSpeak = "Voice Disabled";
            }
            else
            {
                try
                {
                    _synth.SelectVoice(voice.VoiceInfo.Name);
                }
                catch
                {
                    // Keep default voice.
                }
            }

            try
            {
                // Use async speech so we can cancel if new input arrives
                _synth.SpeakAsync(textToSpeak);
                
                // Wait for speech to complete, but cancel if new item arrives
                while (_synth.State == SynthesizerState.Speaking)
                {
                    Thread.Sleep(50);
                    if (_newItemPending)
                    {
                        _synth.SpeakAsyncCancelAll();
                        break;
                    }
                }
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

        private InstalledVoice GetCachedVoice(CultureInfo culture)
        {
            var key = culture.Name;
            lock (_voiceLock)
            {
                if (_voiceCache.TryGetValue(key, out var cached))
                {
                    return cached;
                }

                var voice = TryGetVoiceWithFallback(culture);
                _voiceCache[key] = voice;
                return voice;
            }
        }

        private InstalledVoice TryGetVoiceWithFallback(CultureInfo culture)
        {
            // Try exact culture match (e.g., "es-MX")
            var voice = _synth.GetInstalledVoices(culture).FirstOrDefault();
            if (voice != null)
            {
                return voice;
            }

            // Try fallback from specific locale to base language (e.g., from "es-MX" to "es-ES")
            if (culture.Name.Contains('-'))
            {
                string baseLanguage = culture.Name.Split('-')[0];
                
                // Try common base language variants (es-ES, de-DE, etc.)
                try
                {
                    var baseCulture = new CultureInfo($"{baseLanguage}-{baseLanguage.ToUpper()}");
                    voice = _synth.GetInstalledVoices(baseCulture).FirstOrDefault();
                    if (voice != null)
                    {
                        return voice;
                    }
                }
                catch (CultureNotFoundException)
                {
                    // Invalid culture, continue to next fallback
                }
                catch (ArgumentException)
                {
                    // Invalid culture name format, continue to next fallback
                }

                // Try just the base language (e.g., "es")
                try
                {
                    var langOnlyCulture = new CultureInfo(baseLanguage);
                    voice = _synth.GetInstalledVoices(langOnlyCulture).FirstOrDefault();
                    if (voice != null)
                    {
                        return voice;
                    }
                }
                catch (CultureNotFoundException)
                {
                    // Invalid culture, continue to next fallback
                }
                catch (ArgumentException)
                {
                    // Invalid culture name format, continue to next fallback
                }
            }

            // Final fallback to English
            try
            {
                var enUsCulture = new CultureInfo("en-US");
                voice = _synth.GetInstalledVoices(enUsCulture).FirstOrDefault();
                if (voice != null)
                {
                    return voice;
                }
            }
            catch (CultureNotFoundException)
            {
                // If even en-US fails, try just "en"
            }
            catch (ArgumentException)
            {
                // Invalid culture name format, try just "en"
            }

            try
            {
                var enCulture = new CultureInfo("en");
                voice = _synth.GetInstalledVoices(enCulture).FirstOrDefault();
                if (voice != null)
                {
                    return voice;
                }
            }
            catch (CultureNotFoundException)
            {
                // All fallbacks failed
            }
            catch (ArgumentException)
            {
                // Invalid culture name format
            }

            // Last resort: return any available voice
            return _synth.GetInstalledVoices().FirstOrDefault();
        }
    }
}
