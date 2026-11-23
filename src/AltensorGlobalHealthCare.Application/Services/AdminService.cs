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
        var inquiries = await _unitOfWork.Inquiries.GetAllAsync(
            cancellationToken,
            i => i.MedicalService!,
            i => i.Documents,
            i => i.Offers);
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
                DocumentsCount = i.Documents.Count,
                OffersCount = i.Offers.Count,
                CreatedAtUtc = i.CreatedAtUtc
            }).ToList()
        };
    }

    // --- Clinics ---
    public async Task<IReadOnlyList<ClinicSummaryDto>> GetAllClinicsAsync(CancellationToken cancellationToken = default)
    {
        var clinics = await _unitOfWork.Clinics.GetAllAsync(cancellationToken, c => c.Doctors, c => c.ClinicServices);
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
        var doctor = await _unitOfWork.Doctors.FindAsync(d => d.Id == id, cancellationToken, d => d.Clinic);
        if (doctor == null) throw NotFoundException.For<Doctor>(id);

        var clinicName = doctor.Clinic?.Name ?? string.Empty;
        if (request.ClinicId != Guid.Empty && request.ClinicId != doctor.ClinicId)
        {
            var newClinic = await _unitOfWork.Clinics.GetByIdAsync(request.ClinicId, cancellationToken);
            if (newClinic == null) throw NotFoundException.For<Clinic>(request.ClinicId);
            doctor.ClinicId = newClinic.Id;
            clinicName = newClinic.Name;
        }

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
            ClinicName = clinicName,
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
    public async Task<IReadOnlyList<AdminCategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var all = await _unitOfWork.ServiceCategories.GetAllWithCountsAsync(cancellationToken);
        var roots = all.Where(c => c.ParentId == null).OrderBy(c => c.DisplayOrder).ToList();
        return roots.Select(MapAdminCategory).ToList();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (request.ParentId.HasValue)
        {
            var parent = await _unitOfWork.ServiceCategories.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent == null) throw NotFoundException.For<ServiceCategory>(request.ParentId.Value);
        }

        if (await _unitOfWork.ServiceCategories.ExistsAsync(c => c.ParentId == request.ParentId && c.Slug == slug, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə kateqoriya artıq mövcuddur.", "SLUG_ALREADY_EXISTS");
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
            ShowInHeaderNav = request.ShowInHeaderNav,
            ShowOnHomePage = request.ShowOnHomePage,
            MetaTitleAz = request.MetaTitleAz?.Trim(),
            MetaTitleEn = request.MetaTitleEn?.Trim(),
            MetaTitleRu = request.MetaTitleRu?.Trim(),
            MetaDescriptionAz = request.MetaDescriptionAz?.Trim(),
            MetaDescriptionEn = request.MetaDescriptionEn?.Trim(),
            MetaDescriptionRu = request.MetaDescriptionRu?.Trim(),
            IsActive = true
        };

        await _unitOfWork.ServiceCategories.AddAsync(cat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = cat.Id,
            ParentId = cat.ParentId,
            Slug = cat.Slug,
            Title = cat.TitleAz,
            DisplayOrder = cat.DisplayOrder,
            ShowInHeaderNav = cat.ShowInHeaderNav,
            ShowOnHomePage = cat.ShowOnHomePage
        };
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var cat = await _unitOfWork.ServiceCategories.GetByIdAsync(id, cancellationToken);
        if (cat == null) throw NotFoundException.For<ServiceCategory>(id);

        if (request.ParentId == id)
        {
            throw new BadRequestException("Kateqoriya özünün valideyni ola bilməz.", "INVALID_PARENT_CATEGORY");
        }

        if (request.ParentId.HasValue)
        {
            var allCategories = await _unitOfWork.ServiceCategories.GetAllAsync(cancellationToken);
            var parentMap = allCategories.ToDictionary(c => c.Id, c => c.ParentId);
            if (!parentMap.ContainsKey(request.ParentId.Value))
            {
                throw NotFoundException.For<ServiceCategory>(request.ParentId.Value);
            }

            var current = request.ParentId;
            while (current.HasValue)
            {
                if (current.Value == id)
                {
                    throw new BadRequestException("Kateqoriya iyerarxiyasında dövri istinada icazə verilmir.", "CIRCULAR_CATEGORY_REFERENCE");
                }
                parentMap.TryGetValue(current.Value, out current);
            }
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.ServiceCategories.ExistsAsync(c => c.ParentId == request.ParentId && c.Slug == slug && c.Id != id, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə kateqoriya artıq mövcuddur.", "SLUG_ALREADY_EXISTS");
        }

        cat.ParentId = request.ParentId;
        cat.Slug = slug;
        cat.TitleAz = request.TitleAz.Trim();
        cat.TitleEn = request.TitleEn?.Trim();
        cat.TitleRu = request.TitleRu?.Trim();
        cat.DescriptionAz = request.DescriptionAz?.Trim();
        cat.DescriptionEn = request.DescriptionEn?.Trim();
        cat.DescriptionRu = request.DescriptionRu?.Trim();
        cat.IconKey = request.IconKey?.Trim();
        cat.DisplayOrder = request.DisplayOrder;
        cat.ShowInHeaderNav = request.ShowInHeaderNav;
        cat.ShowOnHomePage = request.ShowOnHomePage;
        cat.MetaTitleAz = request.MetaTitleAz?.Trim();
        cat.MetaTitleEn = request.MetaTitleEn?.Trim();
        cat.MetaTitleRu = request.MetaTitleRu?.Trim();
        cat.MetaDescriptionAz = request.MetaDescriptionAz?.Trim();
        cat.MetaDescriptionEn = request.MetaDescriptionEn?.Trim();
        cat.MetaDescriptionRu = request.MetaDescriptionRu?.Trim();
        cat.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new CategoryDto
        {
            Id = cat.Id,
            ParentId = cat.ParentId,
            Slug = cat.Slug,
            Title = cat.TitleAz,
            DisplayOrder = cat.DisplayOrder,
            ShowInHeaderNav = cat.ShowInHeaderNav,
            ShowOnHomePage = cat.ShowOnHomePage
        };
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cat = await _unitOfWork.ServiceCategories.GetByIdAsync(id, cancellationToken);
        if (cat == null) throw NotFoundException.For<ServiceCategory>(id);

        _unitOfWork.ServiceCategories.Remove(cat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminMedicalServiceSummaryDto>> GetAllServicesAsync(
        Guid? categoryId = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var services = await _unitOfWork.MedicalServices.GetAllWithDetailsForAdminAsync(categoryId, search, cancellationToken);
        return services.Select(s =>
        {
            var minPrice = s.ClinicServices.Where(cs => cs.IsActive).OrderBy(cs => cs.PriceFrom).FirstOrDefault();
            return new AdminMedicalServiceSummaryDto
            {
                Id = s.Id,
                Slug = s.Slug,
                TitleAz = s.TitleAz,
                TitleEn = s.TitleEn,
                TitleRu = s.TitleRu,
                CategoryId = s.CategoryId,
                CategoryNameAz = s.Category?.TitleAz ?? string.Empty,
                CategoryNameEn = s.Category?.TitleEn,
                ParentCategoryId = s.Category?.ParentId,
                ParentCategoryNameAz = s.Category?.Parent?.TitleAz,
                ParentCategoryNameEn = s.Category?.Parent?.TitleEn,
                ClinicsCount = s.ClinicServices.Count,
                DoctorsCount = s.DoctorServices.Count,
                StartingPrice = minPrice?.PriceFrom,
                Currency = minPrice?.Currency ?? "EUR",
                IsActive = s.IsActive,
                IsMostSearched = s.IsMostSearched,
                DisplayOrder = s.DisplayOrder,
                HeroImageUrl = s.HeroImageUrl
            };
        }).ToList();
    }

    public async Task<AdminMedicalServiceDetailDto> GetServiceByIdForAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var s = await _unitOfWork.MedicalServices.GetByIdWithDetailsAsync(id, cancellationToken);
        if (s == null) throw NotFoundException.For<MedicalService>(id);

        return new AdminMedicalServiceDetailDto
        {
            Id = s.Id,
            CategoryId = s.CategoryId,
            Slug = s.Slug,
            TitleAz = s.TitleAz,
            TitleEn = s.TitleEn,
            TitleRu = s.TitleRu,
            AboutAz = s.AboutAz,
            AboutEn = s.AboutEn,
            AboutRu = s.AboutRu,
            CandidateAz = s.CandidateAz,
            CandidateEn = s.CandidateEn,
            CandidateRu = s.CandidateRu,
            PreparationAz = s.PreparationAz,
            PreparationEn = s.PreparationEn,
            PreparationRu = s.PreparationRu,
            StepsAz = s.StepsAz,
            StepsEn = s.StepsEn,
            StepsRu = s.StepsRu,
            PrecautionsAz = s.PrecautionsAz,
            PrecautionsEn = s.PrecautionsEn,
            PrecautionsRu = s.PrecautionsRu,
            ResultAz = s.ResultAz,
            ResultEn = s.ResultEn,
            ResultRu = s.ResultRu,
            PackageInfoAz = s.PackageInfoAz,
            PackageInfoEn = s.PackageInfoEn,
            PackageInfoRu = s.PackageInfoRu,
            HeroImageUrl = s.HeroImageUrl,
            PromoBannerUrl = s.PromoBannerUrl,
            RelatedPackageId = s.RelatedPackageId,
            DurationText = s.DurationText,
            IsMostSearched = s.IsMostSearched,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            ClinicPrices = s.ClinicServices.Select(cs => new AdminClinicPriceDto
            {
                ClinicId = cs.ClinicId,
                ClinicName = cs.Clinic?.Name ?? string.Empty,
                PriceFrom = cs.PriceFrom,
                Currency = cs.Currency,
                IsActive = cs.IsActive
            }).ToList(),
            DoctorIds = s.DoctorServices.OrderBy(dm => dm.DisplayOrder).Select(dm => dm.DoctorId).ToList(),
            Faqs = s.Faqs.OrderBy(f => f.DisplayOrder).Select(MapAdminFaq).ToList()
        };
    }

    public async Task<MedicalServiceDetailDto> CreateServiceAsync(CreateMedicalServiceRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.ServiceCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null) throw NotFoundException.For<ServiceCategory>(request.CategoryId);

        if (request.RelatedPackageId.HasValue)
        {
            var pkg = await _unitOfWork.TreatmentPackages.GetByIdAsync(request.RelatedPackageId.Value, cancellationToken);
            if (pkg == null) throw NotFoundException.For<TreatmentPackage>(request.RelatedPackageId.Value);
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.MedicalServices.ExistsAsync(s => s.Slug == slug, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə tibbi prosedur artıq mövcuddur.", "SLUG_ALREADY_EXISTS");
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
            CandidateAz = request.CandidateAz?.Trim(),
            CandidateEn = request.CandidateEn?.Trim(),
            CandidateRu = request.CandidateRu?.Trim(),
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
            PackageInfoAz = request.PackageInfoAz?.Trim(),
            PackageInfoEn = request.PackageInfoEn?.Trim(),
            PackageInfoRu = request.PackageInfoRu?.Trim(),
            RelatedPackageId = request.RelatedPackageId,
            DurationText = request.DurationText?.Trim(),
            IsMostSearched = request.IsMostSearched,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _unitOfWork.MedicalServices.AddAsync(s, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MedicalServiceDetailDto
        {
            Id = s.Id,
            Slug = s.Slug,
            Title = s.TitleAz,
            CategoryId = s.CategoryId,
            CategorySlug = category.Slug,
            CategoryName = category.TitleAz
        };
    }

    public async Task<MedicalServiceDetailDto> UpdateServiceAsync(Guid id, UpdateMedicalServiceRequestDto request, CancellationToken cancellationToken = default)
    {
        var s = await _unitOfWork.MedicalServices.GetByIdAsync(id, cancellationToken);
        if (s == null) throw NotFoundException.For<MedicalService>(id);

        var category = await _unitOfWork.ServiceCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null) throw NotFoundException.For<ServiceCategory>(request.CategoryId);

        if (request.RelatedPackageId.HasValue)
        {
            var pkg = await _unitOfWork.TreatmentPackages.GetByIdAsync(request.RelatedPackageId.Value, cancellationToken);
            if (pkg == null) throw NotFoundException.For<TreatmentPackage>(request.RelatedPackageId.Value);
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.MedicalServices.ExistsAsync(ms => ms.Slug == slug && ms.Id != id, cancellationToken))
        {
            throw new ConflictException("Bu slug ilə tibbi prosedur artıq mövcuddur.", "SLUG_ALREADY_EXISTS");
        }

        s.CategoryId = request.CategoryId;
        s.Slug = slug;
        s.TitleAz = request.TitleAz.Trim();
        s.TitleEn = request.TitleEn?.Trim();
        s.TitleRu = request.TitleRu?.Trim();
        s.AboutAz = request.AboutAz?.Trim();
        s.AboutEn = request.AboutEn?.Trim();
        s.AboutRu = request.AboutRu?.Trim();
        s.CandidateAz = request.CandidateAz?.Trim();
        s.CandidateEn = request.CandidateEn?.Trim();
        s.CandidateRu = request.CandidateRu?.Trim();
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
        s.PackageInfoAz = request.PackageInfoAz?.Trim();
        s.PackageInfoEn = request.PackageInfoEn?.Trim();
        s.PackageInfoRu = request.PackageInfoRu?.Trim();
        s.RelatedPackageId = request.RelatedPackageId;
        s.DurationText = request.DurationText?.Trim();
        s.IsMostSearched = request.IsMostSearched;
        s.DisplayOrder = request.DisplayOrder;
        s.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new MedicalServiceDetailDto
        {
            Id = s.Id,
            Slug = s.Slug,
            Title = s.TitleAz,
            CategoryId = s.CategoryId,
            CategorySlug = category.Slug,
            CategoryName = category.TitleAz
        };
    }

    public async Task DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var s = await _unitOfWork.MedicalServices.GetByIdAsync(id, cancellationToken);
        if (s == null) throw NotFoundException.For<MedicalService>(id);

        _unitOfWork.MedicalServices.Remove(s);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> UpdateServiceHeroImageAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var s = await _unitOfWork.MedicalServices.GetByIdAsync(id, cancellationToken);
        if (s == null) throw NotFoundException.For<MedicalService>(id);

        if (!string.IsNullOrEmpty(s.HeroImageUrl)) _fileStorage.DeleteFile(s.HeroImageUrl);

        var url = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "services/heroes", cancellationToken);
        s.HeroImageUrl = url;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    public async Task<string> UpdateServicePromoBannerAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var s = await _unitOfWork.MedicalServices.GetByIdAsync(id, cancellationToken);
        if (s == null) throw NotFoundException.For<MedicalService>(id);

        if (!string.IsNullOrEmpty(s.PromoBannerUrl)) _fileStorage.DeleteFile(s.PromoBannerUrl);

        var url = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "services/banners", cancellationToken);
        s.PromoBannerUrl = url;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    public async Task SetClinicPriceAsync(SetClinicPriceRequestDto request, CancellationToken cancellationToken = default)
    {
        var clinicExists = await _unitOfWork.Clinics.ExistsAsync(c => c.Id == request.ClinicId, cancellationToken);
        if (!clinicExists) throw NotFoundException.For<Clinic>(request.ClinicId);

        var serviceExists = await _unitOfWork.MedicalServices.ExistsAsync(s => s.Id == request.MedicalServiceId, cancellationToken);
        if (!serviceExists) throw NotFoundException.For<MedicalService>(request.MedicalServiceId);

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

    public async Task DeleteClinicPriceAsync(Guid serviceId, Guid clinicId, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.ClinicServices.FindAsync(
            cs => cs.MedicalServiceId == serviceId && cs.ClinicId == clinicId,
            cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException("Klinika üçün prosedur qiyməti tapılmadı.", "CLINIC_PRICE_NOT_FOUND");
        }

        _unitOfWork.ClinicServices.Remove(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetServiceDoctorsAsync(Guid serviceId, SetServiceDoctorsRequestDto request, CancellationToken cancellationToken = default)
    {
        var service = await _unitOfWork.MedicalServices.GetByIdAsync(serviceId, cancellationToken);
        if (service == null) throw NotFoundException.For<MedicalService>(serviceId);

        var requestedIds = (request.DoctorIds ?? Array.Empty<Guid>()).Distinct().ToList();
        if (requestedIds.Count > 0)
        {
            var validDoctors = await _unitOfWork.Doctors.GetListAsync(d => requestedIds.Contains(d.Id), cancellationToken);
            if (validDoctors.Count != requestedIds.Count)
            {
                var foundIds = validDoctors.Select(d => d.Id).ToHashSet();
                var missingId = requestedIds.First(id => !foundIds.Contains(id));
                throw NotFoundException.For<Doctor>(missingId);
            }
        }

        var existingLinks = await _unitOfWork.DoctorMedicalServices.GetListAsync(dm => dm.MedicalServiceId == serviceId, cancellationToken);
        var existingMap = existingLinks.ToDictionary(dm => dm.DoctorId);

        foreach (var existing in existingLinks.Where(e => !requestedIds.Contains(e.DoctorId)))
        {
            _unitOfWork.DoctorMedicalServices.Remove(existing);
        }

        for (var i = 0; i < requestedIds.Count; i++)
        {
            var doctorId = requestedIds[i];
            if (existingMap.TryGetValue(doctorId, out var existingLink))
            {
                existingLink.DisplayOrder = i + 1;
            }
            else
            {
                await _unitOfWork.DoctorMedicalServices.AddAsync(new DoctorMedicalService
                {
                    DoctorId = doctorId,
                    MedicalServiceId = serviceId,
                    DisplayOrder = i + 1
                }, cancellationToken);
            }
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

    // --- Content & About Us ---
    private static readonly HashSet<string> ProtectedSystemPageSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "about-us", "contact", "how-it-works"
    };

    public async Task<IReadOnlyList<AdminCmsPageDto>> GetAllCmsPagesAsync(CancellationToken cancellationToken = default)
    {
        var pages = await _unitOfWork.CmsPages.GetAllAsync(cancellationToken);
        return pages
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Slug)
            .Select(MapAdminCmsPage)
            .ToList();
    }

    public async Task<AdminCmsPageDto> GetCmsPageBySlugForAdminAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var page = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == normalizedSlug, cancellationToken);
        if (page == null) throw new NotFoundException($"Səhifə tapılmadı (Slug: {normalizedSlug}).", "PAGE_NOT_FOUND");
        return MapAdminCmsPage(page);
    }

    public async Task<AdminCmsPageDto> UpsertCmsPageAsync(UpsertCmsPageRequestDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new BadRequestException("Səhifə slug-ı məcburidir.", "SLUG_REQUIRED");
        }

        var titleAz = (!string.IsNullOrWhiteSpace(request.TitleAz) ? request.TitleAz : request.Title ?? string.Empty).Trim();
        var contentAz = (!string.IsNullOrWhiteSpace(request.ContentAz) ? request.ContentAz : request.Content ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(titleAz) || string.IsNullOrWhiteSpace(contentAz))
        {
            throw new BadRequestException("Səhifə başlığı və məzmunu (Az) məcburidir.", "PAGE_REQUIRED_FIELDS");
        }

        var headingAz = (!string.IsNullOrWhiteSpace(request.HeadingAz) ? request.HeadingAz : request.Heading)?.Trim();
        var pageType = string.IsNullOrWhiteSpace(request.PageType) ? "Standard" : request.PageType.Trim();
        var parentSlug = string.IsNullOrWhiteSpace(request.ParentSlug) ? null : request.ParentSlug.Trim().ToLowerInvariant();

        var existing = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == slug, cancellationToken);
        if (existing != null)
        {
            existing.ParentSlug = parentSlug;
            existing.PageType = pageType;
            existing.TitleAz = titleAz;
            existing.TitleEn = request.TitleEn?.Trim();
            existing.TitleRu = request.TitleRu?.Trim();
            existing.HeadingAz = headingAz;
            existing.HeadingEn = request.HeadingEn?.Trim();
            existing.HeadingRu = request.HeadingRu?.Trim();
            existing.SubtitleAz = request.SubtitleAz?.Trim();
            existing.SubtitleEn = request.SubtitleEn?.Trim();
            existing.SubtitleRu = request.SubtitleRu?.Trim();
            existing.ContentAz = contentAz;
            existing.ContentEn = request.ContentEn?.Trim();
            existing.ContentRu = request.ContentRu?.Trim();
            existing.MissionTitleAz = request.MissionTitleAz?.Trim();
            existing.MissionTitleEn = request.MissionTitleEn?.Trim();
            existing.MissionTitleRu = request.MissionTitleRu?.Trim();
            existing.MissionTextAz = request.MissionTextAz?.Trim();
            existing.MissionTextEn = request.MissionTextEn?.Trim();
            existing.MissionTextRu = request.MissionTextRu?.Trim();
            existing.HeroImageUrl = request.HeroImageUrl?.Trim();
            existing.ShowInMenu = request.ShowInMenu;
            existing.DisplayOrder = request.DisplayOrder;
            existing.IsPublished = request.IsPublished;
        }
        else
        {
            existing = new CmsPage
            {
                Slug = slug,
                ParentSlug = parentSlug,
                PageType = pageType,
                TitleAz = titleAz,
                TitleEn = request.TitleEn?.Trim(),
                TitleRu = request.TitleRu?.Trim(),
                HeadingAz = headingAz,
                HeadingEn = request.HeadingEn?.Trim(),
                HeadingRu = request.HeadingRu?.Trim(),
                SubtitleAz = request.SubtitleAz?.Trim(),
                SubtitleEn = request.SubtitleEn?.Trim(),
                SubtitleRu = request.SubtitleRu?.Trim(),
                ContentAz = contentAz,
                ContentEn = request.ContentEn?.Trim(),
                ContentRu = request.ContentRu?.Trim(),
                MissionTitleAz = request.MissionTitleAz?.Trim(),
                MissionTitleEn = request.MissionTitleEn?.Trim(),
                MissionTitleRu = request.MissionTitleRu?.Trim(),
                MissionTextAz = request.MissionTextAz?.Trim(),
                MissionTextEn = request.MissionTextEn?.Trim(),
                MissionTextRu = request.MissionTextRu?.Trim(),
                HeroImageUrl = request.HeroImageUrl?.Trim(),
                ShowInMenu = request.ShowInMenu,
                DisplayOrder = request.DisplayOrder,
                IsPublished = request.IsPublished
            };
            await _unitOfWork.CmsPages.AddAsync(existing, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminCmsPage(existing);
    }

    public async Task DeleteCmsPageAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        if (ProtectedSystemPageSlugs.Contains(normalizedSlug))
        {
            throw new BusinessRuleException($"Sistem səhifəsi ('{normalizedSlug}') silinə bilməz.", "SYSTEM_PAGE_PROTECTED");
        }

        var page = await _unitOfWork.CmsPages.FindAsync(p => p.Slug == normalizedSlug, cancellationToken);
        if (page == null) throw new NotFoundException($"Səhifə tapılmadı (Slug: {normalizedSlug}).", "PAGE_NOT_FOUND");

        _unitOfWork.CmsPages.Remove(page);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // --- About Pillars ---
    public async Task<IReadOnlyList<AdminAboutPillarDto>> GetAllAboutPillarsAsync(CancellationToken cancellationToken = default)
    {
        var pillars = await _unitOfWork.AboutPillars.GetAllAsync(cancellationToken);
        return pillars.OrderBy(p => p.DisplayOrder).Select(MapAdminPillar).ToList();
    }

    public async Task<AdminAboutPillarDto> CreateAboutPillarAsync(UpsertAboutPillarRequestDto request, CancellationToken cancellationToken = default)
    {
        var pillar = new AboutPillar
        {
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            DescriptionAz = request.DescriptionAz?.Trim(),
            DescriptionEn = request.DescriptionEn?.Trim(),
            DescriptionRu = request.DescriptionRu?.Trim(),
            IconKey = request.IconKey?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.AboutPillars.AddAsync(pillar, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminPillar(pillar);
    }

    public async Task<AdminAboutPillarDto> UpdateAboutPillarAsync(Guid id, UpsertAboutPillarRequestDto request, CancellationToken cancellationToken = default)
    {
        var pillar = await _unitOfWork.AboutPillars.GetByIdAsync(id, cancellationToken);
        if (pillar == null) throw NotFoundException.For<AboutPillar>(id);

        pillar.TitleAz = request.TitleAz.Trim();
        pillar.TitleEn = request.TitleEn?.Trim();
        pillar.TitleRu = request.TitleRu?.Trim();
        pillar.DescriptionAz = request.DescriptionAz?.Trim();
        pillar.DescriptionEn = request.DescriptionEn?.Trim();
        pillar.DescriptionRu = request.DescriptionRu?.Trim();
        pillar.IconKey = request.IconKey?.Trim();
        pillar.DisplayOrder = request.DisplayOrder;
        pillar.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminPillar(pillar);
    }

    public async Task DeleteAboutPillarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pillar = await _unitOfWork.AboutPillars.GetByIdAsync(id, cancellationToken);
        if (pillar == null) throw NotFoundException.For<AboutPillar>(id);

        _unitOfWork.AboutPillars.Remove(pillar);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // --- Team Members ---
    public async Task<IReadOnlyList<AdminTeamMemberDto>> GetAllTeamMembersAsync(CancellationToken cancellationToken = default)
    {
        var members = await _unitOfWork.TeamMembers.GetAllAsync(cancellationToken);
        return members.OrderBy(t => t.DisplayOrder).Select(MapAdminTeamMember).ToList();
    }

    public async Task<AdminTeamMemberDto> CreateTeamMemberAsync(UpsertTeamMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.IsPrimaryCoordinator)
        {
            await ClearOtherPrimaryCoordinatorsAsync(null, cancellationToken);
        }

        var member = new TeamMember
        {
            FullName = request.FullName.Trim(),
            RoleAz = request.RoleAz.Trim(),
            RoleEn = request.RoleEn?.Trim(),
            RoleRu = request.RoleRu?.Trim(),
            BioAz = request.BioAz?.Trim(),
            BioEn = request.BioEn?.Trim(),
            BioRu = request.BioRu?.Trim(),
            QuoteAz = request.QuoteAz?.Trim(),
            QuoteEn = request.QuoteEn?.Trim(),
            QuoteRu = request.QuoteRu?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            WhatsAppUrl = request.WhatsAppUrl?.Trim(),
            IsPrimaryCoordinator = request.IsPrimaryCoordinator,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.TeamMembers.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminTeamMember(member);
    }

    public async Task<AdminTeamMemberDto> UpdateTeamMemberAsync(Guid id, UpsertTeamMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.TeamMembers.GetByIdAsync(id, cancellationToken);
        if (member == null) throw NotFoundException.For<TeamMember>(id);

        if (request.IsPrimaryCoordinator)
        {
            await ClearOtherPrimaryCoordinatorsAsync(id, cancellationToken);
        }

        member.FullName = request.FullName.Trim();
        member.RoleAz = request.RoleAz.Trim();
        member.RoleEn = request.RoleEn?.Trim();
        member.RoleRu = request.RoleRu?.Trim();
        member.BioAz = request.BioAz?.Trim();
        member.BioEn = request.BioEn?.Trim();
        member.BioRu = request.BioRu?.Trim();
        member.QuoteAz = request.QuoteAz?.Trim();
        member.QuoteEn = request.QuoteEn?.Trim();
        member.QuoteRu = request.QuoteRu?.Trim();
        member.Email = request.Email?.Trim();
        member.Phone = request.Phone?.Trim();
        member.WhatsAppUrl = request.WhatsAppUrl?.Trim();
        member.IsPrimaryCoordinator = request.IsPrimaryCoordinator;
        member.DisplayOrder = request.DisplayOrder;
        member.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminTeamMember(member);
    }

    public async Task DeleteTeamMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.TeamMembers.GetByIdAsync(id, cancellationToken);
        if (member == null) throw NotFoundException.For<TeamMember>(id);

        _unitOfWork.TeamMembers.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> UpdateTeamMemberPhotoAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.TeamMembers.GetByIdAsync(id, cancellationToken);
        if (member == null) throw NotFoundException.For<TeamMember>(id);

        if (!string.IsNullOrEmpty(member.PhotoUrl)) _fileStorage.DeleteFile(member.PhotoUrl);

        var url = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "team", cancellationToken);
        member.PhotoUrl = url;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    // --- How It Works Steps ---
    public async Task<IReadOnlyList<AdminHowItWorksStepDto>> GetAllHowItWorksStepsAsync(CancellationToken cancellationToken = default)
    {
        var steps = await _unitOfWork.HowItWorksSteps.GetAllAsync(cancellationToken);
        return steps.OrderBy(s => s.DisplayOrder).ThenBy(s => s.StepNumber).Select(MapAdminStep).ToList();
    }

    public async Task<AdminHowItWorksStepDto> CreateHowItWorksStepAsync(UpsertHowItWorksStepRequestDto request, CancellationToken cancellationToken = default)
    {
        var step = new HowItWorksStep
        {
            StepNumber = request.StepNumber,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            ContentAz = request.ContentAz.Trim(),
            ContentEn = request.ContentEn?.Trim(),
            ContentRu = request.ContentRu?.Trim(),
            CtaLabelAz = request.CtaLabelAz?.Trim(),
            CtaLabelEn = request.CtaLabelEn?.Trim(),
            CtaLabelRu = request.CtaLabelRu?.Trim(),
            CtaUrl = request.CtaUrl?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.HowItWorksSteps.AddAsync(step, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminStep(step);
    }

    public async Task<AdminHowItWorksStepDto> UpdateHowItWorksStepAsync(Guid id, UpsertHowItWorksStepRequestDto request, CancellationToken cancellationToken = default)
    {
        var step = await _unitOfWork.HowItWorksSteps.GetByIdAsync(id, cancellationToken);
        if (step == null) throw NotFoundException.For<HowItWorksStep>(id);

        step.StepNumber = request.StepNumber;
        step.TitleAz = request.TitleAz.Trim();
        step.TitleEn = request.TitleEn?.Trim();
        step.TitleRu = request.TitleRu?.Trim();
        step.ContentAz = request.ContentAz.Trim();
        step.ContentEn = request.ContentEn?.Trim();
        step.ContentRu = request.ContentRu?.Trim();
        step.CtaLabelAz = request.CtaLabelAz?.Trim();
        step.CtaLabelEn = request.CtaLabelEn?.Trim();
        step.CtaLabelRu = request.CtaLabelRu?.Trim();
        step.CtaUrl = request.CtaUrl?.Trim();
        step.DisplayOrder = request.DisplayOrder;
        step.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminStep(step);
    }

    public async Task DeleteHowItWorksStepAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var step = await _unitOfWork.HowItWorksSteps.GetByIdAsync(id, cancellationToken);
        if (step == null) throw NotFoundException.For<HowItWorksStep>(id);

        _unitOfWork.HowItWorksSteps.Remove(step);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // --- Contact Messages ---
    public async Task<IReadOnlyList<ContactMessageDto>> GetContactMessagesAsync(bool? isHandled = null, CancellationToken cancellationToken = default)
    {
        var messages = isHandled.HasValue
            ? await _unitOfWork.ContactMessages.GetListAsync(m => m.IsHandled == isHandled.Value, cancellationToken)
            : await _unitOfWork.ContactMessages.GetAllAsync(cancellationToken);

        return messages
            .OrderByDescending(m => m.CreatedAtUtc)
            .Select(m => new ContactMessageDto
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Subject = m.Subject,
                Message = m.Message,
                IsHandled = m.IsHandled,
                HandledAtUtc = m.HandledAtUtc,
                CreatedAtUtc = m.CreatedAtUtc
            })
            .ToList();
    }

    public async Task<ContactMessageDto> HandleContactMessageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var msg = await _unitOfWork.ContactMessages.GetByIdAsync(id, cancellationToken);
        if (msg == null) throw NotFoundException.For<ContactMessage>(id);

        msg.IsHandled = true;
        msg.HandledAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ContactMessageDto
        {
            Id = msg.Id,
            Name = msg.Name,
            Email = msg.Email,
            Phone = msg.Phone,
            Subject = msg.Subject,
            Message = msg.Message,
            IsHandled = msg.IsHandled,
            HandledAtUtc = msg.HandledAtUtc,
            CreatedAtUtc = msg.CreatedAtUtc
        };
    }

    private async Task ClearOtherPrimaryCoordinatorsAsync(Guid? excludeId, CancellationToken cancellationToken)
    {
        var currentPrimaries = await _unitOfWork.TeamMembers.GetListAsync(
            t => t.IsPrimaryCoordinator && (!excludeId.HasValue || t.Id != excludeId.Value),
            cancellationToken);
        foreach (var other in currentPrimaries)
        {
            other.IsPrimaryCoordinator = false;
        }
    }

    private static AdminCmsPageDto MapAdminCmsPage(CmsPage p) => new()
    {
        Id = p.Id,
        Slug = p.Slug,
        ParentSlug = p.ParentSlug,
        PageType = p.PageType,
        TitleAz = p.TitleAz,
        TitleEn = p.TitleEn,
        TitleRu = p.TitleRu,
        HeadingAz = p.HeadingAz,
        HeadingEn = p.HeadingEn,
        HeadingRu = p.HeadingRu,
        SubtitleAz = p.SubtitleAz,
        SubtitleEn = p.SubtitleEn,
        SubtitleRu = p.SubtitleRu,
        ContentAz = p.ContentAz,
        ContentEn = p.ContentEn,
        ContentRu = p.ContentRu,
        MissionTitleAz = p.MissionTitleAz,
        MissionTitleEn = p.MissionTitleEn,
        MissionTitleRu = p.MissionTitleRu,
        MissionTextAz = p.MissionTextAz,
        MissionTextEn = p.MissionTextEn,
        MissionTextRu = p.MissionTextRu,
        HeroImageUrl = p.HeroImageUrl,
        ShowInMenu = p.ShowInMenu,
        DisplayOrder = p.DisplayOrder,
        IsPublished = p.IsPublished,
        CreatedAtUtc = p.CreatedAtUtc,
        UpdatedAtUtc = p.UpdatedAtUtc
    };

    private static AdminAboutPillarDto MapAdminPillar(AboutPillar p) => new()
    {
        Id = p.Id,
        TitleAz = p.TitleAz,
        TitleEn = p.TitleEn,
        TitleRu = p.TitleRu,
        DescriptionAz = p.DescriptionAz,
        DescriptionEn = p.DescriptionEn,
        DescriptionRu = p.DescriptionRu,
        IconKey = p.IconKey,
        DisplayOrder = p.DisplayOrder,
        IsActive = p.IsActive
    };

    private static AdminTeamMemberDto MapAdminTeamMember(TeamMember t) => new()
    {
        Id = t.Id,
        FullName = t.FullName,
        RoleAz = t.RoleAz,
        RoleEn = t.RoleEn,
        RoleRu = t.RoleRu,
        BioAz = t.BioAz,
        BioEn = t.BioEn,
        BioRu = t.BioRu,
        QuoteAz = t.QuoteAz,
        QuoteEn = t.QuoteEn,
        QuoteRu = t.QuoteRu,
        Email = t.Email,
        Phone = t.Phone,
        WhatsAppUrl = t.WhatsAppUrl,
        PhotoUrl = t.PhotoUrl,
        IsPrimaryCoordinator = t.IsPrimaryCoordinator,
        DisplayOrder = t.DisplayOrder,
        IsActive = t.IsActive
    };

    private static AdminHowItWorksStepDto MapAdminStep(HowItWorksStep s) => new()
    {
        Id = s.Id,
        StepNumber = s.StepNumber,
        TitleAz = s.TitleAz,
        TitleEn = s.TitleEn,
        TitleRu = s.TitleRu,
        ContentAz = s.ContentAz,
        ContentEn = s.ContentEn,
        ContentRu = s.ContentRu,
        CtaLabelAz = s.CtaLabelAz,
        CtaLabelEn = s.CtaLabelEn,
        CtaLabelRu = s.CtaLabelRu,
        CtaUrl = s.CtaUrl,
        DisplayOrder = s.DisplayOrder,
        IsActive = s.IsActive
    };

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

    public async Task<IReadOnlyList<AdminFaqItemDto>> GetAllFaqsAsync(Guid? medicalServiceId = null, CancellationToken cancellationToken = default)
    {
        var faqs = medicalServiceId.HasValue
            ? await _unitOfWork.FaqItems.GetListAsync(f => f.MedicalServiceId == medicalServiceId.Value, cancellationToken)
            : await _unitOfWork.FaqItems.GetAllAsync(cancellationToken);

        return faqs.OrderBy(f => f.DisplayOrder).Select(MapAdminFaq).ToList();
    }

    public async Task<AdminFaqItemDto> CreateFaqAsync(UpsertFaqRequestDto request, CancellationToken cancellationToken = default)
    {
        var questionAz = (!string.IsNullOrWhiteSpace(request.QuestionAz) ? request.QuestionAz : request.Question ?? string.Empty).Trim();
        var answerAz = (!string.IsNullOrWhiteSpace(request.AnswerAz) ? request.AnswerAz : request.Answer ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(questionAz) || string.IsNullOrWhiteSpace(answerAz))
        {
            throw new BadRequestException("Sual və cavab (Az) sahələri məcburidir.", "FAQ_REQUIRED_FIELDS");
        }

        if (request.MedicalServiceId.HasValue && !await _unitOfWork.MedicalServices.ExistsAsync(s => s.Id == request.MedicalServiceId.Value, cancellationToken))
        {
            throw NotFoundException.For<MedicalService>(request.MedicalServiceId.Value);
        }
        if (request.ServiceCategoryId.HasValue && !await _unitOfWork.ServiceCategories.ExistsAsync(c => c.Id == request.ServiceCategoryId.Value, cancellationToken))
        {
            throw NotFoundException.For<ServiceCategory>(request.ServiceCategoryId.Value);
        }

        var faq = new FaqItem
        {
            QuestionAz = questionAz,
            QuestionEn = request.QuestionEn?.Trim(),
            QuestionRu = request.QuestionRu?.Trim(),
            AnswerAz = answerAz,
            AnswerEn = request.AnswerEn?.Trim(),
            AnswerRu = request.AnswerRu?.Trim(),
            Category = request.Category?.Trim(),
            MedicalServiceId = request.MedicalServiceId,
            ServiceCategoryId = request.ServiceCategoryId,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.FaqItems.AddAsync(faq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapAdminFaq(faq);
    }

    public async Task<AdminFaqItemDto> UpdateFaqAsync(Guid id, UpsertFaqRequestDto request, CancellationToken cancellationToken = default)
    {
        var faq = await _unitOfWork.FaqItems.GetByIdAsync(id, cancellationToken);
        if (faq == null) throw NotFoundException.For<FaqItem>(id);

        var questionAz = (!string.IsNullOrWhiteSpace(request.QuestionAz) ? request.QuestionAz : request.Question ?? string.Empty).Trim();
        var answerAz = (!string.IsNullOrWhiteSpace(request.AnswerAz) ? request.AnswerAz : request.Answer ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(questionAz) || string.IsNullOrWhiteSpace(answerAz))
        {
            throw new BadRequestException("Sual və cavab (Az) sahələri məcburidir.", "FAQ_REQUIRED_FIELDS");
        }

        if (request.MedicalServiceId.HasValue && !await _unitOfWork.MedicalServices.ExistsAsync(s => s.Id == request.MedicalServiceId.Value, cancellationToken))
        {
            throw NotFoundException.For<MedicalService>(request.MedicalServiceId.Value);
        }
        if (request.ServiceCategoryId.HasValue && !await _unitOfWork.ServiceCategories.ExistsAsync(c => c.Id == request.ServiceCategoryId.Value, cancellationToken))
        {
            throw NotFoundException.For<ServiceCategory>(request.ServiceCategoryId.Value);
        }

        faq.QuestionAz = questionAz;
        faq.QuestionEn = request.QuestionEn?.Trim();
        faq.QuestionRu = request.QuestionRu?.Trim();
        faq.AnswerAz = answerAz;
        faq.AnswerEn = request.AnswerEn?.Trim();
        faq.AnswerRu = request.AnswerRu?.Trim();
        faq.Category = request.Category?.Trim();
        faq.MedicalServiceId = request.MedicalServiceId;
        faq.ServiceCategoryId = request.ServiceCategoryId;
        faq.DisplayOrder = request.DisplayOrder;
        faq.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapAdminFaq(faq);
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

    private static AdminCategoryDto MapAdminCategory(ServiceCategory c)
    {
        return new AdminCategoryDto
        {
            Id = c.Id,
            ParentId = c.ParentId,
            Slug = c.Slug,
            TitleAz = c.TitleAz,
            TitleEn = c.TitleEn,
            TitleRu = c.TitleRu,
            DescriptionAz = c.DescriptionAz,
            DescriptionEn = c.DescriptionEn,
            DescriptionRu = c.DescriptionRu,
            IconKey = c.IconKey,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ShowInHeaderNav = c.ShowInHeaderNav,
            ShowOnHomePage = c.ShowOnHomePage,
            MetaTitleAz = c.MetaTitleAz,
            MetaTitleEn = c.MetaTitleEn,
            MetaTitleRu = c.MetaTitleRu,
            MetaDescriptionAz = c.MetaDescriptionAz,
            MetaDescriptionEn = c.MetaDescriptionEn,
            MetaDescriptionRu = c.MetaDescriptionRu,
            ServicesCount = c.Services.Count,
            SubCategories = c.SubCategories.OrderBy(sub => sub.DisplayOrder).Select(MapAdminCategory).ToList()
        };
    }

    private static AdminFaqItemDto MapAdminFaq(FaqItem f)
    {
        return new AdminFaqItemDto
        {
            Id = f.Id,
            QuestionAz = f.QuestionAz,
            QuestionEn = f.QuestionEn,
            QuestionRu = f.QuestionRu,
            AnswerAz = f.AnswerAz,
            AnswerEn = f.AnswerEn,
            AnswerRu = f.AnswerRu,
            Category = f.Category,
            MedicalServiceId = f.MedicalServiceId,
            ServiceCategoryId = f.ServiceCategoryId,
            DisplayOrder = f.DisplayOrder,
            IsActive = f.IsActive
        };
    }
}

