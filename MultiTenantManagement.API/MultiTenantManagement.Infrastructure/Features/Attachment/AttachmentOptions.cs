namespace MultiTenantManagement.Infrastructure.Features.Attachment
{
    public class AttachmentOptions
    {
        public const string SectionName = "Attachment";

        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

        public List<string> AllowedExtensions { get; set; } = new()
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp",
            ".pdf"
        };

        public List<string> AllowedContentTypes { get; set; } = new()
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "application/pdf"
        };
    }
}
