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

        // 11. Phase 6 Idempotent Dynamic Services & About Us Seed Data
        await SeedDynamicServicesAndAboutUsAsync(context);
    }

    private static async Task SeedDynamicServicesAndAboutUsAsync(ApplicationDbContext context)
    {
        // A. Root categories flags + Dentistry -> Dental Implants sub-category
        var rootSlugs = new[] { "orthopaedics", "plastic-surgery", "dentistry", "dermatology" };
        var roots = await context.ServiceCategories
            .Where(c => c.ParentId == null && rootSlugs.Contains(c.Slug))
            .ToListAsync();

        foreach (var root in roots)
        {
            root.ShowInHeaderNav = true;
            root.ShowOnHomePage = true;
        }

        var dentistry = roots.FirstOrDefault(c => c.Slug == "dentistry");
        var orthopaedics = roots.FirstOrDefault(c => c.Slug == "orthopaedics");

        ServiceCategory? dentalImplantsSub = null;
        if (dentistry != null)
        {
            dentalImplantsSub = await context.ServiceCategories
                .FirstOrDefaultAsync(c => c.ParentId == dentistry.Id && c.Slug == "dental-implants");

            if (dentalImplantsSub == null)
            {
                dentalImplantsSub = new ServiceCategory
                {
                    ParentId = dentistry.Id,
                    Slug = "dental-implants",
                    TitleAz = "Diş İmplantları",
                    TitleEn = "Dental Implants",
                    TitleRu = "Дентальные импланты",
                    DescriptionAz = "İsveçrə və Avropa istehsalı olan premium diş implantları ilə tam diş bərpası.",
                    DescriptionEn = "Complete tooth restoration with premium Swiss and European dental implants.",
                    DescriptionRu = "Полное восстановление зубов премиальными швейцарскими и европейскими имплантами.",
                    ShowInHeaderNav = true,
                    DisplayOrder = 1,
                    IsActive = true
                };
                await context.ServiceCategories.AddAsync(dentalImplantsSub);
                await context.SaveChangesAsync();
            }
        }

        // B. Enrich Total Knee Replacement & Dental Implants (3 languages + Candidate + RelatedPackage)
        var kneePkg = await context.TreatmentPackages.FirstOrDefaultAsync(p => p.Slug == "all-in-one-knee-package");
        var smilePkg = await context.TreatmentPackages.FirstOrDefaultAsync(p => p.Slug == "all-in-one-smile-makeover");

        var kneeService = await context.MedicalServices.FirstOrDefaultAsync(s => s.Slug == "total-knee-replacement");
        if (kneeService != null)
        {
            kneeService.AboutEn ??= "Replacement of damaged cartilage and bone surfaces of the knee joint with biocompatible titanium-ceramic prostheses using minimally invasive surgical techniques.";
            kneeService.AboutRu ??= "Замена поврежденных суставных поверхностей коленного сустава биосовместимым титано-керамическим протезом малоинвазивным методом.";
            kneeService.CandidateAz ??= "Ağır dərəcəli diz osteoartriti, davamlı ağrı və gündəlik hərəkət məhdudiyyəti olan, konservativ müalicədən nəticə almayan pasientlər.";
            kneeService.CandidateEn ??= "Patients experiencing severe knee osteoarthritis, persistent joint pain, stiffness, and limited mobility that has not responded to conservative therapy.";
            kneeService.CandidateRu ??= "Пациенты с тяжелым артрозом коленного сустава, хронической болью и ограничением подвижности при неэффективности консервативного лечения.";
            kneeService.PreparationEn ??= "Pre-operative blood tests, X-ray or MRI imaging, ECG, and anesthesiologist consultation.";
            kneeService.PreparationRu ??= "Предоперационные анализы крови, рентген или МРТ, ЭКГ и консультация анестезиолога.";
            kneeService.StepsEn ??= "1. Minimally invasive incision; 2. Removal of damaged cartilage; 3. Precision prosthesis placement; 4. Guided physiotherapy starting Day 1.";
            kneeService.StepsRu ??= "1. Малоинвазивный доступ; 2. Удаление поврежденного хряща; 3. Установка эндопротеза; 4. Начало реабилитации в первые сутки.";
            kneeService.PrecautionsAz ??= "İlk 4-6 həftə ərzində yüksək zərbəli idman növlərindən çəkinmək və həkim reabilitasiya proqramına əməl etmək tövsiyə olunur.";
            kneeService.PrecautionsEn ??= "Avoid high-impact activities during the initial 4–6 weeks and follow the prescribed physiotherapy protocol.";
            kneeService.PrecautionsRu ??= "Избегайте высоких ударных нагрузок в первые 4–6 недель и следуйте программе физиотерапии.";
            kneeService.ResultEn ??= "Pain-free full joint mobility and prosthesis longevity of 20–25 years.";
            kneeService.ResultRu ??= "Полная подвижность без боли и срок службы протеза 20–25 лет.";
            kneeService.PackageInfoAz ??= "Əməliyyat, İsveçrə protezi, 5 gün klinika stasionarı, 5* otel və hava limanı transferləri daxil paket mövcuddur.";
            kneeService.PackageInfoEn ??= "Available as an all-in-one package including surgery, Swiss implant, 5-night clinic stay, 5* hotel, and airport transfers.";
            kneeService.PackageInfoRu ??= "Доступно в рамках комплексного пакета: операция, швейцарский имплант, 5 ночей в клинике, отель 5* и трансферы.";
            kneeService.RelatedPackageId ??= kneePkg?.Id;
        }

        var dentalService = await context.MedicalServices.FirstOrDefaultAsync(s => s.Slug == "dental-implants");
        if (dentalService != null)
        {
            if (dentalImplantsSub != null)
            {
                dentalService.CategoryId = dentalImplantsSub.Id;
            }
            dentalService.AboutEn ??= "Permanent replacement of missing teeth using world-leading Straumann and Nobel Biocare titanium dental implants.";
            dentalService.AboutRu ??= "Надежное восстановление утраченных зубов с помощью премиальных титановых имплантов Straumann и Nobel Biocare.";
            dentalService.CandidateAz ??= "Bir və ya bir neçə dişini itirmiş, çənə sümüyü həcmi uyğun olan və uzunmüddətli estetik nəticə istəyən pasientlər.";
            dentalService.CandidateEn ??= "Patients missing one or multiple teeth with adequate jawbone density seeking a permanent, natural-looking restoration.";
            dentalService.CandidateRu ??= "Пациенты с отсутствием одного или нескольких зубов, желающие получить долговечное эстетическое восстановление.";
            dentalService.PreparationAz ??= "3D CBCT çənə tomoqrafiyası, ağız boşluğu müayinəsi və rəqəmsal gülüş planlaması.";
            dentalService.PreparationEn ??= "3D CBCT dental scan, comprehensive oral exam, and digital smile planning.";
            dentalService.PreparationRu ??= "3D КТ челюсти, осмотр полости рта и цифровое планирование улыбки.";
            dentalService.StepsAz ??= "1. Yerli anesteziya altında implantın yerləşdirilməsi; 2. Sağalma və osteointeqrasiya; 3. Zirkon/keramik qapağın fiksasiyası.";
            dentalService.StepsEn ??= "1. Guided implant placement under local anesthesia; 2. Healing and osseointegration; 3. Custom ceramic/zirconia crown placement.";
            dentalService.StepsRu ??= "1. Установка импланта под местной анестезией; 2. Заживление и остеоинтеграция; 3. Фиксация циркониевой/керамической коронки.";
            dentalService.PrecautionsAz ??= "İlk 48 saat ərzində isti qidalardan uzaq durmaq və gigiyena qaydalarına ciddi əməl etmək.";
            dentalService.PrecautionsEn ??= "Avoid hot foods and strenuous activity for the first 48 hours and maintain gentle oral hygiene.";
            dentalService.PrecautionsRu ??= "Избегайте горячей пищи в первые 48 часов и соблюдайте гигиену полости рта.";
            dentalService.ResultAz ??= "Təbii diş görünüşü, tam çeynəmə funksiyası və ömürlük implant zəmanəti.";
            dentalService.ResultEn ??= "Natural aesthetics, full chewing strength, and lifetime implant manufacturer warranty.";
            dentalService.ResultRu ??= "Естественная эстетика, полноценная жевательная функция и пожизненная гарантия на имплант.";
            dentalService.PackageInfoAz ??= "3D diaqnostika, otel və VIP transfer daxil stomatoloji paketlər mövcuddur.";
            dentalService.PackageInfoEn ??= "All-in-one dental tourism packages available including 3D diagnostics, hotel accommodation, and VIP transfers.";
            dentalService.PackageInfoRu ??= "Доступны стоматологические пакеты, включающие 3D-диагностику, проживание в отеле и трансфер.";
            dentalService.RelatedPackageId ??= smilePkg?.Id;
        }

        await context.SaveChangesAsync();

        // C. ClinicService price for Dental Implants + DoctorMedicalService links
        var medikolClinic = await context.Clinics.FirstOrDefaultAsync(c => c.Slug == "medikol");
        var patelaClinic = await context.Clinics.FirstOrDefaultAsync(c => c.Slug == "patela");
        var sintezaClinic = await context.Clinics.FirstOrDefaultAsync(c => c.Slug == "sinteza");

        if (dentalService != null && medikolClinic != null &&
            !await context.ClinicServices.AnyAsync(cs => cs.ClinicId == medikolClinic.Id && cs.MedicalServiceId == dentalService.Id))
        {
            context.ClinicServices.Add(new ClinicService
            {
                ClinicId = medikolClinic.Id,
                MedicalServiceId = dentalService.Id,
                PriceFrom = 850,
                Currency = "EUR",
                IsActive = true
            });
        }

        if (kneeService != null && patelaClinic != null &&
            !await context.ClinicServices.AnyAsync(cs => cs.ClinicId == patelaClinic.Id && cs.MedicalServiceId == kneeService.Id))
        {
            context.ClinicServices.Add(new ClinicService
            {
                ClinicId = patelaClinic.Id,
                MedicalServiceId = kneeService.Id,
                PriceFrom = 6800,
                Currency = "EUR",
                IsActive = true
            });
        }

        var markoDoctor = await context.Doctors.FirstOrDefaultAsync(d => d.Email == "dr.marko@altensorglobalhealthcare.com");
        var anaDoctor = await context.Doctors.FirstOrDefaultAsync(d => d.Email == "dr.ana@altensorglobalhealthcare.com");
        var hipService = await context.MedicalServices.FirstOrDefaultAsync(s => s.Slug == "direct-anterior-hip-replacement");
        var rhinoService = await context.MedicalServices.FirstOrDefaultAsync(s => s.Slug == "rhinoplasty");

        if (markoDoctor != null && kneeService != null &&
            !await context.DoctorMedicalServices.AnyAsync(dm => dm.DoctorId == markoDoctor.Id && dm.MedicalServiceId == kneeService.Id))
        {
            context.DoctorMedicalServices.Add(new DoctorMedicalService
            {
                DoctorId = markoDoctor.Id,
                MedicalServiceId = kneeService.Id,
                DisplayOrder = 1
            });
        }

        if (markoDoctor != null && hipService != null &&
            !await context.DoctorMedicalServices.AnyAsync(dm => dm.DoctorId == markoDoctor.Id && dm.MedicalServiceId == hipService.Id))
        {
            context.DoctorMedicalServices.Add(new DoctorMedicalService
            {
                DoctorId = markoDoctor.Id,
                MedicalServiceId = hipService.Id,
                DisplayOrder = 2
            });
        }

        if (anaDoctor != null && rhinoService != null &&
            !await context.DoctorMedicalServices.AnyAsync(dm => dm.DoctorId == anaDoctor.Id && dm.MedicalServiceId == rhinoService.Id))
        {
            context.DoctorMedicalServices.Add(new DoctorMedicalService
            {
                DoctorId = anaDoctor.Id,
                MedicalServiceId = rhinoService.Id,
                DisplayOrder = 1
            });
        }

        if (anaDoctor != null && dentalService != null &&
            !await context.DoctorMedicalServices.AnyAsync(dm => dm.DoctorId == anaDoctor.Id && dm.MedicalServiceId == dentalService.Id))
        {
            context.DoctorMedicalServices.Add(new DoctorMedicalService
            {
                DoctorId = anaDoctor.Id,
                MedicalServiceId = dentalService.Id,
                DisplayOrder = 1
            });
        }

        await context.SaveChangesAsync();

        // D. Procedure-level FAQs
        if (kneeService != null && !await context.FaqItems.AnyAsync(f => f.MedicalServiceId == kneeService.Id))
        {
            context.FaqItems.AddRange(
                new FaqItem
                {
                    MedicalServiceId = kneeService.Id,
                    QuestionAz = "Diz endoprotezləşdirilməsindən sonra nə vaxt yeriməyə başlaya bilərəm?",
                    QuestionEn = "When can I start walking after total knee replacement?",
                    QuestionRu = "Когда я смогу начать ходить после эндопротезирования коленного сустава?",
                    AnswerAz = "Minimal invaziv cərrahi metod sayəsində pasientlər əməliyyatdan sonrakı ilk 24 saat ərzində fizioterapevtin dəstəyi ilə ayağa qalxıb yeriməyə başlayırlar.",
                    AnswerEn = "With minimally invasive protocols, most patients stand and begin assisted walking within 24 hours of surgery.",
                    AnswerRu = "Благодаря малоинвазивному методу большинство пациентов начинают вставать и ходить с поддержкой в течение первых 24 часов.",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new FaqItem
                {
                    MedicalServiceId = kneeService.Id,
                    QuestionAz = "Zaqrebdə ümumi qalma müddəti nə qədərdir?",
                    QuestionEn = "How long do I need to stay in Zagreb?",
                    QuestionRu = "Сколько времени необходимо оставаться в Загребе?",
                    AnswerAz = "Adətən 3-5 gün klinika stasionarı və 7-10 gün ambulator reabilitasiya daxil olmaqla ümumi 10-14 gün kifayət edir.",
                    AnswerEn = "A typical stay is 10–14 days, including 3–5 days of inpatient care followed by supervised early rehabilitation.",
                    AnswerRu = "Обычно требуется 10–14 дней, включая 3–5 дней стационара и начальную реабилитацию.",
                    DisplayOrder = 2,
                    IsActive = true
                }
            );
        }

        if (dentalService != null && !await context.FaqItems.AnyAsync(f => f.MedicalServiceId == dentalService.Id))
        {
            context.FaqItems.Add(new FaqItem
            {
                MedicalServiceId = dentalService.Id,
                QuestionAz = "Diş implantasiyası ağrılı prosedurudurmu?",
                QuestionEn = "Is the dental implant procedure painful?",
                QuestionRu = "Болезненна ли процедура дентальной имплантации?",
                AnswerAz = "Prosedur müasir yerli anesteziya altında tam ağrısız icra olunur. Əməliyyatdan sonra yüngül həssaslıq standart ağrıkəsicilərlə rahatlıqla idarə olunur.",
                AnswerEn = "The procedure is performed under effective local anesthesia and is painless. Mild post-procedure soreness is easily managed with standard pain relief.",
                AnswerRu = "Процедура проводится под современной местной анестезией и абсолютно безболезненна.",
                DisplayOrder = 1,
                IsActive = true
            });
        }

        // E. CMS Pages for About Us ecosystem
        await UpsertSeedPageAsync(context, new CmsPage
        {
            Slug = "about-us",
            ParentSlug = null,
            PageType = "AboutOverview",
            TitleAz = "Haqqımızda — AltensorGlobalHealthCare",
            TitleEn = "About Us — AltensorGlobalHealthCare",
            TitleRu = "О нас — AltensorGlobalHealthCare",
            HeadingAz = "Zaqreb və Xorvatiyada qabaqcıl özəl səhiyyəyə birbaşa çıxış",
            HeadingEn = "Connecting International Patients with Leading Private Healthcare in Zagreb",
            HeadingRu = "Прямой доступ к ведущим частным клиникам Загреба и Хорватии",
            SubtitleAz = "Etibarlı klinikalar, beynəlxalq standartlar və ödənişsiz fərdi koordinator dəstəyi.",
            SubtitleEn = "Accredited partner clinics, transparent pricing, and complimentary dedicated coordination.",
            SubtitleRu = "Аккредитованные клиники, прозрачные цены и бесплатная поддержка координатора.",
            ContentAz = "AltensorGlobalHealthCare Xorvatiyanın paytaxtı Zaqrebdə yerləşən aparıcı özəl klinika və xəstəxanaları birləşdirən rəqəmsal tibbi turizm platformasıdır. İlk müraciətdən tam sağalmaya qədər hər addımda yanınızdayıq.",
            ContentEn = "AltensorGlobalHealthCare connects international patients with top accredited private clinics and renowned medical specialists in Zagreb, Croatia, providing seamless coordination from initial inquiry to recovery.",
            ContentRu = "AltensorGlobalHealthCare объединяет ведущие аккредитованные частные клиники Загреба и обеспечивает полное сопровождение иностранных пациентов.",
            MissionTitleAz = "Missiyamız",
            MissionTitleEn = "Our Mission",
            MissionTitleRu = "Наша миссия",
            MissionTextAz = "Dünyanın hər yerindən pasientlərə gözləmə siyahısı olmadan, şəffaf qiymətlərlə və tam fərdi dəstəklə Avropa səviyyəli tibbi xidməti əlçatan etmək.",
            MissionTextEn = "To make world-class European medical care accessible without waiting lists, hidden fees, or language barriers through trusted personalized coordination.",
            MissionTextRu = "Сделать европейскую медицину мирового уровня доступной без очередей, скрытых платежей и языковых барьеров.",
            ShowInMenu = false,
            DisplayOrder = 0,
            IsPublished = true
        });

        await UpsertSeedPageAsync(context, new CmsPage
        {
            Slug = "how-it-works",
            ParentSlug = "about-us",
            PageType = "HowItWorks",
            TitleAz = "Sistem Necə İşləyir",
            TitleEn = "How It Works",
            TitleRu = "Как это работает",
            HeadingAz = "İlk addımı atmaq üçün hazır diaqnoza ehtiyacınız yoxdur",
            HeadingEn = "You don’t need a diagnosis to take the first step",
            HeadingRu = "Вам не нужен точный диагноз, чтобы сделать первый шаг",
            SubtitleAz = "Müraciətdən sağalmaya qədər 6 sadə addım",
            SubtitleEn = "6 transparent steps from inquiry to full recovery",
            SubtitleRu = "6 простых шагов от заявки до выздоровления",
            ContentAz = "Simptomlarınızı və mövcud sənədlərinizi paylaşın, koordinatorunuz bütün tibbi və səyahət prosesini sizin üçün planlaşdırsın.",
            ContentEn = "Share your symptoms and existing medical records, and our dedicated coordinator handles specialist review, clinic offers, flights, and hotel arrangements.",
            ContentRu = "Отправьте ваши жалобы и медицинские документы, а ваш персональный координатор организует консультацию, лечение и поездку.",
            ShowInMenu = true,
            DisplayOrder = 1,
            IsPublished = true
        });

        await UpsertSeedPageAsync(context, new CmsPage
        {
            Slug = "airline-partner",
            ParentSlug = "about-us",
            PageType = "Standard",
            TitleAz = "Aviaşirkət Tərəfdaşımız",
            TitleEn = "Airline Partner",
            TitleRu = "Авиапартнер",
            HeadingAz = "Croatia Airlines ilə Xüsusi Tibbi Səyahət Tarifləri",
            HeadingEn = "Special Medical Travel Fares with Croatia Airlines",
            HeadingRu = "Специальные тарифы на перелет с Croatia Airlines",
            SubtitleAz = "Pasientlərimiz və müşayiətçiləri üçün güzəştli uçuşlar",
            SubtitleEn = "Preferential flight conditions for patients and accompanying persons",
            SubtitleRu = "Льготные условия перелета для пациентов и сопровождающих",
            ContentAz = "Rəsmi aviaşirkət tərəfdaşımız Croatia Airlines vasitəsilə AltensorGlobalHealthCare pasientləri Zaqrebə uçuşlarda xüsusi endirimlərdən və çevik bilet dəyişikliyi şərtlərindən yararlanırlar.",
            ContentEn = "Through our official partnership with Croatia Airlines, AltensorGlobalHealthCare patients and their companions benefit from dedicated medical travel discounts and flexible booking options to Zagreb.",
            ContentRu = "Благодаря партнерству с Croatia Airlines пациенты получают специальные скидки и гибкие условия бронирования авиабилетов в Загреб.",
            ShowInMenu = true,
            DisplayOrder = 2,
            IsPublished = true
        });

        await UpsertSeedPageAsync(context, new CmsPage
        {
            Slug = "obzor-holidays-travel-partner",
            ParentSlug = "about-us",
            PageType = "Standard",
            TitleAz = "Səyahət Tərəfdaşı — Obzor Holidays",
            TitleEn = "Obzor Holidays Travel Partner",
            TitleRu = "Туристический партнер — Obzor Holidays",
            HeadingAz = "Obzor Holidays ilə Rahat Mehmanxana və Transfer Təşkili",
            HeadingEn = "Seamless Hotel & Transfer Arrangements with Obzor Holidays",
            HeadingRu = "Комфортное проживание и трансфер с Obzor Holidays",
            SubtitleAz = "Klinikalara yaxın 4* və 5* otellər və fərdi transfer",
            SubtitleEn = "Carefully selected 4* & 5* hotels near clinics and private transfers",
            SubtitleRu = "Отели 4* и 5* рядом с клиниками и индивидуальный трансфер",
            ContentAz = "Səyahət tərəfdaşımız Obzor Holidays pasientlərin sağalma dövrü üçün komfortlu otel yerləşdirməsi, hava limanı qarşılanması və Zaqreb daxili transferləri təmin edir.",
            ContentEn = "Our official travel partner Obzor Holidays arranges comfortable accommodation close to partner clinics, VIP airport transfers, and tailored city assistance throughout your stay.",
            ContentRu = "Наш партнер Obzor Holidays организует комфортное проживание рядом с клиниками и трансфер из аэропорта.",
            ShowInMenu = true,
            DisplayOrder = 3,
            IsPublished = true
        });

        await UpsertSeedPageAsync(context, new CmsPage
        {
            Slug = "official-editorial-partner",
            ParentSlug = "about-us",
            PageType = "Standard",
            TitleAz = "Rəsmi Redaksiya Tərəfdaşı",
            TitleEn = "Official Editorial Partner",
            TitleRu = "Официальный редакционный партнер",
            HeadingAz = "Təsdiqlənmiş Tibbi Məzmun və Ekspert Məqalələri",
            HeadingEn = "Medically Reviewed Health Information & Editorial Standards",
            HeadingRu = "Проверенная медицинская информация и стандарты редакции",
            SubtitleAz = "Elmi əsaslı və pasient yönümlü tibbi bələdçilər",
            SubtitleEn = "Evidence-based patient guides reviewed by clinical specialists",
            SubtitleRu = "Научно обоснованные руководства для пациентов",
            ContentAz = "Platformadakı bütün prosedur təsvirləri və sağlamlıq məqalələri rəsmi tibbi redaksiya tərəfdaşlarımız və ixtisaslı həkimlər tərəfindən yoxlanılaraq dərc edilir.",
            ContentEn = "All procedure descriptions and health guides published on our platform undergo rigorous medical editorial review by licensed specialists.",
            ContentRu = "Все описания процедур и статьи проходят обязательную проверку медицинскими экспертами.",
            ShowInMenu = true,
            DisplayOrder = 4,
            IsPublished = true
        });

        await UpsertSeedPageAsync(context, new CmsPage
        {
            Slug = "contact",
            ParentSlug = "about-us",
            PageType = "Contact",
            TitleAz = "Əlaqə",
            TitleEn = "Contact",
            TitleRu = "Контакты",
            HeadingAz = "Komandamızla əlaqə saxlayın",
            HeadingEn = "Get in Touch with Our Patient Support Team",
            HeadingRu = "Свяжитесь с нашей командой поддержки пациентов",
            SubtitleAz = "Suallarınızı ünvanlayın — koordinatorunuz 24 saat ərzində cavab verəcək",
            SubtitleEn = "Send us a message and our coordinator will respond within 24 hours",
            SubtitleRu = "Напишите нам — координатор ответит в течение 24 часов",
            ContentAz = "Müalicə prosedurları, qiymət təklifləri və Zaqrebə tibbi səyahət barədə bütün suallarınız üçün əlaqə formasından və ya WhatsApp xəttindən istifadə edə bilərsiniz.",
            ContentEn = "Reach out via our contact form, email, or direct WhatsApp line for questions regarding treatments, pricing, or medical travel planning in Zagreb.",
            ContentRu = "Свяжитесь с нами через форму обратной связи, по электронной почте или WhatsApp по любым вопросам о лечении в Загребе.",
            ShowInMenu = true,
            DisplayOrder = 5,
            IsPublished = true
        });

        // F. 6 About Pillars
        if (!await context.AboutPillars.AnyAsync())
        {
            context.AboutPillars.AddRange(
                new AboutPillar
                {
                    TitleAz = "İstifadəsi asan",
                    TitleEn = "Easy to use",
                    TitleRu = "Удобство использования",
                    DescriptionAz = "Sadə müraciət forması ilə bir neçə dəqiqəyə bütün tibbi sənədlərinizi təqdim edin.",
                    DescriptionEn = "Submit your inquiry and upload existing medical records online in just a few minutes.",
                    DescriptionRu = "Отправьте заявку и загрузите медицинские документы онлайн всего за несколько минут.",
                    IconKey = "easy",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new AboutPillar
                {
                    TitleAz = "Tam dəstək",
                    TitleEn = "Supported",
                    TitleRu = "Полная поддержка",
                    DescriptionAz = "Fərdi pasient koordinatorunuz ilk sorğudan sağalmaya qədər bütün mərhələlərdə yanınızdadır.",
                    DescriptionEn = "Your dedicated patient coordinator assists you at every step from inquiry to recovery.",
                    DescriptionRu = "Персональный координатор сопровождает вас на каждом этапе от заявки до выздоровления.",
                    IconKey = "supported",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new AboutPillar
                {
                    TitleAz = "Fərdi yanaşma",
                    TitleEn = "Customized",
                    TitleRu = "Индивидуальный подход",
                    DescriptionAz = "Sağlamlıq vəziyyətinizə uyğun xüsusi müalicə planı, otel və transfer paketi hazırlanır.",
                    DescriptionEn = "Tailored medical treatment plans, clinic options, and travel packages built around your needs.",
                    DescriptionRu = "Индивидуальный план лечения, подбор клиники и организация поездки под ваши задачи.",
                    IconKey = "customized",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new AboutPillar
                {
                    TitleAz = "Mühafizə olunan",
                    TitleEn = "Protected",
                    TitleRu = "Защита данных",
                    DescriptionAz = "Bütün tibbi sənədləriniz və şəxsi məlumatlarınız Avropa GDPR standartlarına uyğun qorunur.",
                    DescriptionEn = "All medical documents and personal data are handled confidentially under strict GDPR standards.",
                    DescriptionRu = "Все медицинские документы надежно защищены в соответствии со стандартами GDPR.",
                    IconKey = "protected",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new AboutPillar
                {
                    TitleAz = "Əlavə ödənişsiz",
                    TitleEn = "Without added costs",
                    TitleRu = "Без скрытых доплат",
                    DescriptionAz = "Koordinator dəstəyi və təkliflərin hazırlanması pasientlər üçün tamamilə pulsuzdur.",
                    DescriptionEn = "Coordination services and specialist treatment offers carry zero platform commission for patients.",
                    DescriptionRu = "Услуги координатора и подготовка предложений абсолютно бесплатны для пациентов.",
                    IconKey = "no-extra-cost",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new AboutPillar
                {
                    TitleAz = "Əlçatan",
                    TitleEn = "Accessible",
                    TitleRu = "Доступность",
                    DescriptionAz = "Uzun gözləmə növbələri olmadan aparıcı mütəxəssislərlə sürətli qəbul və müalicə imkanı.",
                    DescriptionEn = "Fast access to board-certified specialists and modern diagnostics without long waiting lists.",
                    DescriptionRu = "Быстрый доступ к ведущим специалистам и современной диагностике без очередей.",
                    IconKey = "accessible",
                    DisplayOrder = 6,
                    IsActive = true
                }
            );
        }

        // G. Team Members
        if (!await context.TeamMembers.AnyAsync())
        {
            context.TeamMembers.AddRange(
                new TeamMember
                {
                    FullName = "Ivana Šajn",
                    RoleAz = "Baş Pasient Koordinatoru",
                    RoleEn = "Chief Patient Coordinator",
                    RoleRu = "Главный координатор пациентов",
                    BioAz = "Beynəlxalq pasientlərlə iş üzrə 10+ illik təcrübəyə malik baş koordinator. Müraciətlərin həkimlərə yönləndirilməsi və Zaqreb səyahətinin planlaşdırılmasına rəhbərlik edir.",
                    BioEn = "Lead patient coordinator with over 10 years of experience in international healthcare hospitality, guiding patients through diagnosis review, scheduling, and Zagreb stay.",
                    BioRu = "Ведущий координатор с опытом более 10 лет в международном медицинском туризме и сопровождении пациентов.",
                    QuoteAz = "Sizin sağlamlığınız və rahatlığınız bizim əsas prioritetimizdir.",
                    QuoteEn = "Your health, comfort, and peace of mind are our highest priority from day one.",
                    QuoteRu = "Ваше здоровье и комфорт — наш главный приоритет с первого дня.",
                    Email = "hospitality@altensorglobalhealthcare.com",
                    Phone = "+385993875702",
                    WhatsAppUrl = "https://wa.me/385993875702",
                    IsPrimaryCoordinator = true,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new TeamMember
                {
                    FullName = "Luka Kovačević",
                    RoleAz = "Tibbi Səyahət və Logistika Mütəxəssisi",
                    RoleEn = "Medical Travel & Logistics Specialist",
                    RoleRu = "Специалист по медицинской логистике",
                    BioAz = "Uçuş biletləri, otel yerləşdirməsi və hava limanı transferlərinin koordinasiyasını həyata keçirir.",
                    BioEn = "Coordinates flight bookings, clinic-nearby hotel stays, and private airport transfers for incoming patients.",
                    BioRu = "Отвечает за организацию авиабилетов, проживания и индивидуальных трансферов.",
                    Email = "travel@altensorglobalhealthcare.com",
                    Phone = "+38514898555",
                    IsPrimaryCoordinator = false,
                    DisplayOrder = 2,
                    IsActive = true
                }
            );
        }

        // H. 6 How It Works Steps
        if (!await context.HowItWorksSteps.AnyAsync())
        {
            context.HowItWorksSteps.AddRange(
                new HowItWorksStep
                {
                    StepNumber = 1,
                    TitleAz = "1. Müraciət göndərin",
                    TitleEn = "1. Send inquiry",
                    TitleRu = "1. Отправьте заявку",
                    ContentAz = "<p>Onlayn müraciət formasını doldurun, şikayətlərinizi qeyd edin və mövcud tibbi analiz/MRT nəticələrinizi əlavə edin.</p>",
                    ContentEn = "<p>Complete the online inquiry form, describe your symptoms or desired procedure, and attach any existing medical files.</p>",
                    ContentRu = "<p>Заполните онлайн-форму заявки, опишите симптомы и прикрепите имеющиеся медицинские документы.</p>",
                    CtaLabelAz = "Müraciət göndər",
                    CtaLabelEn = "Send inquiry",
                    CtaLabelRu = "Отправить заявку",
                    CtaUrl = "/en/my-profile/inquiries",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new HowItWorksStep
                {
                    StepNumber = 2,
                    TitleAz = "2. Koordinator və həkim dəyərləndirməsi",
                    TitleEn = "2. Specialist medical review",
                    TitleRu = "2. Оценка координатора и врача",
                    ContentAz = "<p>Baş pasient koordinatorunuz müraciətinizi qəbul edir və sənədlərinizi ixtisaslı həkimlərə təqdim edir.</p>",
                    ContentEn = "<p>Your dedicated coordinator reviews your case and forwards your medical documentation to the most suitable Zagreb specialists.</p>",
                    ContentRu = "<p>Ваш координатор рассматривает заявку и передает документы профильным специалистам в Загребе.</p>",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new HowItWorksStep
                {
                    StepNumber = 3,
                    TitleAz = "3. Fərdi müalicə təklifi alın",
                    TitleEn = "3. Receive your tailored offer",
                    TitleRu = "3. Получите персональное предложение",
                    ContentAz = "<p>Klinika tərəfindən hazırlanmış şəffaf qiymət cədvəlini (müalicə, hotel, transfer və uçuş seçimləri) şəxsi kabinetinizdə görün.</p>",
                    ContentEn = "<p>Receive a transparent all-in-one treatment and travel proposal detailing medical costs, clinic options, hotel, and transfers.</p>",
                    ContentRu = "<p>Получите прозрачное предложение с точной стоимостью лечения, проживания и трансферов в личном кабинете.</p>",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new HowItWorksStep
                {
                    StepNumber = 4,
                    TitleAz = "4. Təklifi təsdiqləyin və səfəri planlaşdırın",
                    TitleEn = "4. Confirm offer & plan your trip",
                    TitleRu = "4. Подтвердите предложение и поездку",
                    ContentAz = "<p>Sizə uyğun tarixləri seçin. Tərəfdaşlarımız Croatia Airlines və Obzor Holidays vasitəsilə bütün səyahət detalları təsdiqlənir.</p>",
                    ContentEn = "<p>Approve your preferred offer and dates. Our team coordinates discounted flights, hotel booking, and private airport pick-up.</p>",
                    ContentRu = "<p>Подтвердите удобные даты. Наша команда организует авиабилеты со скидкой, бронь отеля и встречу в аэропорту.</p>",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new HowItWorksStep
                {
                    StepNumber = 5,
                    TitleAz = "5. Zaqrebdə müalicə və fərdi müşayiət",
                    TitleEn = "5. Treatment & support in Zagreb",
                    TitleRu = "5. Лечение и сопровождение в Загребе",
                    ContentAz = "<p>Zaqrebə gəldiyiniz andan müayinə, əməliyyat və reabilitasiya müddətində koordinatorunuz bütün görüşləri müşayiət edir.</p>",
                    ContentEn = "<p>Enjoy seamless hospitality in Zagreb with on-ground coordinator support for clinic appointments, translation, and recovery care.</p>",
                    ContentRu = "<p>С момента прилета в Загреб координатор сопровождает вас на всех визитах, процедурах и во время реабилитации.</p>",
                    DisplayOrder = 5,
                    IsActive = true
                },
                new HowItWorksStep
                {
                    StepNumber = 6,
                    TitleAz = "6. Rəyiniz bizim üçün önəmlidir",
                    TitleEn = "6. Your opinion matters to us",
                    TitleRu = "6. Ваше мнение важно для нас",
                    ContentAz = "<p>Müalicəniz tamamlandıqdan sonra nəzarət konsultasiyaları davam edir və xidmət keyfiyyəti barədə rəyiniz platformamızda paylaşılır.</p>",
                    ContentEn = "<p>After returning home, we stay connected for follow-up care and invite you to share your verified experience to help future patients.</p>",
                    ContentRu = "<p>После возвращения домой мы остаемся на связи для контроля восстановления и будем рады вашему отзыву о лечении.</p>",
                    CtaLabelAz = "Pasient rəylərinə bax",
                    CtaLabelEn = "Read patient reviews",
                    CtaLabelRu = "Читать отзывы пациентов",
                    CtaUrl = "/en/patient-reviews",
                    DisplayOrder = 6,
                    IsActive = true
                }
            );
        }

        await context.SaveChangesAsync();
    }

    private static async Task UpsertSeedPageAsync(ApplicationDbContext context, CmsPage seed)
    {
        var existing = await context.CmsPages.FirstOrDefaultAsync(p => p.Slug == seed.Slug);
        if (existing == null)
        {
            await context.CmsPages.AddAsync(seed);
            return;
        }

        existing.ParentSlug ??= seed.ParentSlug;
        if (string.IsNullOrWhiteSpace(existing.PageType) || existing.PageType == "Standard")
        {
            existing.PageType = seed.PageType;
        }
        existing.TitleEn ??= seed.TitleEn;
        existing.TitleRu ??= seed.TitleRu;
        existing.HeadingEn ??= seed.HeadingEn;
        existing.HeadingRu ??= seed.HeadingRu;
        existing.SubtitleAz ??= seed.SubtitleAz;
        existing.SubtitleEn ??= seed.SubtitleEn;
        existing.SubtitleRu ??= seed.SubtitleRu;
        existing.ContentEn ??= seed.ContentEn;
        existing.ContentRu ??= seed.ContentRu;
        existing.MissionTitleAz ??= seed.MissionTitleAz;
        existing.MissionTitleEn ??= seed.MissionTitleEn;
        existing.MissionTitleRu ??= seed.MissionTitleRu;
        existing.MissionTextAz ??= seed.MissionTextAz;
        existing.MissionTextEn ??= seed.MissionTextEn;
        existing.MissionTextRu ??= seed.MissionTextRu;
        if (existing.DisplayOrder == 0 && seed.DisplayOrder > 0)
        {
            existing.DisplayOrder = seed.DisplayOrder;
        }
    }
}

