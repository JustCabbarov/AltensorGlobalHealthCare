using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Entities;

namespace AltensorGlobalHealthCare.Persistence.Contexts;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationCode> VerificationCodes => Set<EmailVerificationCode>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<ClinicService> ClinicServices => Set<ClinicService>();
    public DbSet<DoctorMedicalService> DoctorMedicalServices => Set<DoctorMedicalService>();
    public DbSet<TreatmentPackage> TreatmentPackages => Set<TreatmentPackage>();

    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<InquiryDocument> InquiryDocuments => Set<InquiryDocument>();
    public DbSet<InquiryMessage> InquiryMessages => Set<InquiryMessage>();

    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferAttachment> OfferAttachments => Set<OfferAttachment>();

    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<CmsPage> CmsPages => Set<CmsPage>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<AboutPillar> AboutPillars => Set<AboutPillar>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<HowItWorksStep> HowItWorksSteps => Set<HowItWorksStep>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for ISoftDelete entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var filter = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                builder.Entity(entityType.ClrType).HasQueryFilter(System.Linq.Expressions.Expression.Lambda(filter, parameter));
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = utcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

