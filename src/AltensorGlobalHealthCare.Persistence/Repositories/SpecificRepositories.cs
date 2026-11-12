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
            .Include(c => c.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}

public class MedicalServiceRepository : GenericRepository<MedicalService>, IMedicalServiceRepository
{
    public MedicalServiceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<MedicalService?> GetBySlugWithClinicsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Category)
            .Include(s => s.ClinicServices.Where(cs => cs.IsActive))
                .ThenInclude(cs => cs.Clinic)
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<MedicalService>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<MedicalService>();

        var term = searchTerm.Trim().ToLowerInvariant();

        return await DbSet
            .Include(s => s.Category)
            .Where(s => s.IsActive &&
                       (s.TitleAz.ToLower().Contains(term) ||
                       (s.TitleEn != null && s.TitleEn.ToLower().Contains(term)) ||
                       (s.TitleRu != null && s.TitleRu.ToLower().Contains(term)) ||
                       (s.AboutAz != null && s.AboutAz.ToLower().Contains(term))))
            .Take(30)
            .ToListAsync(cancellationToken);
    }
}

