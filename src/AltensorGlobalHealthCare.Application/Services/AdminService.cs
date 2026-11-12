using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Admin;
using AltensorGlobalHealthCare.Application.DTOs.Catalog;
using AltensorGlobalHealthCare.Application.DTOs.Content;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;

    public AdminService(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
    }

    public async Task<DashboardSummaryDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var inquiries = await _unitOfWork.Inquiries.GetAllAsync(cancellationToken);
        var activeInquiries = inquiries.Count(i => i.Status != InquiryStatus.Closed && i.Status != InquiryStatus.Cancelled);
        var patientsCount = await _unitOfWork.PatientProfiles.CountAsync(null, cancellationToken);
        var clinicsCount = await _unitOfWork.Clinics.CountAsync(null, cancellationToken);
        var doctorsCount = await _unitOfWork.Doctors.CountAsync(null, cancellationToken);
        var offers = await _unitOfWork.Offers.GetAllAsync(cancellationToken);

        var acceptedOffers = offers.Where(o => o.Status == OfferStatus.Accepted).ToList();
        var totalVolume = acceptedOffers.Sum(o => o.TotalPrice);

        return new DashboardSummaryDto
        {
            TotalInquiries = inquiries.Count,
            ActiveInquiries = activeInquiries,
            TotalPatients = patientsCount,
            TotalClinics = clinicsCount,
            TotalDoctors = doctorsCount,
            TotalOffers = offers.Count,
            AcceptedOffers = acceptedOffers.Count,
            TotalAcceptedVolume = totalVolume,
            RecentInquiries = inquiries.OrderByDescending(i => i.CreatedAtUtc).Take(10).Select(i => new InquirySummaryDto
            {
                Id = i.Id,
                InquiryNumber = i.InquiryNumber,
                Symptoms = i.Symptoms,
                ServiceTitle = i.MedicalService?.TitleAz,
                Status = i.Status,
                StatusText = i.Status.ToString(),
                CreatedAtUtc = i.CreatedAtUtc
            }).ToList()
        };
    }

    // --- Clinics ---
    public async Task<IReadOnlyList<ClinicSummaryDto>> GetAllClinicsAsync(CancellationToken cancellationToken = default)
    {
        var clinics = await _unitOfWork.Clinics.GetAllAsync(cancellationToken);
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
            DoctorsCount = c.Doctors.Count,
            ServicesCount = c.ClinicServices.Count
        }).ToList();
    }

    public async Task<ClinicDetailDto> CreateClinicAsync(CreateClinicRequestDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.Clinics.ExistsAsync(c => c.Slug == slug, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə klinika artıq mövcuddur.", "SLUG_ALREADY_EXISTS");
        }

        var clinic = new Clinic
        {
            Name = request.Name.Trim(),
            Slug = slug,
            DescriptionAz = request.DescriptionAz?.Trim(),
            DescriptionEn = request.DescriptionEn?.Trim(),
            DescriptionRu = request.DescriptionRu?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            WebsiteUrl = request.WebsiteUrl?.Trim(),
            Certificates = request.Certificates?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _unitOfWork.Clinics.AddAsync(clinic, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClinicDetailDto
        {
            Id = clinic.Id,
            Name = clinic.Name,
            Slug = clinic.Slug,
            Address = clinic.Address,
            City = clinic.City,
            Phone = clinic.Phone,
            Email = clinic.Email
        };
    }

    public async Task<ClinicDetailDto> UpdateClinicAsync(Guid id, UpdateClinicRequestDto request, CancellationToken cancellationToken = default)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(id, cancellationToken);
        if (clinic == null) throw NotFoundException.For<Clinic>(id);

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.Clinics.ExistsAsync(c => c.Slug == slug && c.Id != id, cancellationToken))
        {
            throw new ConflictException("Bu slug başqa bir klinika üçün istifadə olunur.", "SLUG_ALREADY_EXISTS");
        }

        clinic.Name = request.Name.Trim();
        clinic.Slug = slug;
        clinic.DescriptionAz = request.DescriptionAz?.Trim();
        clinic.DescriptionEn = request.DescriptionEn?.Trim();
        clinic.DescriptionRu = request.DescriptionRu?.Trim();
        clinic.Address = request.Address?.Trim();
        clinic.City = request.City?.Trim();
        clinic.Phone = request.Phone?.Trim();
        clinic.Email = request.Email?.Trim();
        clinic.WebsiteUrl = request.WebsiteUrl?.Trim();
        clinic.Certificates = request.Certificates?.Trim();
        clinic.DisplayOrder = request.DisplayOrder;
        clinic.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClinicDetailDto
        {
            Id = clinic.Id,
            Name = clinic.Name,
            Slug = clinic.Slug,
            Address = clinic.Address,
            City = clinic.City,
            Phone = clinic.Phone,
            Email = clinic.Email
        };
    }

    public async Task DeleteClinicAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(id, cancellationToken);
        if (clinic == null) throw NotFoundException.For<Clinic>(id);

        _unitOfWork.Clinics.Remove(clinic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> UpdateClinicLogoAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(id, cancellationToken);
        if (clinic == null) throw NotFoundException.For<Clinic>(id);

        if (!string.IsNullOrEmpty(clinic.LogoUrl)) _fileStorage.DeleteFile(clinic.LogoUrl);

        var logoUrl = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "clinics/logos", cancellationToken);
        clinic.LogoUrl = logoUrl;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return logoUrl;
    }

    public async Task<string> UpdateClinicCoverAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(id, cancellationToken);
        if (clinic == null) throw NotFoundException.For<Clinic>(id);

        if (!string.IsNullOrEmpty(clinic.CoverUrl)) _fileStorage.DeleteFile(clinic.CoverUrl);

        var coverUrl = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "clinics/covers", cancellationToken);
        clinic.CoverUrl = coverUrl;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return coverUrl;
    }

    // --- Doctors ---
    public async Task<DoctorDto> CreateDoctorAsync(CreateDoctorRequestDto request, CancellationToken cancellationToken = default)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(request.ClinicId, cancellationToken);
        if (clinic == null) throw NotFoundException.For<Clinic>(request.ClinicId);

        var doctor = new Doctor
        {
            ClinicId = request.ClinicId,
            FullName = request.FullName.Trim(),
            TitleAz = request.TitleAz?.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            SpecializationAz = request.SpecializationAz?.Trim(),
            SpecializationEn = request.SpecializationEn?.Trim(),
            SpecializationRu = request.SpecializationRu?.Trim(),
            BioAz = request.BioAz?.Trim(),
            BioEn = request.BioEn?.Trim(),
            BioRu = request.BioRu?.Trim(),
            ExperienceYears = request.ExperienceYears,
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _unitOfWork.Doctors.AddAsync(doctor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DoctorDto
        {
            Id = doctor.Id,
            ClinicId = clinic.Id,
            ClinicName = clinic.Name,
            FullName = doctor.FullName,
            Title = doctor.TitleAz,
            Specialization = doctor.SpecializationAz,
            ExperienceYears = doctor.ExperienceYears
        };
    }

    public async Task<DoctorDto> UpdateDoctorAsync(Guid id, UpdateDoctorRequestDto request, CancellationToken cancellationToken = default)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(id, cancellationToken);
        if (doctor == null) throw NotFoundException.For<Doctor>(id);

        doctor.FullName = request.FullName.Trim();
        doctor.TitleAz = request.TitleAz?.Trim();
        doctor.TitleEn = request.TitleEn?.Trim();
        doctor.TitleRu = request.TitleRu?.Trim();
        doctor.SpecializationAz = request.SpecializationAz?.Trim();
        doctor.SpecializationEn = request.SpecializationEn?.Trim();
        doctor.SpecializationRu = request.SpecializationRu?.Trim();
        doctor.BioAz = request.BioAz?.Trim();
        doctor.BioEn = request.BioEn?.Trim();
        doctor.BioRu = request.BioRu?.Trim();
        doctor.ExperienceYears = request.ExperienceYears;
        doctor.Phone = request.Phone?.Trim();
        doctor.Email = request.Email?.Trim();
        doctor.DisplayOrder = request.DisplayOrder;
        doctor.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DoctorDto
        {
            Id = doctor.Id,
            ClinicId = doctor.ClinicId,
            FullName = doctor.FullName,
            Title = doctor.TitleAz,
            Specialization = doctor.SpecializationAz,
            ExperienceYears = doctor.ExperienceYears,
            PhotoUrl = doctor.PhotoUrl
        };
    }

    public async Task DeleteDoctorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(id, cancellationToken);
        if (doctor == null) throw NotFoundException.For<Doctor>(id);

        _unitOfWork.Doctors.Remove(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> UpdateDoctorPhotoAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var doctor = await _unitOfWork.Doctors.GetByIdAsync(id, cancellationToken);
        if (doctor == null) throw NotFoundException.For<Doctor>(id);

        if (!string.IsNullOrEmpty(doctor.PhotoUrl)) _fileStorage.DeleteFile(doctor.PhotoUrl);

        var photoUrl = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "doctors", cancellationToken);
        doctor.PhotoUrl = photoUrl;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return photoUrl;
    }

    // --- Categories & Services ---
    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.ServiceCategories.ExistsAsync(c => c.Slug == slug, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə kateqoriya mövcuddur.");
        }

        var cat = new ServiceCategory
        {
            ParentId = request.ParentId,
            Slug = slug,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            DescriptionAz = request.DescriptionAz?.Trim(),
            DescriptionEn = request.DescriptionEn?.Trim(),
            DescriptionRu = request.DescriptionRu?.Trim(),
            IconKey = request.IconKey?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _unitOfWork.ServiceCategories.AddAsync(cat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryDto { Id = cat.Id, Slug = cat.Slug, Title = cat.TitleAz, DisplayOrder = cat.DisplayOrder };
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var cat = await _unitOfWork.ServiceCategories.GetByIdAsync(id, cancellationToken);
        if (cat == null) throw NotFoundException.For<ServiceCategory>(id);

        cat.ParentId = request.ParentId;
        cat.Slug = request.Slug.Trim().ToLowerInvariant();
        cat.TitleAz = request.TitleAz.Trim();
        cat.TitleEn = request.TitleEn?.Trim();
        cat.TitleRu = request.TitleRu?.Trim();
        cat.DescriptionAz = request.DescriptionAz?.Trim();
        cat.DescriptionEn = request.DescriptionEn?.Trim();
        cat.DescriptionRu = request.DescriptionRu?.Trim();
        cat.IconKey = request.IconKey?.Trim();
        cat.DisplayOrder = request.DisplayOrder;
        cat.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CategoryDto { Id = cat.Id, Slug = cat.Slug, Title = cat.TitleAz, DisplayOrder = cat.DisplayOrder };
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cat = await _unitOfWork.ServiceCategories.GetByIdAsync(id, cancellationToken);
        if (cat == null) throw NotFoundException.For<ServiceCategory>(id);

        _unitOfWork.ServiceCategories.Remove(cat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<MedicalServiceDetailDto> CreateServiceAsync(CreateMedicalServiceRequestDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.MedicalServices.ExistsAsync(s => s.Slug == slug, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə tibbi prosedur artıq mövcuddur.");
        }

        var s = new MedicalService
        {
            CategoryId = request.CategoryId,
            Slug = slug,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            AboutAz = request.AboutAz?.Trim(),
            AboutEn = request.AboutEn?.Trim(),
            AboutRu = request.AboutRu?.Trim(),
            PreparationAz = request.PreparationAz?.Trim(),
            PreparationEn = request.PreparationEn?.Trim(),
            PreparationRu = request.PreparationRu?.Trim(),
            StepsAz = request.StepsAz?.Trim(),
            StepsEn = request.StepsEn?.Trim(),
            StepsRu = request.StepsRu?.Trim(),
            PrecautionsAz = request.PrecautionsAz?.Trim(),
            PrecautionsEn = request.PrecautionsEn?.Trim(),
            PrecautionsRu = request.PrecautionsRu?.Trim(),
            ResultAz = request.ResultAz?.Trim(),
            ResultEn = request.ResultEn?.Trim(),
            ResultRu = request.ResultRu?.Trim(),
            DurationText = request.DurationText?.Trim(),
            IsMostSearched = request.IsMostSearched,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _unitOfWork.MedicalServices.AddAsync(s, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MedicalServiceDetailDto { Id = s.Id, Slug = s.Slug, Title = s.TitleAz, CategoryId = s.CategoryId };
    }

    public async Task<MedicalServiceDetailDto> UpdateServiceAsync(Guid id, UpdateMedicalServiceRequestDto request, CancellationToken cancellationToken = default)
    {
        var s = await _unitOfWork.MedicalServices.GetByIdAsync(id, cancellationToken);
        if (s == null) throw NotFoundException.For<MedicalService>(id);

        s.CategoryId = request.CategoryId;
        s.Slug = request.Slug.Trim().ToLowerInvariant();
        s.TitleAz = request.TitleAz.Trim();
        s.TitleEn = request.TitleEn?.Trim();
        s.TitleRu = request.TitleRu?.Trim();
        s.AboutAz = request.AboutAz?.Trim();
        s.AboutEn = request.AboutEn?.Trim();
        s.AboutRu = request.AboutRu?.Trim();
        s.PreparationAz = request.PreparationAz?.Trim();
        s.PreparationEn = request.PreparationEn?.Trim();
        s.PreparationRu = request.PreparationRu?.Trim();
        s.StepsAz = request.StepsAz?.Trim();
        s.StepsEn = request.StepsEn?.Trim();
        s.StepsRu = request.StepsRu?.Trim();
        s.PrecautionsAz = request.PrecautionsAz?.Trim();
        s.PrecautionsEn = request.PrecautionsEn?.Trim();
        s.PrecautionsRu = request.PrecautionsRu?.Trim();
        s.ResultAz = request.ResultAz?.Trim();
        s.ResultEn = request.ResultEn?.Trim();
        s.ResultRu = request.ResultRu?.Trim();
        s.DurationText = request.DurationText?.Trim();
        s.IsMostSearched = request.IsMostSearched;
        s.DisplayOrder = request.DisplayOrder;
        s.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new MedicalServiceDetailDto { Id = s.Id, Slug = s.Slug, Title = s.TitleAz, CategoryId = s.CategoryId };
    }

    public async Task DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var s = await _unitOfWork.MedicalServices.GetByIdAsync(id, cancellationToken);
        if (s == null) throw NotFoundException.For<MedicalService>(id);

        _unitOfWork.MedicalServices.Remove(s);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetClinicPriceAsync(SetClinicPriceRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.ClinicServices.FindAsync(
            cs => cs.ClinicId == request.ClinicId && cs.MedicalServiceId == request.MedicalServiceId,
            cancellationToken);

        if (existing != null)
        {
            existing.PriceFrom = request.PriceFrom;
            existing.Currency = request.Currency;
            existing.IsActive = request.IsActive;
        }
        else
        {
            await _unitOfWork.ClinicServices.AddAsync(new ClinicService
            {
                ClinicId = request.ClinicId,
                MedicalServiceId = request.MedicalServiceId,
                PriceFrom = request.PriceFrom,
                Currency = request.Currency,
                IsActive = request.IsActive
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // --- Treatment Packages ---
    public async Task<TreatmentPackageDto> CreatePackageAsync(CreateTreatmentPackageRequestDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.TreatmentPackages.ExistsAsync(p => p.Slug == slug, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə paket artıq mövcuddur.");
        }

        var p = new TreatmentPackage
        {
            Slug = slug,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            DescriptionAz = request.DescriptionAz?.Trim(),
            DescriptionEn = request.DescriptionEn?.Trim(),
            DescriptionRu = request.DescriptionRu?.Trim(),
            IncludesAz = request.IncludesAz?.Trim(),
            IncludesEn = request.IncludesEn?.Trim(),
            IncludesRu = request.IncludesRu?.Trim(),
            PriceFrom = request.PriceFrom,
            Currency = request.Currency,
            MedicalServiceId = request.MedicalServiceId,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _unitOfWork.TreatmentPackages.AddAsync(p, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TreatmentPackageDto { Id = p.Id, Slug = p.Slug, Title = p.TitleAz, PriceFrom = p.PriceFrom, Currency = p.Currency };
    }

    public async Task<TreatmentPackageDto> UpdatePackageAsync(Guid id, UpdateTreatmentPackageRequestDto request, CancellationToken cancellationToken = default)
    {
        var p = await _unitOfWork.TreatmentPackages.GetByIdAsync(id, cancellationToken);
        if (p == null) throw NotFoundException.For<TreatmentPackage>(id);

        p.Slug = request.Slug.Trim().ToLowerInvariant();
        p.TitleAz = request.TitleAz.Trim();
        p.TitleEn = request.TitleEn?.Trim();
        p.TitleRu = request.TitleRu?.Trim();
        p.DescriptionAz = request.DescriptionAz?.Trim();
        p.DescriptionEn = request.DescriptionEn?.Trim();
        p.DescriptionRu = request.DescriptionRu?.Trim();
        p.IncludesAz = request.IncludesAz?.Trim();
        p.IncludesEn = request.IncludesEn?.Trim();
        p.IncludesRu = request.IncludesRu?.Trim();
        p.PriceFrom = request.PriceFrom;
        p.Currency = request.Currency;
        p.MedicalServiceId = request.MedicalServiceId;
        p.DisplayOrder = request.DisplayOrder;
        p.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new TreatmentPackageDto { Id = p.Id, Slug = p.Slug, Title = p.TitleAz, PriceFrom = p.PriceFrom, Currency = p.Currency };
    }

    public async Task DeletePackageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var p = await _unitOfWork.TreatmentPackages.GetByIdAsync(id, cancellationToken);
        if (p == null) throw NotFoundException.For<TreatmentPackage>(id);

        _unitOfWork.TreatmentPackages.Remove(p);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> UpdatePackageCoverAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var p = await _unitOfWork.TreatmentPackages.GetByIdAsync(id, cancellationToken);
        if (p == null) throw NotFoundException.For<TreatmentPackage>(id);

        if (!string.IsNullOrEmpty(p.CoverUrl)) _fileStorage.DeleteFile(p.CoverUrl);

        var coverUrl = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "packages", cancellationToken);
        p.CoverUrl = coverUrl;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return coverUrl;
    }

    // --- Content ---
    public async Task<CmsPageDto> UpsertCmsPageAsync(CmsPageDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var existing = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == slug, cancellationToken);

        if (existing != null)
        {
            existing.TitleAz = request.Title.Trim();
            existing.HeadingAz = request.Heading?.Trim();
            existing.ContentAz = request.Content.Trim();
        }
        else
        {
            existing = new CmsPage
            {
                Slug = slug,
                TitleAz = request.Title.Trim(),
                HeadingAz = request.Heading?.Trim(),
                ContentAz = request.Content.Trim(),
                IsPublished = true
            };
            await _unitOfWork.CmsPages.AddAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<BlogPostDetailDto> CreateBlogPostAsync(BlogPostDetailDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var post = new BlogPost
        {
            Slug = slug,
            TitleAz = request.Title.Trim(),
            SummaryAz = request.Summary?.Trim(),
            ContentAz = request.Content.Trim(),
            CoverUrl = request.CoverUrl,
            PublishedAtUtc = DateTime.UtcNow,
            IsPublished = true
        };

        await _unitOfWork.BlogPosts.AddAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request;
    }

    public async Task DeleteBlogPostAsync(string slug, CancellationToken cancellationToken = default)
    {
        var post = await _unitOfWork.BlogPosts.FindAsync(b => b.Slug == slug, cancellationToken);
        if (post == null) throw new NotFoundException("Məqalə tapılmadı.");

        _unitOfWork.BlogPosts.Remove(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<FaqItemDto> CreateFaqAsync(FaqItemDto request, CancellationToken cancellationToken = default)
    {
        var faq = new FaqItem
        {
            QuestionAz = request.Question.Trim(),
            AnswerAz = request.Answer.Trim(),
            Category = request.Category?.Trim(),
            IsActive = true
        };

        await _unitOfWork.FaqItems.AddAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        request.Id = faq.Id;
        return request;
    }

    public async Task DeleteFaqAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var faq = await _unitOfWork.FaqItems.GetByIdAsync(id, cancellationToken);
        if (faq == null) throw NotFoundException.For<FaqItem>(id);

        _unitOfWork.FaqItems.Remove(faq);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PartnerDto> CreatePartnerAsync(PartnerDto request, CancellationToken cancellationToken = default)
    {
        var partner = new Partner
        {
            Name = request.Name.Trim(),
            LogoUrl = request.LogoUrl,
            WebsiteUrl = request.WebsiteUrl,
            PartnerType = request.PartnerType,
            IsActive = true
        };

        await _unitOfWork.Partners.AddAsync(partner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        request.Id = partner.Id;
        return request;
    }

    public async Task DeletePartnerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var partner = await _unitOfWork.Partners.GetByIdAsync(id, cancellationToken);
        if (partner == null) throw NotFoundException.For<Partner>(id);

        _unitOfWork.Partners.Remove(partner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

