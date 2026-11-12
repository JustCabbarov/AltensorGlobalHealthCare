using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;
using AltensorGlobalHealthCare.Persistence.Contexts;

namespace AltensorGlobalHealthCare.Persistence.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // 1. Roles
        string[] roles = { "Admin", "Coordinator", "Doctor", "Patient" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                await roleManager.CreateAsync(new ApplicationRole(r));
            }
        }

        // 2. SuperAdmin User
        var adminEmail = "admin@altensorglobalhealthcare.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Sistem",
                LastName = "İnzibatçısı",
                PhoneNumber = "+38514898555",
                Country = "Xorvatiya",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 3. Coordinator User (Ivana)
        var coordinatorEmail = "coordinator@altensorglobalhealthcare.com";
        var coordinatorUser = await userManager.FindByEmailAsync(coordinatorEmail);
        if (coordinatorUser == null)
        {
            coordinatorUser = new ApplicationUser
            {
                UserName = coordinatorEmail,
                Email = coordinatorEmail,
                FirstName = "Ivana",
                LastName = "Šajn",
                PhoneNumber = "+385993875702",
                Country = "Xorvatiya",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(coordinatorUser, "Coordinator123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
            }
        }

        // 4. Partner Clinics
        if (!await context.Clinics.AnyAsync())
        {
            var medikol = new Clinic
            {
                Name = "Medikol Polyclinic",
                Slug = "medikol",
                DescriptionAz = "Xorvatiyanın ən böyük özəl diaqnostika və tibbi mərkəzlərindən biri. PET/CT, MRT və ən son radiologiya avadanlıqları ilə təchiz olunub.",
                DescriptionEn = "One of the largest private diagnostic and medical clinics in Croatia, equipped with state-of-the-art PET/CT and MRI technology.",
                DescriptionRu = "Один из крупнейших частных диагностических центров Хорватии с новейшими аппаратами ПЭТ/КТ и МРТ.",
                City = "Zagreb",
                Address = "Voćarska cesta 106, 10000 Zagreb",
                Phone = "+38514585888",
                Email = "info@medikol.hr",
                WebsiteUrl = "https://medikol.hr",
                DisplayOrder = 1,
                IsActive = true
            };

            var sinteza = new Clinic
            {
                Name = "Poliklinika Sinteza",
                Slug = "sinteza",
                DescriptionAz = "Plastik cərrahiyyə, estetika və qabaqcıl cərrahiyyə üzrə ixtisaslaşmış aparıcı klinika.",
                DescriptionEn = "Leading clinic specializing in aesthetic surgery, plastic surgery, and advanced outpatient care.",
                DescriptionRu = "Ведущая клиника эстетической и пластической хирургии в Загребе.",
                City = "Zagreb",
                Address = "Ulica Frana Folnegovića 1, 10000 Zagreb",
                Phone = "+38515005970",
                Email = "narucivanje@sinteza.hr",
                WebsiteUrl = "https://sinteza.hr",
                DisplayOrder = 2,
                IsActive = true
            };

            var patela = new Clinic
            {
                Name = "Poliklinika Patela",
                Slug = "patela",
                DescriptionAz = "Ortopediya, oynaq zədələri, idman təbabəti və reabilitasiya mərkəzi.",
                DescriptionEn = "Specialized center for orthopaedics, sports medicine, joint treatment, and advanced physical rehabilitation.",
                DescriptionRu = "Центр ортопедии, спортивной медицины и физической реабилитации суставов.",
                City = "Zagreb",
                Address = "Petrova ulica 119, 10000 Zagreb",
                Phone = "+38512345678",
                Email = "info@patela.hr",
                WebsiteUrl = "https://patela.hr",
                DisplayOrder = 3,
                IsActive = true
            };

            var ivfZagreb = new Clinic
            {
                Name = "Poliklinika IVF Zagreb",
                Slug = "ivf-zagreb",
                DescriptionAz = "Süni mayalanma (IVF), sonsuzluğun müalicəsi və ginekoloji sağlamlıq mərkəzi.",
                DescriptionEn = "Pioneer IVF center in Croatia dedicated to reproductive health, fertility treatments, and gynecology.",
                DescriptionRu = "Первый центр ЭКО в Хорватии, специализирующийся на лечении бесплодия и женском здоровье.",
                City = "Zagreb",
                Address = "Voćarska cesta 14, 10000 Zagreb",
                Phone = "+38514678600",
                Email = "info@ivf.hr",
                WebsiteUrl = "https://ivf.hr",
                DisplayOrder = 4,
                IsActive = true
            };

            await context.Clinics.AddRangeAsync(medikol, sinteza, patela, ivfZagreb);
            await context.SaveChangesAsync();

            // 5. Seed Doctors for clinics
            var drMarko = new ApplicationUser
            {
                UserName = "dr.marko@altensorglobalhealthcare.com",
                Email = "dr.marko@altensorglobalhealthcare.com",
                FirstName = "Marko",
                LastName = "Kovač",
                PhoneNumber = "+385912345678",
                Country = "Xorvatiya",
                EmailConfirmed = true
            };
            if (await userManager.FindByEmailAsync(drMarko.Email) == null)
            {
                await userManager.CreateAsync(drMarko, "Doctor123!");
                await userManager.AddToRoleAsync(drMarko, "Doctor");

                context.Doctors.Add(new Doctor
                {
                    ClinicId = patela.Id,
                    UserId = drMarko.Id,
                    FullName = "Prof. Dr. Marko Kovač",
                    TitleAz = "Ortoped-Travmatoloq",
                    TitleEn = "Orthopaedic Surgeon",
                    TitleRu = "Ортопед-травматолог",
                    SpecializationAz = "Diz və bud-çanaq oynağının endoprotezləşdirilməsi",
                    SpecializationEn = "Knee & Hip Arthroplasty, Sports Injuries",
                    SpecializationRu = "Эндопротезирование коленного и тазобедренного суставов",
                    BioAz = "20 ildən çox cərrahi təcrübə, 3000-dən artıq uğurlu oynaq əməliyyatı.",
                    ExperienceYears = 22,
                    Phone = "+385912345678",
                    Email = drMarko.Email,
                    DisplayOrder = 1,
                    IsActive = true
                });
            }

            var drAna = new ApplicationUser
            {
                UserName = "dr.ana@altensorglobalhealthcare.com",
                Email = "dr.ana@altensorglobalhealthcare.com",
                FirstName = "Ana",
                LastName = "Horvat",
                PhoneNumber = "+385915556677",
                Country = "Xorvatiya",
                EmailConfirmed = true
            };
            if (await userManager.FindByEmailAsync(drAna.Email) == null)
            {
                await userManager.CreateAsync(drAna, "Doctor123!");
                await userManager.AddToRoleAsync(drAna, "Doctor");

                context.Doctors.Add(new Doctor
                {
                    ClinicId = sinteza.Id,
                    UserId = drAna.Id,
                    FullName = "Dr. Ana Horvat",
                    TitleAz = "Plastik və Estetik Cərrah",
                    TitleEn = "Plastic & Aesthetic Surgeon",
                    TitleRu = "Пластический и эстетический хирург",
                    SpecializationAz = "Rinoplastika, üz cavanlaşdırma və bədən şəkilləndirmə",
                    SpecializationEn = "Rhinoplasty, Facial Rejuvenation & Body Contouring",
                    SpecializationRu = "Ринопластика и контурная пластика тела",
                    BioAz = "Avropa Plastik Cərrahiyyə Cəmiyyətinin fəal üzvü, beynəlxalq mühazirəçi.",
                    ExperienceYears = 15,
                    Phone = "+385915556677",
                    Email = drAna.Email,
                    DisplayOrder = 1,
                    IsActive = true
                });
            }

            await context.SaveChangesAsync();
        }

        // 6. Service Categories & Medical Services
        if (!await context.ServiceCategories.AnyAsync())
        {
            var orthopaedics = new ServiceCategory
            {
                Slug = "orthopaedics",
                TitleAz = "Ortopediya",
                TitleEn = "Orthopaedics",
                TitleRu = "Ортопедия",
                DescriptionAz = "Oynaq, sümük və əzələ sisteminin müasir cərrahi və konservativ müalicəsi.",
                IconKey = "ortho",
                DisplayOrder = 1
            };

            var plasticSurgery = new ServiceCategory
            {
                Slug = "plastic-surgery",
                TitleAz = "Plastik Cərrahiyyə",
                TitleEn = "Plastic Surgery",
                TitleRu = "Пластическая хирургия",
                DescriptionAz = "Estetik və rekonstruktiv cərrahiyyənin bütün növləri.",
                IconKey = "plastic",
                DisplayOrder = 2
            };

            var dentistry = new ServiceCategory
            {
                Slug = "dentistry",
                TitleAz = "Stomatologiya",
                TitleEn = "Dentistry",
                TitleRu = "Стоматология",
                DescriptionAz = "İmplantasiya, estetik diş həkimliyi və all-on-4/6 protezləşdirmə.",
                IconKey = "dental",
                DisplayOrder = 3
            };

            var dermatology = new ServiceCategory
            {
                Slug = "dermatology",
                TitleAz = "Dermatologiya",
                TitleEn = "Dermatology",
                TitleRu = "Дерматология",
                DescriptionAz = "Dəri xəstəliklərinin diaqnostikası, lazer prosedurları və anti-ageing.",
                IconKey = "derma",
                DisplayOrder = 4
            };

            await context.ServiceCategories.AddRangeAsync(orthopaedics, plasticSurgery, dentistry, dermatology);
            await context.SaveChangesAsync();

            // Subcategories & Services for Orthopaedics
            var kneeSub = new ServiceCategory
            {
                ParentId = orthopaedics.Id,
                Slug = "knee",
                TitleAz = "Diz oynağı",
                TitleEn = "Knee",
                TitleRu = "Коленный сустав",
                DisplayOrder = 1
            };
            var hipSub = new ServiceCategory
            {
                ParentId = orthopaedics.Id,
                Slug = "hip",
                TitleAz = "Bud-çanaq oynağı",
                TitleEn = "Hip",
                TitleRu = "Тазобедренный сустав",
                DisplayOrder = 2
            };

            await context.ServiceCategories.AddRangeAsync(kneeSub, hipSub);
            await context.SaveChangesAsync();

            var kneeReplacement = new MedicalService
            {
                CategoryId = kneeSub.Id,
                Slug = "total-knee-replacement",
                TitleAz = "Diz oynağının tam endoprotezləşdirilməsi",
                TitleEn = "Total Knee Replacement",
                TitleRu = "Тотальное эндопротезирование коленного сустава",
                AboutAz = "Diz oynağının zədələnmiş qığırdaq və sümük səthlərinin biouyğun titan/keramika protezlə əvəzlənməsi.",
                PreparationAz = "Əməliyyatdan öncə laborator analizlər, rentgen/MRT müayinəsi və anestezioloq konsultasiyası.",
                StepsAz = "1. Minimal invaziv kəsik; 2. Zədəli toxumaların təmizlənməsi; 3. Protezin fiksasiyası; 4. Reabilitasiyanın 1-ci günü hərəkət.",
                ResultAz = "Ağrısız tam hərəkətlilik, 20-25 il protez ömrü.",
                DurationText = "3-5 gün xəstəxana, 2 həftə bərpa",
                IsMostSearched = true,
                DisplayOrder = 1
            };

            var hipReplacement = new MedicalService
            {
                CategoryId = hipSub.Id,
                Slug = "direct-anterior-hip-replacement",
                TitleAz = "Direkt ön girişlə bud-çanaq endoprotezləşdirilməsi",
                TitleEn = "Direct Anterior Hip Replacement",
                TitleRu = "Эндопротезирование тазобедренного сустава прямым передним доступом",
                AboutAz = "Əzələləri kəsmədən icra olunan ən qabaqcıl cərrahi yanaşma. Xəstə ertəsi gün sərbəst yeriyir.",
                DurationText = "3 gün xəstəxana, 10 gün reabilitasiya",
                IsMostSearched = true,
                DisplayOrder = 2
            };

            // Rhinoplasty for Plastic Surgery
            var rhinoplasty = new MedicalService
            {
                CategoryId = plasticSurgery.Id,
                Slug = "rhinoplasty",
                TitleAz = "Rinoplastika (Burun plastikası)",
                TitleEn = "Rhinoplasty (Nose Surgery)",
                TitleRu = "Ринопластика (Пластика носа)",
                AboutAz = "Həm estetik forma korreksiyası, həm də çəpər əyriliyi və nəfəsalma problemlərinin eyni vaxtda aradan qaldırılması.",
                DurationText = "1 gün xəstəxana, 7 gün gips",
                IsMostSearched = true,
                DisplayOrder = 1
            };

            // Dental Implants for Dentistry
            var dentalImplants = new MedicalService
            {
                CategoryId = dentistry.Id,
                Slug = "dental-implants",
                TitleAz = "Dental İmplantasiya (Straumann / Nobel Biocare)",
                TitleEn = "Dental Implants",
                TitleRu = "Дентальная имплантация",
                AboutAz = "İtirilmiş dişlərin premium İsveçrə implantları ilə tam bərpası.",
                DurationText = "2-3 gün birinci mərhələ",
                IsMostSearched = true,
                DisplayOrder = 1
            };

            await context.MedicalServices.AddRangeAsync(kneeReplacement, hipReplacement, rhinoplasty, dentalImplants);
            await context.SaveChangesAsync();

            // Link Clinic Prices
            var patelaClinic = await context.Clinics.FirstAsync(c => c.Slug == "patela");
            var sintezaClinic = await context.Clinics.FirstAsync(c => c.Slug == "sinteza");

            context.ClinicServices.AddRange(
                new ClinicService { ClinicId = patelaClinic.Id, MedicalServiceId = kneeReplacement.Id, PriceFrom = 6800, Currency = "EUR" },
                new ClinicService { ClinicId = patelaClinic.Id, MedicalServiceId = hipReplacement.Id, PriceFrom = 7200, Currency = "EUR" },
                new ClinicService { ClinicId = sintezaClinic.Id, MedicalServiceId = rhinoplasty.Id, PriceFrom = 3200, Currency = "EUR" }
            );

            // 7. Sample All-in-One Packages
            context.TreatmentPackages.AddRange(
                new TreatmentPackage
                {
                    Slug = "all-in-one-knee-package",
                    TitleAz = "Tam Diz Əməliyyatı və Reabilitasiya Paketi",
                    TitleEn = "All-in-One Total Knee Replacement Package",
                    TitleRu = "Пакет 'Все включено' по замене коленного сустава",
                    DescriptionAz = "Əməliyyat, 5 ulduzlu hotel gecələməsi, aeroport transferləri və 7 günlük fərdi fizioterapiya.",
                    IncludesAz = "Cərrahi əməliyyat, İsveçrə protezi, 5 gün klinika, 7 gün 5* hotel, fərdi transferlər, koordinator dəstəyi.",
                    PriceFrom = 8500,
                    Currency = "EUR",
                    MedicalServiceId = kneeReplacement.Id,
                    DisplayOrder = 1
                },
                new TreatmentPackage
                {
                    Slug = "all-in-one-smile-makeover",
                    TitleAz = "Zaqreb Hollivud Təbəssümü Paketi",
                    TitleEn = "Zagreb Smile Makeover All-in-One",
                    TitleRu = "Пакет 'Голливудская улыбка' в Загребе",
                    DescriptionAz = "10 ədəd e.max vinir, hotel və hava limanı qarşılanması daxil xüsusi paket.",
                    IncludesAz = "3D təbəssüm dizaynı, 10 ədəd keramik vinir, 5 gün otel, transferlər.",
                    PriceFrom = 3900,
                    Currency = "EUR",
                    MedicalServiceId = dentalImplants.Id,
                    DisplayOrder = 2
                }
            );

            // 8. CMS Pages
            context.CmsPages.AddRange(
                new CmsPage
                {
                    Slug = "about-us",
                    TitleAz = "Haqqımızda — AltensorGlobalHealthCare",
                    TitleEn = "About Us — AltensorGlobalHealthCare",
                    TitleRu = "О нас — AltensorGlobalHealthCare",
                    HeadingAz = "Zaqreb Tibbi Mərkəzi: Beynəlxalq Pasientlər üçün Qabaqcıl Səhiyyə Xidmətləri",
                    HeadingEn = "About ZagrebMed: Connecting Patients with Private Healthcare in Zagreb and Croatia",
                    HeadingRu = "ZagrebMed: Частная медицина в Загребе для международных пациентов",
                    ContentAz = "AltensorGlobalHealthCare Xorvatiyanın paytaxtı Zaqrebdə yerləşən aparıcı özəl xəstəxana və klinikaların rəsmi tərəfdaşlıq platformasıdır. Biz hər bir pasientə unikal yanaşır, ilk müraciətdən reabilitasiyanın son gününə qədər tam pulsuz fərdi koordinator dəstəyi göstəririk.",
                    ContentEn = "AltensorGlobalHealthCare is a leading network of private medical providers in Zagreb, Croatia. We enable equitable, world-class healthcare with dedicated patient coordinator assistance from your first inquiry to complete recovery.",
                    ContentRu = "AltensorGlobalHealthCare объединяет ведущие частные клиники Загреба, предлагая персонализированную медицинскую помощь высочайшего европейского уровня."
                },
                new CmsPage
                {
                    Slug = "how-it-works",
                    TitleAz = "Sistem Necə İşləyir?",
                    TitleEn = "How It Works",
                    TitleRu = "Как это работает",
                    HeadingAz = "İlk addımı atmaq üçün dəqiq diaqnoza ehtiyacınız yoxdur",
                    HeadingEn = "You don’t need a diagnosis to take the first step",
                    HeadingRu = "Вам не нужен точный диагноз, чтобы сделать первый шаг",
                    ContentAz = "1. Müraciət formunu doldurun və mövcud sənədlərinizi yükləyin.\n2. Baş pasient koordinatorumuz Ivana sizinlə əlaqə saxlasın və müraciəti ən uyğun həkimə yönləndirsin.\n3. Həkimlərin hazırladığı rəsmi təklifi və qiymət cədvəlini qəbul edin.\n4. Zaqrebə səfəriniz (otel, bilet, transfer) platforma tərəfindən təşkil edilsin.",
                    ContentEn = "1. Submit your inquiry with medical documents.\n2. Dedicated patient coordinator reviews your case.\n3. Receive personalized all-in-one treatment and travel offer.\n4. Seamless medical trip to Zagreb with flight and hotel discounts.",
                    ContentRu = "1. Отправьте запрос с документами.\n2. Координатор подберет лучшего специалиста.\n3. Получите официальное предложение.\n4. Организация поездки и лечение в Загребе."
                }
            );

            // 9. Partners
            context.Partners.AddRange(
                new Partner { Name = "Ministarstvo zdravstva (Səhiyyə Nazirliyi)", PartnerType = PartnerType.Institutional, DisplayOrder = 1 },
                new Partner { Name = "Grad Zagreb (Zaqreb Şəhər İcra Hakimiyyəti)", PartnerType = PartnerType.Institutional, DisplayOrder = 2 },
                new Partner { Name = "Turistička zajednica grada Zagreba (TZGZ)", PartnerType = PartnerType.Institutional, DisplayOrder = 3 },
                new Partner { Name = "Hrvatska gospodarska komora (HGK)", PartnerType = PartnerType.Institutional, DisplayOrder = 4 },
                new Partner { Name = "Croatia Airlines", WebsiteUrl = "https://croatiaairlines.com", PartnerType = PartnerType.Airline, DisplayOrder = 5 },
                new Partner { Name = "Obzor Holidays", WebsiteUrl = "https://obzorholidays.com", PartnerType = PartnerType.Accommodation, DisplayOrder = 6 }
            );

            // 10. FAQs
            context.FaqItems.AddRange(
                new FaqItem
                {
                    QuestionAz = "Müraciət etmək üçün xüsusi göndəriş və ya hazır diaqnoz lazımdırmı?",
                    AnswerAz = "Xeyr. Şikayətlərinizi və mövcud analiz/rentgen şəkillərinizi yükləməyiniz kifayətdir. Mütəxəssislərimiz müraciəti araşdırıb ilkin rəy verəcəklər.",
                    Category = "Ümumi",
                    DisplayOrder = 1
                },
                new FaqItem
                {
                    QuestionAz = "Pasient koordinatorunun xidmətləri ödənişlidirmi?",
                    AnswerAz = "Xeyr. Pasient koordinatoru (Ivana) tərəfindən göstərilən bütün konsultasiya, həkim seçimi və logistika dəstəyi pasientlər üçün tamamilə pulsuzdur.",
                    Category = "Xidmətlər",
                    DisplayOrder = 2
                },
                new FaqItem
                {
                    QuestionAz = "Təyyarə bileti və mehmanxana üçün endirimlər necə tətbiq olunur?",
                    AnswerAz = "Rəsmi tərəfdaşlarımız olan Croatia Airlines və yerli otellər vasitəsilə platforma pasientlərimizə 10-20% xüsusi endirimli tariflər təqdim edir.",
                    Category = "Səyahət",
                    DisplayOrder = 3
                }
            );

            await context.SaveChangesAsync();
        }
    }
}

