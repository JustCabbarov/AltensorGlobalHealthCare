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

    public PublicPortalService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var hierarchy = await _unitOfWork.ServiceCategories.GetHierarchyAsync(cancellationToken);
        return hierarchy.Select(c => MapCategoryToDto(c, lang)).ToList();
    }

    public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.ServiceCategories.GetHierarchyAsync(cancellationToken);
        var target = FindCategoryRecursive(categories, slug);
        return target != null ? MapCategoryToDto(target, lang) : null;
    }

    public async Task<MedicalServiceDetailDto?> GetServiceBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var service = await _unitOfWork.MedicalServices.GetBySlugWithClinicsAsync(slug, cancellationToken);
        if (service == null) return null;

        var startingPrice = service.ClinicServices.Where(cs => cs.IsActive).OrderBy(cs => cs.PriceFrom).FirstOrDefault();

        return new MedicalServiceDetailDto
        {
            Id = service.Id,
            Slug = service.Slug,
            Title = LocalizedText.Resolve(service.TitleAz, service.TitleEn, service.TitleRu, lang),
            CategoryId = service.CategoryId,
            CategoryName = LocalizedText.Resolve(service.Category?.TitleAz, service.Category?.TitleEn, service.Category?.TitleRu, lang),
            About = LocalizedText.Resolve(service.AboutAz, service.AboutEn, service.AboutRu, lang),
            Preparation = LocalizedText.Resolve(service.PreparationAz, service.PreparationEn, service.PreparationRu, lang),
            Steps = LocalizedText.Resolve(service.StepsAz, service.StepsEn, service.StepsRu, lang),
            Precautions = LocalizedText.Resolve(service.PrecautionsAz, service.PrecautionsEn, service.PrecautionsRu, lang),
            Result = LocalizedText.Resolve(service.ResultAz, service.ResultEn, service.ResultRu, lang),
            DurationText = service.DurationText,
            StartingPrice = startingPrice?.PriceFrom,
            Currency = startingPrice?.Currency ?? "EUR",
            AvailableClinics = service.ClinicServices.Where(cs => cs.IsActive && cs.Clinic.IsActive).Select(cs => new ClinicPriceDto
            {
                ClinicId = cs.ClinicId,
                ClinicName = cs.Clinic.Name,
                ClinicSlug = cs.Clinic.Slug,
                ClinicLogoUrl = cs.Clinic.LogoUrl,
                City = cs.Clinic.City,
                PriceFrom = cs.PriceFrom,
                Currency = cs.Currency
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<MedicalServiceSummaryDto>> GetMostSearchedServicesAsync(string? lang, CancellationToken cancellationToken = default)
    {
        var services = await _unitOfWork.MedicalServices.GetListAsync(s => s.IsActive && s.IsMostSearched, cancellationToken);
        return services.OrderBy(s => s.DisplayOrder).Select(s => new MedicalServiceSummaryDto
        {
            Id = s.Id,
            Slug = s.Slug,
            Title = LocalizedText.Resolve(s.TitleAz, s.TitleEn, s.TitleRu, lang),
            DurationText = s.DurationText,
            IsMostSearched = s.IsMostSearched
        }).ToList();
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
        var clinics = await _unitOfWork.Clinics.GetListAsync(c => c.IsActive, cancellationToken);
        return clinics.OrderBy(c => c.DisplayOrder).Select(c => new ClinicSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
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
        var doctors = await _unitOfWork.Doctors.GetListAsync(d => d.IsActive && d.Clinic.IsActive, cancellationToken);
        return doctors.OrderBy(d => d.DisplayOrder).Select(d => new DoctorDto
        {
            Id = d.Id,
            ClinicId = d.ClinicId,
            ClinicName = d.Clinic.Name,
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
        var doctor = await _unitOfWork.Doctors.FindAsync(d => d.Id == id && d.IsActive, cancellationToken);
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
            cancellationToken);

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
        return services.Select(s => new MedicalServiceSummaryDto
        {
            Id = s.Id,
            Slug = s.Slug,
            Title = LocalizedText.Resolve(s.TitleAz, s.TitleEn, s.TitleRu, lang),
            DurationText = s.DurationText,
            IsMostSearched = s.IsMostSearched
        }).ToList();
    }

    public async Task<CmsPageDto?> GetCmsPageAsync(string slug, string? lang, CancellationToken cancellationToken = default)
    {
        var page = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == slug && p.IsPublished, cancellationToken);
        if (page == null) return null;

        return new CmsPageDto
        {
            Slug = page.Slug,
            Title = LocalizedText.Resolve(page.TitleAz, page.TitleEn, page.TitleRu, lang),
            Heading = LocalizedText.Resolve(page.HeadingAz, page.HeadingEn, page.HeadingRu, lang),
            Content = LocalizedText.Resolve(page.ContentAz, page.ContentEn, page.ContentRu, lang)
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
        var faqs = await _unitOfWork.FaqItems.GetListAsync(f => f.IsActive, cancellationToken);
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

    private static CategoryDto MapCategoryToDto(ServiceCategory cat, string? lang)
    {
        return new CategoryDto
        {
            Id = cat.Id,
            Slug = cat.Slug,
            Title = LocalizedText.Resolve(cat.TitleAz, cat.TitleEn, cat.TitleRu, lang),
            Description = LocalizedText.Resolve(cat.DescriptionAz, cat.DescriptionEn, cat.DescriptionRu, lang),
            IconKey = cat.IconKey,
            DisplayOrder = cat.DisplayOrder,
            SubCategories = cat.SubCategories.Where(sub => sub.IsActive).OrderBy(sub => sub.DisplayOrder).Select(sub => MapCategoryToDto(sub, lang)).ToList(),
            Services = cat.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).Select(s => new MedicalServiceSummaryDto
            {
                Id = s.Id,
                Slug = s.Slug,
                Title = LocalizedText.Resolve(s.TitleAz, s.TitleEn, s.TitleRu, lang),
                DurationText = s.DurationText,
                IsMostSearched = s.IsMostSearched
            }).ToList()
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

