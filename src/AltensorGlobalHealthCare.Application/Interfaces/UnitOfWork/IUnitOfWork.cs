using AltensorGlobalHealthCare.Application.Interfaces.Repositories;
using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Entities;

namespace AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IInquiryRepository Inquiries { get; }
    IOfferRepository Offers { get; }
    IClinicRepository Clinics { get; }
    IServiceCategoryRepository ServiceCategories { get; }
    IMedicalServiceRepository MedicalServices { get; }

    IGenericRepository<Doctor> Doctors { get; }
    IGenericRepository<ClinicService> ClinicServices { get; }
    IGenericRepository<TreatmentPackage> TreatmentPackages { get; }
    IGenericRepository<InquiryDocument> InquiryDocuments { get; }
    IGenericRepository<InquiryMessage> InquiryMessages { get; }
    IGenericRepository<OfferAttachment> OfferAttachments { get; }
    IGenericRepository<PatientProfile> PatientProfiles { get; }
    IGenericRepository<EmailVerificationCode> VerificationCodes { get; }
    IGenericRepository<RefreshToken> RefreshTokens { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<CmsPage> CmsPages { get; }
    IGenericRepository<BlogPost> BlogPosts { get; }
    IGenericRepository<FaqItem> FaqItems { get; }
    IGenericRepository<Partner> Partners { get; }
    IGenericRepository<ContactMessage> ContactMessages { get; }
    IGenericRepository<NewsletterSubscriber> NewsletterSubscribers { get; }

    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

