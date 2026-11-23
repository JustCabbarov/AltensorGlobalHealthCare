using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AltensorGlobalHealthCare.Domain.Entities;

namespace AltensorGlobalHealthCare.Persistence.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(150);
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FullName).HasMaxLength(150).IsRequired();

        builder.HasOne(d => d.Clinic)
            .WithMany(c => c.Doctors)
            .HasForeignKey(d => d.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.User)
            .WithOne(u => u.DoctorProfile)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Slug).HasMaxLength(150).IsRequired();
        builder.HasIndex(c => new { c.ParentId, c.Slug }).IsUnique();
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter("\"ParentId\" IS NULL");
        builder.Property(c => c.TitleAz).HasMaxLength(200).IsRequired();
        builder.Property(c => c.MetaTitleAz).HasMaxLength(200);
        builder.Property(c => c.MetaTitleEn).HasMaxLength(200);
        builder.Property(c => c.MetaTitleRu).HasMaxLength(200);
        builder.Property(c => c.MetaDescriptionAz).HasMaxLength(500);
        builder.Property(c => c.MetaDescriptionEn).HasMaxLength(500);
        builder.Property(c => c.MetaDescriptionRu).HasMaxLength(500);

        builder.HasOne(c => c.Parent)
            .WithMany(p => p.SubCategories)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class MedicalServiceConfiguration : IEntityTypeConfiguration<MedicalService>
{
    public void Configure(EntityTypeBuilder<MedicalService> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(s => s.Slug).IsUnique();
        builder.Property(s => s.TitleAz).HasMaxLength(250).IsRequired();
        builder.Property(s => s.HeroImageUrl).HasMaxLength(500);
        builder.Property(s => s.PromoBannerUrl).HasMaxLength(500);

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.RelatedPackage)
            .WithMany()
            .HasForeignKey(s => s.RelatedPackageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DoctorMedicalServiceConfiguration : IEntityTypeConfiguration<DoctorMedicalService>
{
    public void Configure(EntityTypeBuilder<DoctorMedicalService> builder)
    {
        builder.HasKey(dm => dm.Id);
        builder.HasIndex(dm => new { dm.DoctorId, dm.MedicalServiceId }).IsUnique();

        builder.HasOne(dm => dm.Doctor)
            .WithMany(d => d.DoctorServices)
            .HasForeignKey(dm => dm.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dm => dm.MedicalService)
            .WithMany(s => s.DoctorServices)
            .HasForeignKey(dm => dm.MedicalServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ClinicServiceConfiguration : IEntityTypeConfiguration<ClinicService>
{
    public void Configure(EntityTypeBuilder<ClinicService> builder)
    {
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.PriceFrom).HasPrecision(18, 2);
        builder.Property(cs => cs.Currency).HasMaxLength(10).HasDefaultValue("EUR");

        builder.HasIndex(cs => new { cs.ClinicId, cs.MedicalServiceId }).IsUnique();

        builder.HasOne(cs => cs.Clinic)
            .WithMany(c => c.ClinicServices)
            .HasForeignKey(cs => cs.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.MedicalService)
            .WithMany(s => s.ClinicServices)
            .HasForeignKey(cs => cs.MedicalServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TreatmentPackageConfiguration : IEntityTypeConfiguration<TreatmentPackage>
{
    public void Configure(EntityTypeBuilder<TreatmentPackage> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.TitleAz).HasMaxLength(250).IsRequired();
        builder.Property(p => p.PriceFrom).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(10).HasDefaultValue("EUR");

        builder.HasOne(p => p.MedicalService)
            .WithMany()
            .HasForeignKey(p => p.MedicalServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InquiryNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(i => i.InquiryNumber).IsUnique();
        builder.Property(i => i.Symptoms).HasMaxLength(1000).IsRequired();

        builder.HasOne(i => i.Patient)
            .WithMany()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.MedicalService)
            .WithMany()
            .HasForeignKey(i => i.MedicalServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.AssignedDoctor)
            .WithMany()
            .HasForeignKey(i => i.AssignedDoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.AssignedCoordinator)
            .WithMany()
            .HasForeignKey(i => i.AssignedCoordinatorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class InquiryDocumentConfiguration : IEntityTypeConfiguration<InquiryDocument>
{
    public void Configure(EntityTypeBuilder<InquiryDocument> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(d => d.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();

        builder.HasOne(d => d.Inquiry)
            .WithMany(i => i.Documents)
            .HasForeignKey(d => d.InquiryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InquiryMessageConfiguration : IEntityTypeConfiguration<InquiryMessage>
{
    public void Configure(EntityTypeBuilder<InquiryMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.SenderName).HasMaxLength(150).IsRequired();
        builder.Property(m => m.SenderRole).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Body).HasMaxLength(4000).IsRequired();

        builder.HasOne(m => m.Inquiry)
            .WithMany(i => i.Messages)
            .HasForeignKey(m => m.InquiryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.SenderUser)
            .WithMany()
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Title).HasMaxLength(250).IsRequired();

        builder.Property(o => o.MedicalTreatmentPrice).HasPrecision(18, 2);
        builder.Property(o => o.AccommodationPrice).HasPrecision(18, 2);
        builder.Property(o => o.AirportTransferPrice).HasPrecision(18, 2);
        builder.Property(o => o.AirplaneTicketPrice).HasPrecision(18, 2);
        builder.Property(o => o.Currency).HasMaxLength(10).HasDefaultValue("EUR");

        builder.HasOne(o => o.Inquiry)
            .WithMany(i => i.Offers)
            .HasForeignKey(o => o.InquiryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Clinic)
            .WithMany(c => c.Offers)
            .HasForeignKey(o => o.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Doctor)
            .WithMany()
            .HasForeignKey(o => o.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class OfferAttachmentConfiguration : IEntityTypeConfiguration<OfferAttachment>
{
    public void Configure(EntityTypeBuilder<OfferAttachment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.StoragePath).HasMaxLength(500).IsRequired();

        builder.HasOne(a => a.Offer)
            .WithMany(o => o.Attachments)
            .HasForeignKey(a => a.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CmsPageConfiguration : IEntityTypeConfiguration<CmsPage>
{
    public void Configure(EntityTypeBuilder<CmsPage> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Slug).HasMaxLength(150).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.ParentSlug).HasMaxLength(150);
        builder.Property(p => p.PageType).HasMaxLength(50).HasDefaultValue("Standard").IsRequired();
        builder.Property(p => p.TitleAz).HasMaxLength(200).IsRequired();
        builder.Property(p => p.ContentAz).IsRequired();
        builder.Property(p => p.HeroImageUrl).HasMaxLength(500);
    }
}

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(b => b.Slug).IsUnique();
        builder.Property(b => b.TitleAz).HasMaxLength(300).IsRequired();
    }
}

public class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.QuestionAz).HasMaxLength(500).IsRequired();
        builder.Property(f => f.AnswerAz).IsRequired();
        builder.Property(f => f.Category).HasMaxLength(100);

        builder.HasOne(f => f.MedicalService)
            .WithMany(s => s.Faqs)
            .HasForeignKey(f => f.MedicalServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.ServiceCategory)
            .WithMany(c => c.Faqs)
            .HasForeignKey(f => f.ServiceCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.AuthorName).HasMaxLength(150).IsRequired();
        builder.Property(r => r.AuthorCountry).HasMaxLength(100);
        builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();

        builder.HasOne(r => r.Clinic)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.ClinicId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Doctor)
            .WithMany(d => d.Reviews)
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.MedicalService)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.MedicalServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AboutPillarConfiguration : IEntityTypeConfiguration<AboutPillar>
{
    public void Configure(EntityTypeBuilder<AboutPillar> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.TitleAz).HasMaxLength(200).IsRequired();
        builder.Property(p => p.IconKey).HasMaxLength(100);
    }
}

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.FullName).HasMaxLength(150).IsRequired();
        builder.Property(t => t.RoleAz).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Email).HasMaxLength(150);
        builder.Property(t => t.Phone).HasMaxLength(50);
        builder.Property(t => t.WhatsAppUrl).HasMaxLength(300);
        builder.Property(t => t.PhotoUrl).HasMaxLength(500);
    }
}

public class HowItWorksStepConfiguration : IEntityTypeConfiguration<HowItWorksStep>
{
    public void Configure(EntityTypeBuilder<HowItWorksStep> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TitleAz).HasMaxLength(250).IsRequired();
        builder.Property(s => s.ContentAz).IsRequired();
        builder.Property(s => s.CtaLabelAz).HasMaxLength(150);
        builder.Property(s => s.CtaLabelEn).HasMaxLength(150);
        builder.Property(s => s.CtaLabelRu).HasMaxLength(150);
        builder.Property(s => s.CtaUrl).HasMaxLength(300);
    }
}

