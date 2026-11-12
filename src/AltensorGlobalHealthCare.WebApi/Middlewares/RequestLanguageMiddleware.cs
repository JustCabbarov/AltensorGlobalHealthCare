using System.Globalization;

namespace AltensorGlobalHealthCare.WebApi.Middlewares;

public class RequestLanguageMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] SupportedLanguages = { "az", "en", "ru" };

    public RequestLanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var lang = "az";

        // 1. Query parameter ?lang=en
        if (context.Request.Query.TryGetValue("lang", out var queryLang) &&
            !string.IsNullOrWhiteSpace(queryLang))
        {
            var clean = queryLang.ToString().Trim().ToLowerInvariant();
            if (SupportedLanguages.Contains(clean))
            {
                lang = clean;
            }
        }
        // 2. Accept-Language header
        else if (context.Request.Headers.TryGetValue("Accept-Language", out var acceptLang) &&
                 !string.IsNullOrWhiteSpace(acceptLang))
        {
            var primary = acceptLang.ToString().Split(',')[0].Trim().ToLowerInvariant();
            if (primary.Length >= 2)
            {
                var twoLetter = primary.Substring(0, 2);
                if (SupportedLanguages.Contains(twoLetter))
                {
                    lang = twoLetter;
                }
            }
        }

        var culture = new CultureInfo(lang == "en" ? "en-US" : (lang == "ru" ? "ru-RU" : "az-Latn-AZ"));
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        context.Items["Language"] = lang;

        await _next(context);
    }
}

