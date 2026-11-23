using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.DTOs.Catalog;
using AltensorGlobalHealthCare.Application.DTOs.Content;
using AltensorGlobalHealthCare.Application.Interfaces.Services;

namespace AltensorGlobalHealthCare.WebApi.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly IPublicPortalService _publicService;

    public PublicController(IPublicPortalService publicService)
    {
        _publicService = publicService;
    }

    private string? CurrentLang => HttpContext.Items["Language"] as string ?? Request.Query["lang"].ToString();

    /// <summary>
    /// Bütün tibbi kateqoriyalar və alt-kateqoriyaların iyerarxik ağacı
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetCategories(
        [FromQuery] bool headerOnly = false,
        [FromQuery] bool homeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _publicService.GetCategoriesAsync(CurrentLang, headerOnly, homeOnly, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(result));
    }

    /// <summary>
    /// Slug üzrə konkret kateqoriya, alt-kateqoriyalar, xidmətlər və əlaqəli klinika/həkim/rəylər
    /// </summary>
    [HttpGet("categories/{categorySlug}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailDto>>> GetCategoryBySlug(string categorySlug, CancellationToken cancellationToken)
    {
        var result = await _publicService.GetCategoryBySlugAsync(categorySlug, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<CategoryDetailDto>.Fail("Kateqoriya tapılmadı.", "CATEGORY_NOT_FOUND"));
        return Ok(ApiResponse<CategoryDetailDto>.Ok(result));
    }

    /// <summary>
    /// Valideyn kateqoriya və alt-kateqoriya slug-ı üzrə alt-kateqoriya detalları
    /// </summary>
    [HttpGet("categories/{categorySlug}/{subSlug}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailDto>>> GetSubCategoryBySlug(
        string categorySlug,
        string subSlug,
        CancellationToken cancellationToken)
    {
        var result = await _publicService.GetSubCategoryBySlugAsync(categorySlug, subSlug, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<CategoryDetailDto>.Fail("Alt-kateqoriya tapılmadı.", "CATEGORY_NOT_FOUND"));
        return Ok(ApiResponse<CategoryDetailDto>.Ok(result));
    }

    /// <summary>
    /// Konkret tibbi prosedurun ətraflı məlumatı və təklif edən klinikalar/qiymətlər
    /// </summary>
    [HttpGet("services/{slug}")]
    public async Task<ActionResult<ApiResponse<MedicalServiceDetailDto>>> GetServiceBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _publicService.GetServiceBySlugAsync(slug, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<MedicalServiceDetailDto>.Fail("Tibbi prosedur tapılmadı.", "SERVICE_NOT_FOUND"));
        return Ok(ApiResponse<MedicalServiceDetailDto>.Ok(result));
    }

    /// <summary>
    /// Ən çox axtarılan tibbi xidmətlər (Ana səhifə üçün)
    /// </summary>
    [HttpGet("services/most-searched")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MedicalServiceSummaryDto>>>> GetMostSearchedServices(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetMostSearchedServicesAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MedicalServiceSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Bütün "Hər şey daxil" (All-in-one) müalicə və səyahət paketləri
    /// </summary>
    [HttpGet("packages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TreatmentPackageDto>>>> GetPackages(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetPackagesAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TreatmentPackageDto>>.Ok(result));
    }

    /// <summary>
    /// Konkret müalicə paketinin detalları
    /// </summary>
    [HttpGet("packages/{slug}")]
    public async Task<ActionResult<ApiResponse<TreatmentPackageDto>>> GetPackageBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _publicService.GetPackageBySlugAsync(slug, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<TreatmentPackageDto>.Fail("Paket tapılmadı.", "PACKAGE_NOT_FOUND"));
        return Ok(ApiResponse<TreatmentPackageDto>.Ok(result));
    }

    /// <summary>
    /// Zaqrebdəki tərəfdaş klinikaların siyahısı
    /// </summary>
    [HttpGet("clinics")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClinicSummaryDto>>>> GetClinics(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetClinicsAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ClinicSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Konkret klinikanın profili, həkimləri və təklif etdiyi xidmətlər
    /// </summary>
    [HttpGet("clinics/{slug}")]
    public async Task<ActionResult<ApiResponse<ClinicDetailDto>>> GetClinicBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _publicService.GetClinicBySlugAsync(slug, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<ClinicDetailDto>.Fail("Klinika tapılmadı.", "CLINIC_NOT_FOUND"));
        return Ok(ApiResponse<ClinicDetailDto>.Ok(result));
    }

    /// <summary>
    /// Bütün ixtisaslı həkimlərin siyahısı
    /// </summary>
    [HttpGet("doctors")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DoctorDto>>>> GetDoctors(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetDoctorsAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DoctorDto>>.Ok(result));
    }

    /// <summary>
    /// Həkimin ID üzrə profili
    /// </summary>
    [HttpGet("doctors/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DoctorDto>>> GetDoctorById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _publicService.GetDoctorByIdAsync(id, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<DoctorDto>.Fail("Həkim tapılmadı.", "DOCTOR_NOT_FOUND"));
        return Ok(ApiResponse<DoctorDto>.Ok(result));
    }

    /// <summary>
    /// Pasient rəyləri (istəyə görə klinika və ya həkim ID ilə filtrlənir)
    /// </summary>
    [HttpGet("reviews")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ReviewDto>>>> GetReviews(
        [FromQuery] Guid? clinicId,
        [FromQuery] Guid? doctorId,
        CancellationToken cancellationToken)
    {
        var result = await _publicService.GetReviewsAsync(clinicId, doctorId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.Ok(result));
    }

    /// <summary>
    /// Xidmət, prosedur və simptomlar üzrə qlobal axtarış (ZagrebMed /search analoqu)
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MedicalServiceSummaryDto>>>> Search(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        var result = await _publicService.SearchAsync(q, CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MedicalServiceSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Haqqımızda səhifəsinin tam dinamik məzmunu (səhifə, alt-səhifələr, dəyərlər, komanda, addımlar)
    /// </summary>
    [HttpGet("about-us")]
    public async Task<ActionResult<ApiResponse<AboutUsDto>>> GetAboutUs(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetAboutUsAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<AboutUsDto>.Ok(result));
    }

    /// <summary>
    /// Necə işləyir səhifəsi və addımları
    /// </summary>
    [HttpGet("how-it-works")]
    public async Task<ActionResult<ApiResponse<HowItWorksDto>>> GetHowItWorks(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetHowItWorksAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<HowItWorksDto>.Ok(result));
    }

    /// <summary>
    /// Menyu üçün dinamik CMS səhifələrinin siyahısı (məsələn: ?parentSlug=about-us)
    /// </summary>
    [HttpGet("pages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CmsPageMenuItemDto>>>> GetCmsPagesMenu(
        [FromQuery] string? parentSlug,
        CancellationToken cancellationToken)
    {
        var result = await _publicService.GetCmsPagesMenuAsync(parentSlug, CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CmsPageMenuItemDto>>.Ok(result));
    }

    /// <summary>
    /// Dinamik CMS səhifələri (məsələn: about-us, how-it-works, terms)
    /// </summary>
    [HttpGet("pages/{slug}")]
    public async Task<ActionResult<ApiResponse<CmsPageDto>>> GetCmsPage(string slug, CancellationToken cancellationToken)
    {
        var result = await _publicService.GetCmsPageAsync(slug, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<CmsPageDto>.Fail("Səhifə tapılmadı.", "PAGE_NOT_FOUND"));
        return Ok(ApiResponse<CmsPageDto>.Ok(result));
    }

    /// <summary>
    /// Bloq məqalələri
    /// </summary>
    [HttpGet("blog")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BlogPostSummaryDto>>>> GetBlogPosts(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetBlogPostsAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BlogPostSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Bloq məqaləsinin tam mətni
    /// </summary>
    [HttpGet("blog/{slug}")]
    public async Task<ActionResult<ApiResponse<BlogPostDetailDto>>> GetBlogPostBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _publicService.GetBlogPostBySlugAsync(slug, CurrentLang, cancellationToken);
        if (result == null) return NotFound(ApiResponse<BlogPostDetailDto>.Fail("Məqalə tapılmadı.", "POST_NOT_FOUND"));
        return Ok(ApiResponse<BlogPostDetailDto>.Ok(result));
    }

    /// <summary>
    /// Tez-tez verilən suallar (F.A.Q.)
    /// </summary>
    [HttpGet("faq")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FaqItemDto>>>> GetFaqItems(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetFaqItemsAsync(CurrentLang, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FaqItemDto>>.Ok(result));
    }

    /// <summary>
    /// Rəsmi institusional və logistika tərəfdaşları
    /// </summary>
    [HttpGet("partners")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PartnerDto>>>> GetPartners(CancellationToken cancellationToken)
    {
        var result = await _publicService.GetPartnersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PartnerDto>>.Ok(result));
    }

    /// <summary>
    /// Əlaqə formasının göndərilməsi (ZagrebMed /user/sendContactEmail analoqu)
    /// </summary>
    [HttpPost("contact")]
    public async Task<ActionResult<ApiResponse<string>>> SubmitContactMessage(
        [FromBody] ContactMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _publicService.SubmitContactMessageAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Xəbər bülleteninə abunəlik (ZagrebMed /user/subscribe analoqu)
    /// </summary>
    [HttpPost("newsletter")]
    public async Task<ActionResult<ApiResponse<string>>> SubscribeNewsletter(
        [FromBody] NewsletterRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _publicService.SubscribeNewsletterAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Pasient rəyinin göndərilməsi
    /// </summary>
    [HttpPost("reviews")]
    public async Task<ActionResult<ApiResponse<string>>> SubmitReview(
        [FromBody] CreateReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _publicService.SubmitReviewAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }
}

