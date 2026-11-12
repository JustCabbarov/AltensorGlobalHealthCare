namespace AltensorGlobalHealthCare.Application.Common;

public record FileUploadDto(Stream Stream, string FileName, string ContentType, long Length);

