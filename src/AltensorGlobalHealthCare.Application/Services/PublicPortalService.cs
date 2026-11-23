using Microsoft.Extensions.Logging;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.Common.Localization;
using AltensorGlobalHealthCare.Application.DTOs.Catalog;
using AltensorGlobalHealthCare.Application.DTOs.Content;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Services;

public class PublicPortalService : IPublicPortalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notifications;
    private readonly ILogger<PublicPortalService> _logger;

    public PublicPortalService(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notifications,
        ILogger<PublicPortalService> logger)
    {
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(
        string? lang,
        bool headerOnly = false,
        bool homeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var hierarchy = await _unitOfWork.ServiceCategories.GetHierarchyAsync(cancellationToken);

        var filtered = hierarchy.Where(c =>
            (!headerOnly || c.ShowInHeaderNav) &&
            (!homeOnly || c.ShowOnHomePage));

        return filtered.Select(c => MapCategoryToDto(c, lang, headerOnly)).ToList();
    }

    public async Task<CategoryDetailDto?> GetCategoryBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var target = await _unitOfWork.ServiceCategories.GetBySlugWithDetailsAsync(normalizedSlug, null, cancellationToken);
        if (target == null)
        {
            // Fallback: check sub-category by slug if root not found
            var hierarchy = await _unitOfWork.ServiceCategories.GetHierarchyAsync(cancellationToken);
            var subMatch = FindCategoryRecursive(hierarchy, normalizedSlug);
            if (subMatch == null) return null;

            target = await _unitOfWork.ServiceCategories.GetBySlugWithDetailsAsync(
                subMatch.Slug,
                subMatch.Parent?.Slug,
                cancellationToken);
        }

        return target != null ? MapCategoryDetailToDto(target, lang) : null;
    }

    public async Task<CategoryDetailDto?> GetSubCategoryBySlugAsync(
        string categorySlug,
        string subSlug,
        string? lang,
        CancellationToken cancellationToken = default)
    {
        var parentSlug = categorySlug.Trim().ToLowerInvariant();
        var childSlug = subSlug.Trim().ToLowerInvariant();

        var target = await _unitOfWork.ServiceCategories.GetBySlugWithDetailsAsync(childSlug, parentSlug, cancellationToken);
        return target != null ? MapCategoryDetailToDto(target, lang) : null;
    }

    public async Task<MedicalServiceDetailDto?> GetServiceBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var service = await _unitOfWork.MedicalServices.GetBySlugWithClinicsAsync(normalizedSlug, cancellationToken);
        if (service == null) return null;

        var activePrices = service.ClinicServices.Where(cs => cs.IsActive && cs.Clinic.IsActive).ToList();
        var startingPrice = activePrices.OrderBy(cs => cs.PriceFrom).FirstOrDefault();

        return new MedicalServiceDetailDto
        {
            Id = service.Id,
            Slug = service.Slug,
            Title = LocalizedText.Resolve(service.TitleAz, service.TitleEn, service.TitleRu, lang),
            CategoryId = service.CategoryId,
            CategorySlug = service.Category?.Slug ?? string.Empty,
            CategoryName = LocalizedText.Resolve(service.Category?.TitleAz, service.Category?.TitleEn, service.Category?.TitleRu, lang),
            ParentCategoryId = service.Category?.ParentId,
            ParentCategorySlug = service.Category?.Parent?.Slug,
            ParentCategoryName = service.Category?.Parent != null
                ? LocalizedText.Resolve(service.Category.Parent.TitleAz, service.Category.Parent.TitleEn, service.Category.Parent.TitleRu, lang)
                : null,
            About = LocalizedText.Resolve(service.AboutAz, service.AboutEn, service.AboutRu, lang),
            Candidate = LocalizedText.Resolve(service.CandidateAz, service.CandidateEn, service.CandidateRu, lang),
            Preparation = LocalizedText.Resolve(service.PreparationAz, service.PreparationEn, service.PreparationRu, lang),
            Steps = LocalizedText.Resolve(service.StepsAz, service.StepsEn, service.StepsRu, lang),
            Precautions = LocalizedText.Resolve(service.PrecautionsAz, service.PrecautionsEn, service.PrecautionsRu, lang),
            Result = LocalizedText.Resolve(service.ResultAz, service.ResultEn, service.ResultRu, lang),
            PackageInfo = LocalizedText.Resolve(service.PackageInfoAz, service.PackageInfoEn, service.PackageInfoRu, lang),
            HeroImageUrl = service.HeroImageUrl,
            PromoBannerUrl = service.PromoBannerUrl,
            RelatedPackage = service.RelatedPackage != null && service.RelatedPackage.IsActive
                ? new TreatmentPackageDto
                {
                    Id = service.RelatedPackage.Id,
                    Slug = service.RelatedPackage.Slug,
                    Title = LocalizedText.Resolve(service.RelatedPackage.TitleAz, service.RelatedPackage.TitleEn, service.RelatedPackage.TitleRu, lang),
                    Description = LocalizedText.Resolve(service.RelatedPackage.DescriptionAz, service.RelatedPackage.DescriptionEn, service.RelatedPackage.DescriptionRu, lang),
                    Includes = LocalizedText.Resolve(service.RelatedPackage.IncludesAz, service.RelatedPackage.IncludesEn, service.RelatedPackage.IncludesRu, lang),
                    PriceFrom = service.RelatedPackage.PriceFrom,
                    Currency = service.RelatedPackage.Currency,
                    CoverUrl = service.RelatedPackage.CoverUrl,
                    MedicalServiceId = service.RelatedPackage.MedicalServiceId
                }
                : null,
            DurationText = service.DurationText,
            StartingPrice = startingPrice?.PriceFrom,
            Currency = startingPrice?.Currency ?? "EUR",
            AvailableClinics = activePrices
                .OrderBy(cs => cs.PriceFrom)
                .Select(cs => new ClinicPriceDto
                {
                    ClinicId = cs.ClinicId,
                    ClinicName = cs.Clinic.Name,
                    ClinicSlug = cs.Clinic.Slug,
                    ClinicLogoUrl = cs.Clinic.LogoUrl,
                    ClinicCoverUrl = cs.Clinic.CoverUrl,
                    ShortDescription = LocalizedText.Resolve(cs.Clinic.DescriptionAz, cs.Clinic.DescriptionEn, cs.Clinic.DescriptionRu, lang),
                    City = cs.Clinic.City,
                    PriceFrom = cs.PriceFrom,
                    Currency = cs.Currency
                }).ToList(),
            AvailableDoctors = service.DoctorServices
                .Where(dm => dm.Doctor != null && dm.Doctor.IsActive && dm.Doctor.Clinic != null && dm.Doctor.Clinic.IsActive)
                .OrderBy(dm => dm.DisplayOrder)
                .ThenBy(dm => dm.Doctor.DisplayOrder)
                .Select(dm => MapDoctorToDto(dm.Doctor, lang))
                .ToList(),
            Reviews = service.Reviews
                .Where(r => r.IsPublished)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Select(MapReviewToDto)
                .ToList(),
            Faqs = service.Faqs
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new FaqItemDto
                {
                    Id = f.Id,
                    Question = LocalizedText.Resolve(f.QuestionAz, f.QuestionEn, f.QuestionRu, lang),
                    Answer = LocalizedText.Resolve(f.AnswerAz, f.AnswerEn, f.AnswerRu, lang),
                    Category = f.Category
                }).ToList()
        };
    }

    public async Task<IReadOnlyList<MedicalServiceSummaryDto>> GetMostSearchedServicesAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var services = await _unitOfWork.MedicalServices.GetListAsync(
            s => s.IsActive && s.IsMostSearched,
            cancellationToken,
            s => s.ClinicServices);
        return services.OrderBy(s => s.DisplayOrder).Select(s => MapServiceSummaryToDto(s, lang)).ToList();
    }

    public async Task<IReadOnlyList<TreatmentPackageDto>> GetPackagesAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var packages = await _unitOfWork.TreatmentPackages.GetListAsync(p => p.IsActive, cancellationToken);
        return packages.OrderBy(p => p.DisplayOrder).Select(p => new TreatmentPackageDto
        {
            Id = p.Id,
            Slug = p.Slug,
            Title = LocalizedText.Resolve(p.TitleAz, p.TitleEn, p.TitleRu, lang),
            Description = LocalizedText.Resolve(p.DescriptionAz, p.DescriptionEn, p.DescriptionRu, lang),
            Includes = LocalizedText.Resolve(p.IncludesAz, p.IncludesEn, p.IncludesRu, lang),
            PriceFrom = p.PriceFrom,
            Currency = p.Currency,
            CoverUrl = p.CoverUrl,
            MedicalServiceId = p.MedicalServiceId
        }).ToList();
    }

    public async Task<TreatmentPackageDto?> GetPackageBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var package = await _unitOfWork.TreatmentPackages.FindAsync(p => p.Slug == slug && p.IsActive, cancellationToken);
        if (package == null) return null;

        return new TreatmentPackageDto
        {
            Id = package.Id,
            Slug = package.Slug,
            Title = LocalizedText.Resolve(package.TitleAz, package.TitleEn, package.TitleRu, lang),
            Description = LocalizedText.Resolve(package.DescriptionAz, package.DescriptionEn, package.DescriptionRu, lang),
            Includes = LocalizedText.Resolve(package.IncludesAz, package.IncludesEn, package.IncludesRu, lang),
            PriceFrom = package.PriceFrom,
            Currency = package.Currency,
            CoverUrl = package.CoverUrl,
            MedicalServiceId = package.MedicalServiceId
        };
    }

    public async Task<IReadOnlyList<ClinicSummaryDto>> GetClinicsAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var clinics = await _unitOfWork.Clinics.GetListAsync(
            c => c.IsActive,
            cancellationToken,
            c => c.Doctors,
            c => c.ClinicServices);
        return clinics.OrderBy(c => c.DisplayOrder).Select(c => new ClinicSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = LocalizedText.Resolve(c.DescriptionAz, c.DescriptionEn, c.DescriptionRu, lang),
            LogoUrl = c.LogoUrl,
            CoverUrl = c.CoverUrl,
            City = c.City,
            Phone = c.Phone,
            Email = c.Email,
            DoctorsCount = c.Doctors.Count(d => d.IsActive),
            ServicesCount = c.ClinicServices.Count(cs => cs.IsActive)
        }).ToList();
    }

    public async Task<ClinicDetailDto?> GetClinicBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var clinic = await _unitOfWork.Clinics.GetBySlugWithDetailsAsync(slug, cancellationToken);
        if (clinic == null) return null;

        return new ClinicDetailDto
        {
            Id = clinic.Id,
            Name = clinic.Name,
            Slug = clinic.Slug,
            Description = LocalizedText.Resolve(clinic.DescriptionAz, clinic.DescriptionEn, clinic.DescriptionRu, lang),
            Address = clinic.Address,
            City = clinic.City,
            Phone = clinic.Phone,
            Email = clinic.Email,
            WebsiteUrl = clinic.WebsiteUrl,
            LogoUrl = clinic.LogoUrl,
            CoverUrl = clinic.CoverUrl,
            Certificates = clinic.Certificates,
            Doctors = clinic.Doctors.Where(d => d.IsActive).OrderBy(d => d.DisplayOrder).Select(d => new DoctorDto
            {
                Id = d.Id,
                ClinicId = clinic.Id,
                ClinicName = clinic.Name,
                FullName = d.FullName,
                Title = LocalizedText.Resolve(d.TitleAz, d.TitleEn, d.TitleRu, lang),
                Specialization = LocalizedText.Resolve(d.SpecializationAz, d.SpecializationEn, d.SpecializationRu, lang),
                Bio = LocalizedText.Resolve(d.BioAz, d.BioEn, d.BioRu, lang),
                ExperienceYears = d.ExperienceYears,
                PhotoUrl = d.PhotoUrl
            }).ToList(),
            OfferedServices = clinic.ClinicServices.Where(cs => cs.IsActive).Select(cs => new MedicalServiceSummaryDto
            {
                Id = cs.MedicalServiceId,
                Slug = cs.MedicalService.Slug,
                Title = LocalizedText.Resolve(cs.MedicalService.TitleAz, cs.MedicalService.TitleEn, cs.MedicalService.TitleRu, lang),
                DurationText = cs.MedicalService.DurationText,
                StartingPrice = cs.PriceFrom,
                Currency = cs.Currency
            }).ToList(),
            Reviews = clinic.Reviews.Where(r => r.IsPublished).OrderByDescending(r => r.CreatedAtUtc).Select(r => new ClinicReviewDto
            {
                Id = r.Id,
                AuthorName = r.AuthorName,
                AuthorCountry = r.AuthorCountry,
                Rating = r.Rating,
                Comment = r.Comment,
                IsVerifiedPatient = r.IsVerifiedPatient,
                CreatedAtUtc = r.CreatedAtUtc
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<DoctorDto>> GetDoctorsAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var doctors = await _unitOfWork.Doctors.GetListAsync(
            d => d.IsActive && d.Clinic != null && d.Clinic.IsActive,
            cancellationToken,
            d => d.Clinic);
        return doctors.OrderBy(d => d.DisplayOrder).Select(d => new DoctorDto
        {
            Id = d.Id,
            ClinicId = d.ClinicId,
            ClinicName = d.Clinic?.Name ?? string.Empty,
            FullName = d.FullName,
            Title = LocalizedText.Resolve(d.TitleAz, d.TitleEn, d.TitleRu, lang),
            Specialization = LocalizedText.Resolve(d.SpecializationAz, d.SpecializationEn, d.SpecializationRu, lang),
            Bio = LocalizedText.Resolve(d.BioAz, d.BioEn, d.BioRu, lang),
            ExperienceYears = d.ExperienceYears,
            PhotoUrl = d.PhotoUrl
        }).ToList();
    }

    public async Task<DoctorDto?> GetDoctorByIdAsync(Guid id, string? lang, CancellationToken cancellationToken = default)
    {
        var doctor = await _unitOfWork.Doctors.FindAsync(
            d => d.Id == id && d.IsActive,
            cancellationToken,
            d => d.Clinic);
        if (doctor == null) return null;

        return new DoctorDto
        {
            Id = doctor.Id,
            ClinicId = doctor.ClinicId,
            ClinicName = doctor.Clinic?.Name ?? string.Empty,
            FullName = doctor.FullName,
            Title = LocalizedText.Resolve(doctor.TitleAz, doctor.TitleEn, doctor.TitleRu, lang),
            Specialization = LocalizedText.Resolve(doctor.SpecializationAz, doctor.SpecializationEn, doctor.SpecializationRu, lang),
            Bio = LocalizedText.Resolve(doctor.BioAz, doctor.BioEn, doctor.BioRu, lang),
            ExperienceYears = doctor.ExperienceYears,
            PhotoUrl = doctor.PhotoUrl
        };
    }

    public async Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(Guid? clinicId, Guid? doctorId, CancellationToken cancellationToken = default)
    {
        var reviews = await _unitOfWork.Reviews.GetListAsync(
            r => r.IsPublished &&
                 (!clinicId.HasValue || r.ClinicId == clinicId) &&
                 (!doctorId.HasValue || r.DoctorId == doctorId),
            cancellationToken,
            r => r.Clinic!,
            r => r.Doctor!);

        return reviews.OrderByDescending(r => r.CreatedAtUtc).Select(r => new ReviewDto
        {
            Id = r.Id,
            ClinicId = r.ClinicId,
            ClinicName = r.Clinic?.Name,
            DoctorId = r.DoctorId,
            DoctorName = r.Doctor?.FullName,
            AuthorName = r.AuthorName,
            AuthorCountry = r.AuthorCountry,
            Rating = r.Rating,
            Comment = r.Comment,
            Source = r.Source,
            IsVerifiedPatient = r.IsVerifiedPatient,
            CreatedAtUtc = r.CreatedAtUtc
        }).ToList();
    }

    public async Task<IReadOnlyList<MedicalServiceSummaryDto>> SearchAsync(string query, string? lang, CancellationToken cancellationToken = default)
    {
        var services = await _unitOfWork.MedicalServices.SearchAsync(query, cancellationToken);
        return services.Select(s => MapServiceSummaryToDto(s, lang)).ToList();
    }

    public async Task<CmsPageDto?> GetCmsPageAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var page = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == normalizedSlug && p.IsPublished, cancellationToken);
        return page != null ? MapCmsPageToDto(page, lang) : null;
    }

    public async Task<IReadOnlyList<CmsPageMenuItemDto>> GetCmsPagesMenuAsync(string? parentSlug, string? lang, CancellationToken cancellationToken = default)
    {
        var normalizedParent = parentSlug?.Trim().ToLowerInvariant();
        var pages = await _unitOfWork.CmsPages.GetListAsync(
            p => p.IsPublished && p.ShowInMenu &&
                 (string.IsNullOrEmpty(normalizedParent) || p.ParentSlug == normalizedParent),
            cancellationToken);

        return pages
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new CmsPageMenuItemDto
            {
                Slug = p.Slug,
                Title = LocalizedText.Resolve(p.TitleAz, p.TitleEn, p.TitleRu, lang),
                PageType = p.PageType,
                DisplayOrder = p.DisplayOrder
            })
            .ToList();
    }

    public async Task<AboutUsDto> GetAboutUsAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var aboutPage = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == "about-us" && p.IsPublished, cancellationToken);
        var subPages = await _unitOfWork.CmsPages.GetListAsync(
            p => p.IsPublished && p.ShowInMenu && p.ParentSlug == "about-us",
            cancellationToken);
        var pillars = await _unitOfWork.AboutPillars.GetListAsync(p => p.IsActive, cancellationToken);
        var teamMembers = await _unitOfWork.TeamMembers.GetListAsync(t => t.IsActive, cancellationToken);
        var steps = await _unitOfWork.HowItWorksSteps.GetListAsync(s => s.IsActive, cancellationToken);

        var mappedTeam = teamMembers
            .OrderBy(t => t.DisplayOrder)
            .Select(t => MapTeamMemberToDto(t, lang))
            .ToList();

        return new AboutUsDto
        {
            Page = aboutPage != null ? MapCmsPageToDto(aboutPage, lang) : null,
            SubPages = subPages
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new CmsPageMenuItemDto
                {
                    Slug = p.Slug,
                    Title = LocalizedText.Resolve(p.TitleAz, p.TitleEn, p.TitleRu, lang),
                    PageType = p.PageType,
                    DisplayOrder = p.DisplayOrder
                }).ToList(),
            Pillars = pillars
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new AboutPillarDto
                {
                    Id = p.Id,
                    Title = LocalizedText.Resolve(p.TitleAz, p.TitleEn, p.TitleRu, lang),
                    Description = LocalizedText.Resolve(p.DescriptionAz, p.DescriptionEn, p.DescriptionRu, lang),
                    IconKey = p.IconKey,
                    DisplayOrder = p.DisplayOrder
                }).ToList(),
            TeamMembers = mappedTeam,
            HowItWorksSteps = steps
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.StepNumber)
                .Select(s => MapHowItWorksStepToDto(s, lang))
                .ToList(),
            PrimaryCoordinator = mappedTeam.FirstOrDefault(t => t.IsPrimaryCoordinator)
        };
    }

    public async Task<HowItWorksDto> GetHowItWorksAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var page = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == "how-it-works" && p.IsPublished, cancellationToken);
        var steps = await _unitOfWork.HowItWorksSteps.GetListAsync(s => s.IsActive, cancellationToken);

        return new HowItWorksDto
        {
            Page = page != null ? MapCmsPageToDto(page, lang) : null,
            Steps = steps
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.StepNumber)
                .Select(s => MapHowItWorksStepToDto(s, lang))
                .ToList()
        };
    }

    public async Task<IReadOnlyList<BlogPostSummaryDto>> GetBlogPostsAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var posts = await _unitOfWork.BlogPosts.GetListAsync(b => b.IsPublished, cancellationToken);
        return posts.OrderByDescending(b => b.PublishedAtUtc).Select(b => new BlogPostSummaryDto
        {
            Slug = b.Slug,
            Title = LocalizedText.Resolve(b.TitleAz, b.TitleEn, b.TitleRu, lang),
            Summary = LocalizedText.Resolve(b.SummaryAz, b.SummaryEn, b.SummaryRu, lang),
            CoverUrl = b.CoverUrl,
            PublishedAtUtc = b.PublishedAtUtc
        }).ToList();
    }

    public async Task<BlogPostDetailDto?> GetBlogPostBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var post = await _unitOfWork.BlogPosts.FindAsync(b => b.Slug == slug && b.IsPublished, cancellationToken);
        if (post == null) return null;

        return new BlogPostDetailDto
        {
            Slug = post.Slug,
            Title = LocalizedText.Resolve(post.TitleAz, post.TitleEn, post.TitleRu, lang),
            Summary = LocalizedText.Resolve(post.SummaryAz, post.SummaryEn, post.SummaryRu, lang),
            Content = LocalizedText.Resolve(post.ContentAz, post.ContentEn, post.ContentRu, lang),
            CoverUrl = post.CoverUrl,
            PublishedAtUtc = post.PublishedAtUtc
        };
    }

    public async Task<IReadOnlyList<FaqItemDto>> GetFaqItemsAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var faqs = await _unitOfWork.FaqItems.GetListAsync(
            f => f.IsActive && f.MedicalServiceId == null && f.ServiceCategoryId == null,
            cancellationToken);
        return faqs.OrderBy(f => f.DisplayOrder).Select(f => new FaqItemDto
        {
            Id = f.Id,
            Question = LocalizedText.Resolve(f.QuestionAz, f.QuestionEn, f.QuestionRu, lang),
            Answer = LocalizedText.Resolve(f.AnswerAz, f.AnswerEn, f.AnswerRu, lang),
            Category = f.Category
        }).ToList();
    }

    public async Task<IReadOnlyList<PartnerDto>> GetPartnersAsync(CancellationToken cancellationToken = default)
    {
        var partners = await _unitOfWork.Partners.GetListAsync(p => p.IsActive, cancellationToken);
        return partners.OrderBy(p => p.DisplayOrder).Select(p => new PartnerDto
        {
            Id = p.Id,
            Name = p.Name,
            LogoUrl = p.LogoUrl,
            WebsiteUrl = p.WebsiteUrl,
            PartnerType = p.PartnerType
        }).ToList();
    }

    public async Task<string> SubmitContactMessageAsync(ContactMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        var msg = new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone?.Trim(),
            Subject = request.Subject?.Trim(),
            Message = request.Message.Trim()
        };

        await _unitOfWork.ContactMessages.AddAsync(msg, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _notifications.SendContactMessageNotificationToCoordinatorAsync(msg, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Əlaqə mesajı bildirişi göndərilərkən xəta baş verdi: {MessageId}", msg.Id);
        }

        return "Mesajınız qəbul edildi. Tezliklə əlaqə saxlayacağıq.";
    }

    public async Task<string> SubscribeNewsletterAsync(NewsletterRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _unitOfWork.NewsletterSubscribers.FindAsync(s => s.Email == email, cancellationToken);
        if (existing != null)
        {
            existing.IsActive = true;
        }
        else
        {
            await _unitOfWork.NewsletterSubscribers.AddAsync(new NewsletterSubscriber { Email = email }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return "Xəbər bülleteninə uğurla abunə oldunuz.";
    }

    public async Task<string> SubmitReviewAsync(CreateReviewRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new BadRequestException("Reytinq 1 ilə 5 arasında olmalıdır.", "INVALID_RATING");
        }

        var review = new Review
        {
            ClinicId = request.ClinicId,
            DoctorId = request.DoctorId,
            AuthorName = request.AuthorName.Trim(),
            AuthorCountry = request.AuthorCountry?.Trim(),
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            Source = ReviewSource.Internal,
            IsVerifiedPatient = false,
            IsPublished = true // Can be moderated by admin
        };

        await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Rəyiniz uğurla əlavə edildi. Təşəkkür edirik!";
    }

    private static CategoryDto MapCategoryToDto(ServiceCategory cat, string? lang, bool headerOnly = false)
    {
        return new CategoryDto
        {
            Id = cat.Id,
            ParentId = cat.ParentId,
            Slug = cat.Slug,
            Title = LocalizedText.Resolve(cat.TitleAz, cat.TitleEn, cat.TitleRu, lang),
            Description = LocalizedText.Resolve(cat.DescriptionAz, cat.DescriptionEn, cat.DescriptionRu, lang),
            IconKey = cat.IconKey,
            DisplayOrder = cat.DisplayOrder,
            ShowInHeaderNav = cat.ShowInHeaderNav,
            ShowOnHomePage = cat.ShowOnHomePage,
            MetaTitle = LocalizedText.Resolve(cat.MetaTitleAz, cat.MetaTitleEn, cat.MetaTitleRu, lang),
            MetaDescription = LocalizedText.Resolve(cat.MetaDescriptionAz, cat.MetaDescriptionEn, cat.MetaDescriptionRu, lang),
            SubCategories = cat.SubCategories
                .Where(sub => sub.IsActive && (!headerOnly || sub.ShowInHeaderNav))
                .OrderBy(sub => sub.DisplayOrder)
                .Select(sub => MapCategoryToDto(sub, lang, headerOnly))
                .ToList(),
            Services = cat.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => MapServiceSummaryToDto(s, lang))
                .ToList()
        };
    }

    private static CategoryDetailDto MapCategoryDetailToDto(ServiceCategory cat, string? lang)
    {
        var allServices = cat.Services.Where(s => s.IsActive)
            .Concat(cat.SubCategories.Where(sub => sub.IsActive).SelectMany(sub => sub.Services.Where(s => s.IsActive)))
            .ToList();

        var uniqueClinics = allServices
            .SelectMany(s => s.ClinicServices.Where(cs => cs.IsActive && cs.Clinic != null && cs.Clinic.IsActive))
            .Select(cs => cs.Clinic)
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new ClinicSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = LocalizedText.Resolve(c.DescriptionAz, c.DescriptionEn, c.DescriptionRu, lang),
                LogoUrl = c.LogoUrl,
                CoverUrl = c.CoverUrl,
                City = c.City,
                Phone = c.Phone,
                Email = c.Email
            })
            .ToList();

        var uniqueDoctors = allServices
            .SelectMany(s => s.DoctorServices)
            .Where(dm => dm.Doctor != null && dm.Doctor.IsActive && dm.Doctor.Clinic != null && dm.Doctor.Clinic.IsActive)
            .Select(dm => dm.Doctor)
            .GroupBy(d => d.Id)
            .Select(g => g.First())
            .OrderBy(d => d.DisplayOrder)
            .Select(d => MapDoctorToDto(d, lang))
            .ToList();

        var relatedReviews = allServices
            .SelectMany(s => s.Reviews.Where(r => r.IsPublished))
            .GroupBy(r => r.Id)
            .Select(g => g.First())
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(MapReviewToDto)
            .ToList();

        return new CategoryDetailDto
        {
            Id = cat.Id,
            ParentId = cat.ParentId,
            ParentCategorySlug = cat.Parent?.Slug,
            ParentCategoryTitle = cat.Parent != null
                ? LocalizedText.Resolve(cat.Parent.TitleAz, cat.Parent.TitleEn, cat.Parent.TitleRu, lang)
                : null,
            Slug = cat.Slug,
            Title = LocalizedText.Resolve(cat.TitleAz, cat.TitleEn, cat.TitleRu, lang),
            Description = LocalizedText.Resolve(cat.DescriptionAz, cat.DescriptionEn, cat.DescriptionRu, lang),
            IconKey = cat.IconKey,
            DisplayOrder = cat.DisplayOrder,
            ShowInHeaderNav = cat.ShowInHeaderNav,
            ShowOnHomePage = cat.ShowOnHomePage,
            MetaTitle = LocalizedText.Resolve(cat.MetaTitleAz, cat.MetaTitleEn, cat.MetaTitleRu, lang),
            MetaDescription = LocalizedText.Resolve(cat.MetaDescriptionAz, cat.MetaDescriptionEn, cat.MetaDescriptionRu, lang),
            SubCategories = cat.SubCategories
                .Where(sub => sub.IsActive)
                .OrderBy(sub => sub.DisplayOrder)
                .Select(sub => MapCategoryToDto(sub, lang))
                .ToList(),
            Services = cat.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => MapServiceSummaryToDto(s, lang))
                .ToList(),
            Clinics = uniqueClinics,
            Doctors = uniqueDoctors,
            Reviews = relatedReviews
        };
    }

    private static MedicalServiceSummaryDto MapServiceSummaryToDto(MedicalService s, string? lang)
    {
        var minPrice = s.ClinicServices
            .Where(cs => cs.IsActive)
            .OrderBy(cs => cs.PriceFrom)
            .FirstOrDefault();

        return new MedicalServiceSummaryDto
        {
            Id = s.Id,
            Slug = s.Slug,
            Title = LocalizedText.Resolve(s.TitleAz, s.TitleEn, s.TitleRu, lang),
            DurationText = s.DurationText,
            IsMostSearched = s.IsMostSearched,
            StartingPrice = minPrice?.PriceFrom,
            Currency = minPrice?.Currency ?? "EUR"
        };
    }

    private static DoctorDto MapDoctorToDto(Doctor d, string? lang)
    {
        return new DoctorDto
        {
            Id = d.Id,
            ClinicId = d.ClinicId,
            ClinicName = d.Clinic?.Name ?? string.Empty,
            FullName = d.FullName,
            Title = LocalizedText.Resolve(d.TitleAz, d.TitleEn, d.TitleRu, lang),
            Specialization = LocalizedText.Resolve(d.SpecializationAz, d.SpecializationEn, d.SpecializationRu, lang),
            Bio = LocalizedText.Resolve(d.BioAz, d.BioEn, d.BioRu, lang),
            ExperienceYears = d.ExperienceYears,
            PhotoUrl = d.PhotoUrl
        };
    }

    private static ReviewDto MapReviewToDto(Review r)
    {
        return new ReviewDto
        {
            Id = r.Id,
            ClinicId = r.ClinicId,
            ClinicName = r.Clinic?.Name,
            DoctorId = r.DoctorId,
            DoctorName = r.Doctor?.FullName,
            AuthorName = r.AuthorName,
            AuthorCountry = r.AuthorCountry,
            Rating = r.Rating,
            Comment = r.Comment,
            Source = r.Source,
            IsVerifiedPatient = r.IsVerifiedPatient,
            CreatedAtUtc = r.CreatedAtUtc
        };
    }

    private static CmsPageDto MapCmsPageToDto(CmsPage page, string? lang)
    {
        return new CmsPageDto
        {
            Slug = page.Slug,
            ParentSlug = page.ParentSlug,
            PageType = page.PageType,
            Title = LocalizedText.Resolve(page.TitleAz, page.TitleEn, page.TitleRu, lang),
            Heading = LocalizedText.Resolve(page.HeadingAz, page.HeadingEn, page.HeadingRu, lang),
            Subtitle = LocalizedText.Resolve(page.SubtitleAz, page.SubtitleEn, page.SubtitleRu, lang),
            Content = LocalizedText.Resolve(page.ContentAz, page.ContentEn, page.ContentRu, lang),
            MissionTitle = LocalizedText.Resolve(page.MissionTitleAz, page.MissionTitleEn, page.MissionTitleRu, lang),
            MissionText = LocalizedText.Resolve(page.MissionTextAz, page.MissionTextEn, page.MissionTextRu, lang),
            HeroImageUrl = page.HeroImageUrl,
            ShowInMenu = page.ShowInMenu,
            DisplayOrder = page.DisplayOrder
        };
    }

    private static TeamMemberDto MapTeamMemberToDto(TeamMember t, string? lang)
    {
        return new TeamMemberDto
        {
            Id = t.Id,
            FullName = t.FullName,
            Role = LocalizedText.Resolve(t.RoleAz, t.RoleEn, t.RoleRu, lang),
            Bio = LocalizedText.Resolve(t.BioAz, t.BioEn, t.BioRu, lang),
            Quote = LocalizedText.Resolve(t.QuoteAz, t.QuoteEn, t.QuoteRu, lang),
            Email = t.Email,
            Phone = t.Phone,
            WhatsAppUrl = t.WhatsAppUrl,
            PhotoUrl = t.PhotoUrl,
            IsPrimaryCoordinator = t.IsPrimaryCoordinator,
            DisplayOrder = t.DisplayOrder
        };
    }

    private static HowItWorksStepDto MapHowItWorksStepToDto(HowItWorksStep s, string? lang)
    {
        return new HowItWorksStepDto
        {
            Id = s.Id,
            StepNumber = s.StepNumber,
            Title = LocalizedText.Resolve(s.TitleAz, s.TitleEn, s.TitleRu, lang),
            Content = LocalizedText.Resolve(s.ContentAz, s.ContentEn, s.ContentRu, lang),
            CtaLabel = LocalizedText.Resolve(s.CtaLabelAz, s.CtaLabelEn, s.CtaLabelRu, lang),
            CtaUrl = s.CtaUrl,
            DisplayOrder = s.DisplayOrder
        };
    }

    private static ServiceCategory? FindCategoryRecursive(IEnumerable<ServiceCategory> categories, string slug)
    {
        foreach (var c in categories)
        {
            if (c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)) return c;
            var inSub = FindCategoryRecursive(c.SubCategories, slug);
            if (inSub != null) return inSub;
        }
        return null;
    }
}

