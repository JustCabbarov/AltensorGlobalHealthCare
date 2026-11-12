using System.Globalization;

namespace AltensorGlobalHealthCare.Application.Common.Localization;

public static class LocalizedText
{
    public static string Resolve(string? az, string? en, string? ru, string? requestedLang = null)
    {
        var lang = (requestedLang ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName).ToLowerInvariant();

        return lang switch
        {
            "en" => !string.IsNullOrWhiteSpace(en) ? en : (!string.IsNullOrWhiteSpace(az) ? az : ru ?? string.Empty),
            "ru" => !string.IsNullOrWhiteSpace(ru) ? ru : (!string.IsNullOrWhiteSpace(az) ? az : en ?? string.Empty),
            _ => !string.IsNullOrWhiteSpace(az) ? az : (!string.IsNullOrWhiteSpace(en) ? en : ru ?? string.Empty)
        };
    }
}

