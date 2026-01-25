using System.Globalization;

namespace BabySmash.Core.Interfaces;

public interface ITtsService
{
    void Speak(string text);
    bool TrySetLanguage(CultureInfo culture);
    void CancelSpeech();
}
