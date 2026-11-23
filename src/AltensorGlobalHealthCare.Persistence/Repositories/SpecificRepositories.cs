using Microsoft.EntityFrameworkCore;
using AltensorGlobalHealthCare.Application.Interfaces.Repositories;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Persistence.Contexts;

namespace AltensorGlobalHealthCare.Persistence.Repositories;

public class InquiryRepository : GenericRepository<Inquiry>, IInquiryRepository
{
    public InquiryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Inquiry?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Patient)
            .Include(i => i.MedicalService)
                .ThenInclude(s => s!.Category)
            .Include(i => i.AssignedDoctor)
                .ThenInclude(d => d!.Clinic)
            .Include(i => i.Documents)
            .Include(i => i.Messages)
            .Include(i => i.Offers)
                .ThenInclude(o => o.Clinic)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<string> GenerateInquiryNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await DbSet.IgnoreQueryFilters().CountAsync(cancellationToken);
        return $"INQ-{year}-{(count + 1):D5}";
    }
}

public class OfferRepository : GenericRepository<Offer>, IOfferRepository
{
    public OfferRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Offer?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Clinic)
            .Include(o => o.Doctor)
            .Include(o => o.Attachments)
            .Include(o => o.Inquiry)
                .ThenInclude(i => i.Patient)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Offer>> GetByInquiryIdAsync(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Clinic)
            .Include(o => o.Doctor)
            .Include(o => o.Attachments)
            .Where(o => o.InquiryId == inquiryId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}

public class ClinicRepository : GenericRepository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Clinic?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Doctors.Where(d => d.IsActive))
            .Include(c => c.ClinicServices.Where(cs => cs.IsActive))
                .ThenInclude(cs => cs.MedicalService)
            .Include(c => c.Reviews.Where(r => r.IsPublished))
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }
}

public class ServiceCategoryRepository : GenericRepository<ServiceCategory>, IServiceCategoryRepository
{
    public ServiceCategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ServiceCategory>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.ParentId == null && c.IsActive)
            .Include(c => c.SubCategories.Where(sub => sub.IsActive).OrderBy(sub => sub.DisplayOrder))
                .ThenInclude(sub => sub.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.ClinicServices.Where(cs => cs.IsActive))
            .Include(c => c.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.ClinicServices.Where(cs => cs.IsActive))
            .AsSplitQuery()
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceCategory>> GetAllWithCountsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.SubCategories.OrderBy(sub => sub.DisplayOrder))
                .ThenInclude(sub => sub.Services)
            .Include(c => c.Services)
            .AsSplitQuery()
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceCategory?> GetBySlugWithDetailsAsync(string slug, string? parentSlug = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(c => c.IsActive && c.Slug == slug);

        query = string.IsNullOrWhiteSpace(parentSlug)
            ? query.Where(c => c.ParentId == null)
            : query.Where(c => c.Parent != null && c.Parent.IsActive && c.Parent.Slug == parentSlug);

        return await query
            .Include(c => c.Parent)
            .Include(c => c.SubCategories.Where(sub => sub.IsActive).OrderBy(sub => sub.DisplayOrder))
                .ThenInclude(sub => sub.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.ClinicServices.Where(cs => cs.IsActive))
                        .ThenInclude(cs => cs.Clinic)
            .Include(c => c.SubCategories.Where(sub => sub.IsActive))
                .ThenInclude(sub => sub.Services.Where(s => s.IsActive))
                    .ThenInclude(s => s.DoctorServices)
                        .ThenInclude(dm => dm.Doctor)
                            .ThenInclude(d => d.Clinic)
            .Include(c => c.SubCategories.Where(sub => sub.IsActive))
                .ThenInclude(sub => sub.Services.Where(s => s.IsActive))
                    .ThenInclude(s => s.Reviews.Where(r => r.IsPublished))
                        .ThenInclude(r => r.Clinic)
            .Include(c => c.SubCategories.Where(sub => sub.IsActive))
                .ThenInclude(sub => sub.Services.Where(s => s.IsActive))
                    .ThenInclude(s => s.Reviews.Where(r => r.IsPublished))
                        .ThenInclude(r => r.Doctor)
            .Include(c => c.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.ClinicServices.Where(cs => cs.IsActive))
                    .ThenInclude(cs => cs.Clinic)
            .Include(c => c.Services.Where(s => s.IsActive))
                .ThenInclude(s => s.DoctorServices)
                    .ThenInclude(dm => dm.Doctor)
                        .ThenInclude(d => d.Clinic)
            .Include(c => c.Services.Where(s => s.IsActive))
                .ThenInclude(s => s.Reviews.Where(r => r.IsPublished))
                    .ThenInclude(r => r.Clinic)
            .Include(c => c.Services.Where(s => s.IsActive))
                .ThenInclude(s => s.Reviews.Where(r => r.IsPublished))
                    .ThenInclude(r => r.Doctor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class MedicalServiceRepository : GenericRepository<MedicalService>, IMedicalServiceRepository
{
    public MedicalServiceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<MedicalService?> GetBySlugWithClinicsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Category)
                .ThenInclude(c => c.Parent)
            .Include(s => s.RelatedPackage)
            .Include(s => s.ClinicServices.Where(cs => cs.IsActive))
                .ThenInclude(cs => cs.Clinic)
            .Include(s => s.DoctorServices.OrderBy(dm => dm.DisplayOrder))
                .ThenInclude(dm => dm.Doctor)
                    .ThenInclude(d => d.Clinic)
            .Include(s => s.Faqs.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder))
            .Include(s => s.Reviews.Where(r => r.IsPublished).OrderByDescending(r => r.CreatedAtUtc))
                .ThenInclude(r => r.Clinic)
            .Include(s => s.Reviews.Where(r => r.IsPublished))
                .ThenInclude(r => r.Doctor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive, cancellationToken);
    }

    public async Task<MedicalService?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Category)
                .ThenInclude(c => c.Parent)
            .Include(s => s.RelatedPackage)
            .Include(s => s.ClinicServices)
                .ThenInclude(cs => cs.Clinic)
            .Include(s => s.DoctorServices.OrderBy(dm => dm.DisplayOrder))
                .ThenInclude(dm => dm.Doctor)
            .Include(s => s.Faqs.OrderBy(f => f.DisplayOrder))
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MedicalService>> GetAllWithDetailsForAdminAsync(
        Guid? categoryId = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(s => s.Category)
                .ThenInclude(c => c.Parent)
            .Include(s => s.ClinicServices)
            .Include(s => s.DoctorServices)
            .AsSplitQuery()
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == categoryId.Value || s.Category.ParentId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.TitleAz.ToLower().Contains(term) ||
                (s.TitleEn != null && s.TitleEn.ToLower().Contains(term)) ||
                (s.TitleRu != null && s.TitleRu.ToLower().Contains(term)) ||
                s.Slug.ToLower().Contains(term));
        }

        return await query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.TitleAz).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MedicalService>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<MedicalService>();

        var term = searchTerm.Trim().ToLowerInvariant();

        return await DbSet
            .Include(s => s.Category)
            .Include(s => s.ClinicServices.Where(cs => cs.IsActive))
            .AsSplitQuery()
            .Where(s => s.IsActive &&
                       (s.TitleAz.ToLower().Contains(term) ||
                       (s.TitleEn != null && s.TitleEn.ToLower().Contains(term)) ||
                       (s.TitleRu != null && s.TitleRu.ToLower().Contains(term)) ||
                       (s.AboutAz != null && s.AboutAz.ToLower().Contains(term))))
            .Take(30)
            .ToListAsync(cancellationToken);
    }
}

