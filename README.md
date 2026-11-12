# AltensorGlobalHealthCare — Tibbi Turizm və Rəqəmsal Səhiyyə Platforması (.NET 8 Web API)

`zagrebmed.com` sisteminin rəqəmsal arxitekturası əsasında hazırlanmış, `Otel SaaS` layihəsinin **Clean Architecture** konvensiyaları ilə qurulmuş tam funksional .NET 8 Web API backend platforması.

---

## 🏗️ Arxitektura və Texnoloji Stek

- **Platforma:** .NET 8 (LTS) Web API
- **Arxitektura:** Clean Architecture (5 Layer)
  - `AltensorGlobalHealthCare.Domain` — Vahid biznes entity-ləri, enum-lar, interface-lər.
  - `AltensorGlobalHealthCare.Application` — DTO-lar, FluentValidation validator-ları, servis interfeysləri və biznes məntiqi.
  - `AltensorGlobalHealthCare.Persistence` — EF Core 8, Npgsql (PostgreSQL), Entity configurations, Repository Pattern, Unit of Work, Database Seeder.
  - `AltensorGlobalHealthCare.Infrastructure` — JWT Token servisi, Şifrələmə, MailKit (SMTP), Telegram Bot API, Fayl saxlama (`LocalFileStorageService` + Qorunan `App_Data/medical-docs`), Arxa plan xidmətləri (`OfferExpirationJob`).
  - `AltensorGlobalHealthCare.WebApi` — Controller-lər, Global Exception Middleware, Request Language Middleware, Swagger (JWT Bearer dəstəkli).
- **Verilənlər Bazası:** PostgreSQL (EF Core + Npgsql)
- **Autentifikasiya:** ASP.NET Core Identity + JWT Bearer (Access Token & Refresh Token) + 6 rəqəmli email aktivasiya kodları
- **Dillər:** 3 dildə tam dəstək (`Az`, `En`, `Ru`)
- **Təhlükəsizlik:** Tibbi sənədlər `wwwroot`-dan kənarda, qorunan `App_Data/medical-docs/` qovluğunda saxlanılır və yalnız səlahiyyətli istifadəçilərə təhlükəsiz stream vasitəsilə təqdim olunur.

---

## 🔑 Əsas Funksionallıqlar

1. **İstifadəçi Qeydiyyatı və Email Təsdiqi:**
   - Qeydiyyat zamanı emailə 6 rəqəmli birdəfəlik kod göndərilir.
   - Hesab yalnız təsdiq kodu daxil edildikdən sonra aktivləşir.
2. **Tibbi Turizm Koordinasiyası ("All-in-One"):**
   - Pasient istədiyi prosedur üzrə müraciət göndərir (diaqnoz tələb olunmur).
   - Analiz, rentgen və MRT faylları təhlükəsiz yüklənir.
   - Pasient Koordinatoru müraciəti tərəfdaş xəstəxanalara və həkimlərə yönləndirir.
   - Klinikalar pasientə rəsmi qiymət cədvəli göndərir: Müalicə + Mehmanxana + Uçuş bileti + Aeroport transferi.
   - Pasient təklifi qəbul etdikdə digər təkliflər avtomatik ləğv edilir və səfər təşkil olunur.
3. **Müraciət Daxili Çat:**
   - Pasient, koordinator və həkim arasında birbaşa yazışma sistemi.
4. **Avtomatik Müddət İdarəetməsi:**
   - Fon rejimində işləyən `OfferExpirationJob` müddəti bitmiş təklifləri avtomatik `Expired` statusuna keçirir.
5. **Vahid Global Exception Handling:**
   - Bütün xətalar, validasiya problemləri və verilənlər bazası konfliktləri unikal `traceId` ilə vahid `ApiResponse` formatında qaytarılır.

---

## 🚀 Başlanğıc və Test Hesabları

Verilənlər bazası işə düşərkən sistem tərəfindən avtomatik olaraq ilkin rollar və demo hesablar yaradılır:

| Rol | E-poçt | Şifrə | Təyinatı |
|---|---|---|---|
| **Admin** | `admin@altensorglobalhealthcare.com` | `Admin123!` | Tam sistem nəzarəti |
| **Coordinator** | `coordinator@altensorglobalhealthcare.com` | `Coordinator123!` | Pasient koordinatoru (Ivana) |
| **Doctor** | `dr.marko@altensorglobalhealthcare.com` | `Doctor123!` | Patela klinikasının aparıcı ortopedi |
| **Doctor** | `dr.ana@altensorglobalhealthcare.com` | `Doctor123!` | Sinteza klinikasının plastik cərrahı |

---

## 🛠️ Layihəni İşə Salmaq

### 1. Lokal mühitdə:
```bash
# PostgreSQL bazasını işə salın
# appsettings.json faylında ConnectionStrings yoxlayın
dotnet run --project src/AltensorGlobalHealthCare.WebApi/AltensorGlobalHealthCare.WebApi.csproj
```
Brauzerdə açın: `http://localhost:5000/` (Swagger UI birbaşa root-da açılacaq).

### 2. Docker vasitəsilə:
```bash
docker-compose up -d --build
```

---

## 📑 Sənədləşmə
- Ətraflı frontend inteqrasiya təlimatı üçün: **`FRONTEND_DEVELOPER_GUIDE.md`** faylına baxın.
- Bütün endpoint-lərin icra edilə bilən HTTP sorğuları üçün: **`src/AltensorGlobalHealthCare.WebApi/AltensorGlobalHealthCare.WebApi.http`** faylından istifadə edin.

