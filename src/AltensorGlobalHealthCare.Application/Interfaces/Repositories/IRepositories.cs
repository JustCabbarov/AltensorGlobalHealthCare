using System.Linq.Expressions;
using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

public interface IInquiryRepository : IGenericRepository<Inquiry>
{
    Task<Inquiry?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> GenerateInquiryNumberAsync(CancellationToken cancellationToken = default);
}

public interface IOfferRepository : IGenericRepository<Offer>
{
    Task<Offer?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Offer>> GetByInquiryIdAsync(Guid inquiryId, CancellationToken cancellationToken = default);
}

public interface IClinicRepository : IGenericRepository<Clinic>
{
    Task<Clinic?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken = default);
}

public interface IServiceCategoryRepository : IGenericRepository<ServiceCategory>
{
    Task<IReadOnlyList<ServiceCategory>> GetHierarchyAsync(CancellationToken cancellationToken = default);
}

public interface IMedicalServiceRepository : IGenericRepository<MedicalService>
{
    Task<MedicalService?> GetBySlugWithClinicsAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalService>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
