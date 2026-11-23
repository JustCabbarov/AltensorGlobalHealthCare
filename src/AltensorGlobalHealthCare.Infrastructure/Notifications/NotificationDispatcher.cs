using System.Net.Http.Json;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Domain.Entities;

namespace AltensorGlobalHealthCare.Infrastructure.Notifications;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly HttpClient _httpClient;

    public NotificationDispatcher(
        IConfiguration configuration,
        ILogger<NotificationDispatcher> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task SendVerificationCodeEmailAsync(string email, string name, string code, CancellationToken cancellationToken = default)
    {
        var subject = "AltensorGlobalHealthCare — E-poçt təsdiq kodunuz";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #0077b6; text-align: center;">AltensorGlobalHealthCare Tibbi Portalına Xoş Gəlmisiniz!</h2>
                <p>Hörmətli <strong>{name}</strong>,</p>
                <p>Hesabınızı təsdiqləmək üçün aşağıdakı 6 rəqəmli təsdiq kodundan istifadə edin:</p>
                <div style="background-color: #f0f7fa; padding: 15px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #023e8a; border-radius: 6px; margin: 20px 0;">
                    {code}
                </div>
                <p style="color: #666; font-size: 13px;">Bu kod növbəti 15 dəqiqə ərzində etibarlıdır. Əgər siz bu qeydiyyatı başlatmamısınızsa, bu məktubu nəzərə almayın.</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                <p style="color: #999; font-size: 12px; text-align: center;">&copy; {DateTime.UtcNow.Year} AltensorGlobalHealthCare. Bütün hüquqlar qorunur.</p>
            </div>
            """;

        await SendEmailAsync(email, name, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetCodeEmailAsync(string email, string name, string code, CancellationToken cancellationToken = default)
    {
        var subject = "AltensorGlobalHealthCare — Şifrə sıfırlama kodunuz";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                <h2 style="color: #d90429; text-align: center;">Şifrə Sıfırlama Tələbi</h2>
                <p>Hörmətli <strong>{name}</strong>,</p>
                <p>Hesabınızın şifrəsini yeniləmək üçün aşağıdakı təsdiq kodunu daxil edin:</p>
                <div style="background-color: #fff1f2; padding: 15px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #9f1239; border-radius: 6px; margin: 20px 0;">
                    {code}
                </div>
                <p style="color: #666; font-size: 13px;">Bu kod 15 dəqiqə ərzində etibarlıdır. Əgər siz bu sorğunu göndərməmisinizsə, hesabınızın təhlükəsizliyini təmin etmək üçün dəstək komandamızla əlaqə saxlayın.</p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                <p style="color: #999; font-size: 12px; text-align: center;">&copy; {DateTime.UtcNow.Year} AltensorGlobalHealthCare.</p>
            </div>
            """;

        await SendEmailAsync(email, name, subject, body, cancellationToken);
    }

    public async Task SendInquiryNotificationToCoordinatorAsync(Inquiry inquiry, CancellationToken cancellationToken = default)
    {
        var coordinatorEmail = _configuration["Notifications:CoordinatorEmail"] ?? "hospitality@altensorglobalhealthcare.com";
        var subject = $"[Yeni Müraciət] {inquiry.InquiryNumber} — {inquiry.Patient?.FullName}";
        var body = $"""
            <div style="font-family: Arial, sans-serif;">
                <h3>Yeni pasient müraciəti qəbul edildi!</h3>
                <p><strong>Nömrə:</strong> {inquiry.InquiryNumber}</p>
                <p><strong>Pasient:</strong> {inquiry.Patient?.FullName} ({inquiry.Patient?.Email})</p>
                <p><strong>Telefon:</strong> {inquiry.Patient?.PhoneNumber}</p>
                <p><strong>Simptomlar:</strong> {inquiry.Symptoms}</p>
                <p><strong>Vaxt seçimi:</strong> {inquiry.PreferredTimeframe ?? "Təyin edilməyib"}</p>
                <p><strong>Sənədlər sayı:</strong> {inquiry.Documents.Count}</p>
            </div>
            """;

        await SendEmailAsync(coordinatorEmail, "Baş Koordinator", subject, body, cancellationToken);

        // Also send Telegram alert if configured
        var telegramBotToken = _configuration["Notifications:Telegram:BotToken"];
        var telegramChatId = _configuration["Notifications:Telegram:ChatId"];
        if (!string.IsNullOrWhiteSpace(telegramBotToken) && !string.IsNullOrWhiteSpace(telegramChatId))
        {
            var telegramText = $"🔔 *Yeni Pasient Müraciəti!*\n\n" +
                               $"*Nömrə:* `{inquiry.InquiryNumber}`\n" +
                               $"*Pasient:* {inquiry.Patient?.FullName}\n" +
                               $"*Simptomlar:* {inquiry.Symptoms}\n" +
                               $"*Status:* {inquiry.Status}";
            await SendTelegramMessageAsync(telegramBotToken, telegramChatId, telegramText, cancellationToken);
        }
    }

    public async Task SendContactMessageNotificationToCoordinatorAsync(ContactMessage contactMessage, CancellationToken cancellationToken = default)
    {
        var coordinatorEmail = _configuration["Notifications:CoordinatorEmail"] ?? "hospitality@altensorglobalhealthcare.com";
        var subject = $"[Əlaqə Forması] {contactMessage.Subject ?? "Yeni mesaj"} — {contactMessage.Name}";
        var body = $"""
            <div style="font-family: Arial, sans-serif;">
                <h3>Əlaqə formasından yeni mesaj daxil oldu</h3>
                <p><strong>Ad:</strong> {contactMessage.Name}</p>
                <p><strong>E-poçt:</strong> {contactMessage.Email}</p>
                <p><strong>Telefon:</strong> {contactMessage.Phone ?? "-"}</p>
                <p><strong>Mövzu:</strong> {contactMessage.Subject ?? "-"}</p>
                <p><strong>Mesaj:</strong><br/>{contactMessage.Message}</p>
            </div>
            """;

        await SendEmailAsync(coordinatorEmail, "Baş Koordinator", subject, body, cancellationToken);

        var telegramBotToken = _configuration["Notifications:Telegram:BotToken"];
        var telegramChatId = _configuration["Notifications:Telegram:ChatId"];
        if (!string.IsNullOrWhiteSpace(telegramBotToken) && !string.IsNullOrWhiteSpace(telegramChatId))
        {
            var telegramText = $"📩 *Yeni Əlaqə Mesajı!*\n\n" +
                               $"*Ad:* {contactMessage.Name}\n" +
                               $"*E-poçt:* {contactMessage.Email}\n" +
                               $"*Mövzu:* {contactMessage.Subject ?? "-"}\n" +
                               $"*Mesaj:* {contactMessage.Message}";
            await SendTelegramMessageAsync(telegramBotToken, telegramChatId, telegramText, cancellationToken);
        }
    }

    public async Task SendOfferNotificationToPatientAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        if (offer.Inquiry?.Patient == null) return;

        var patient = offer.Inquiry.Patient;
        var subject = $"Yeni Tibbi Təklif — {offer.Clinic?.Name} ({offer.Inquiry.InquiryNumber})";
        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; padding: 20px;">
                <h2 style="color: #0077b6;">Müraciətiniz üzrə yeni təklif hazırlandı!</h2>
                <p>Hörmətli <strong>{patient.FullName}</strong>,</p>
                <p><strong>{offer.Clinic?.Name}</strong> klinikası sizin <em>{offer.Inquiry.InquiryNumber}</em> nömrəli müraciətiniz üzrə rəsmi tibbi təklif təqdim etdi.</p>
                <table style="width: 100%; border-collapse: collapse; margin: 15px 0;">
                    <tr><td style="padding: 8px; border-bottom: 1px solid #ddd;">Tibbi Müalicə:</td><td style="padding: 8px; border-bottom: 1px solid #ddd; font-weight: bold;">{offer.MedicalTreatmentPrice:N2} {offer.Currency}</td></tr>
                    <tr><td style="padding: 8px; border-bottom: 1px solid #ddd;">Mehmanxana:</td><td style="padding: 8px; border-bottom: 1px solid #ddd;">{offer.AccommodationPrice:N2} {offer.Currency}</td></tr>
                    <tr><td style="padding: 8px; border-bottom: 1px solid #ddd;">Transfer:</td><td style="padding: 8px; border-bottom: 1px solid #ddd;">{offer.AirportTransferPrice:N2} {offer.Currency}</td></tr>
                    <tr style="background: #f8fafc;"><td style="padding: 8px; font-weight: bold;">Cəmi:</td><td style="padding: 8px; font-weight: bold; color: #023e8a; font-size: 18px;">{offer.TotalPrice:N2} {offer.Currency}</td></tr>
                </table>
                <p>Təklifi qəbul etmək və ya ətraflı baxmaq üçün şəxsi kabinetinizə daxil olun.</p>
            </div>
            """;

        await SendEmailAsync(patient.Email!, patient.FullName, subject, body, cancellationToken);
    }

    public async Task SendNewMessageNotificationAsync(Inquiry inquiry, InquiryMessage message, string recipientEmail, CancellationToken cancellationToken = default)
    {
        var subject = $"Yeni Mesaj — Müraciət {inquiry.InquiryNumber}";
        var body = $"""
            <div style="font-family: Arial, sans-serif; padding: 20px;">
                <h3>Müraciətiniz üzrə yeni mesaj var</h3>
                <p><strong>Göndərən:</strong> {message.SenderName} ({message.SenderRole})</p>
                <div style="background: #f1f5f9; padding: 12px; border-radius: 6px; font-style: italic;">
                    "{message.Body}"
                </div>
            </div>
            """;

        await SendEmailAsync(recipientEmail, "İstifadəçi", subject, body, cancellationToken);
    }

    public async Task<(bool Success, string BotUsername, string ChatId, string Message)> ConnectAndVerifyTelegramBotAsync(
        string botToken, string? manualChatId, CancellationToken cancellationToken = default)
    {
        try
        {
            var meUrl = $"https://api.telegram.org/bot{botToken}/getMe";
            var meResponse = await _httpClient.GetAsync(meUrl, cancellationToken);
            if (!meResponse.IsSuccessStatusCode)
            {
                return (false, string.Empty, string.Empty, "Telegram Bot Token etibarsızdır.");
            }

            var meJson = await meResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var username = meJson.GetProperty("result").GetProperty("username").GetString() ?? "UnknownBot";

            var chatId = manualChatId;
            if (string.IsNullOrWhiteSpace(chatId))
            {
                var updatesUrl = $"https://api.telegram.org/bot{botToken}/getUpdates";
                var updateResponse = await _httpClient.GetAsync(updatesUrl, cancellationToken);
                if (updateResponse.IsSuccessStatusCode)
                {
                    var updateJson = await updateResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
                    var results = updateJson.GetProperty("result");
                    if (results.GetArrayLength() > 0)
                    {
                        var last = results[results.GetArrayLength() - 1];
                        if (last.TryGetProperty("message", out var msg) && msg.TryGetProperty("chat", out var chat))
                        {
                            chatId = chat.GetProperty("id").GetInt64().ToString();
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(chatId))
            {
                return (false, username, string.Empty, "Bot aşkar edildi, lakin heç bir mesaj tapılmadı. Botu Telegram-da açıb /start göndərin.");
            }

            // Send test message
            await SendTelegramMessageAsync(botToken, chatId, "✅ *AltensorGlobalHealthCare* sistemi ilə Telegram əlaqəsi uğurla quruldu!", cancellationToken);

            return (true, username, chatId, "Telegram botu uğurla qoşuldu və sınaq bildirişi göndərildi.");
        }
        catch (Exception ex)
        {
            return (false, string.Empty, string.Empty, $"Telegram qoşulmasında xəta: {ex.Message}");
        }
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var smtpHost = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogInformation("SMTP konfiqurasiya edilməyib. [MOCK EMAIL] Göndərildi: {Email}, Mövzu: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@altensorglobalhealthcare.com";
            var fromName = _configuration["Smtp:FromName"] ?? "AltensorGlobalHealthCare";

            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
            var enableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) && ssl;

            await client.ConnectAsync(smtpHost, port, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);

            var user = _configuration["Smtp:Username"];
            var pass = _configuration["Smtp:Password"];
            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
            {
                await client.AuthenticateAsync(user, pass, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("E-poçt uğurla çatdırıldı: {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-poçt göndərilərkən xəta: {ToEmail}, Mövzu: {Subject}", toEmail, subject);
        }
    }

    private async Task SendTelegramMessageAsync(string botToken, string chatId, string text, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = text,
                parse_mode = "Markdown"
            };

            var res = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Telegram bildirişi göndərilmədi: {Error}", err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram mesajı göndərilərkən xəta baş verdi");
        }
    }
}

