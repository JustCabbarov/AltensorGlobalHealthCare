using System.Collections.Concurrent;
using AltensorGlobalHealthCare.Application.Interfaces.Repositories;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Persistence.Contexts;
using AltensorGlobalHealthCare.Persistence.Repositories;

namespace AltensorGlobalHealthCare.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(
        ApplicationDbContext context,
        IInquiryRepository inquiries,
        IOfferRepository offers,
        IClinicRepository clinics,
        IServiceCategoryRepository serviceCategories,
        IMedicalServiceRepository medicalServices)
    {
        _context = context;
        Inquiries = inquiries;
        Offers = offers;
        Clinics = clinics;
        ServiceCategories = serviceCategories;
        MedicalServices = medicalServices;
    }

    public IInquiryRepository Inquiries { get; }
    public IOfferRepository Offers { get; }
    public IClinicRepository Clinics { get; }
    public IServiceCategoryRepository ServiceCategories { get; }
    public IMedicalServiceRepository MedicalServices { get; }

    public IGenericRepository<Doctor> Doctors => Repository<Doctor>();
    public IGenericRepository<ClinicService> ClinicServices => Repository<ClinicService>();
    public IGenericRepository<DoctorMedicalService> DoctorMedicalServices => Repository<DoctorMedicalService>();
    public IGenericRepository<TreatmentPackage> TreatmentPackages => Repository<TreatmentPackage>();
    public IGenericRepository<InquiryDocument> InquiryDocuments => Repository<InquiryDocument>();
    public IGenericRepository<InquiryMessage> InquiryMessages => Repository<InquiryMessage>();
    public IGenericRepository<OfferAttachment> OfferAttachments => Repository<OfferAttachment>();
    public IGenericRepository<PatientProfile> PatientProfiles => Repository<PatientProfile>();
    public IGenericRepository<EmailVerificationCode> VerificationCodes => Repository<EmailVerificationCode>();
    public IGenericRepository<RefreshToken> RefreshTokens => Repository<RefreshToken>();
    public IGenericRepository<Review> Reviews => Repository<Review>();
    public IGenericRepository<CmsPage> CmsPages => Repository<CmsPage>();
    public IGenericRepository<BlogPost> BlogPosts => Repository<BlogPost>();
    public IGenericRepository<FaqItem> FaqItems => Repository<FaqItem>();
    public IGenericRepository<AboutPillar> AboutPillars => Repository<AboutPillar>();
    public IGenericRepository<TeamMember> TeamMembers => Repository<TeamMember>();
    public IGenericRepository<HowItWorksStep> HowItWorksSteps => Repository<HowItWorksStep>();
    public IGenericRepository<Partner> Partners => Repository<Partner>();
    public IGenericRepository<ContactMessage> ContactMessages => Repository<ContactMessage>();
    public IGenericRepository<NewsletterSubscriber> NewsletterSubscribers => Repository<NewsletterSubscriber>();

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        return (IGenericRepository<T>)_repositories.GetOrAdd(
            typeof(T),
            _ => new GenericRepository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

