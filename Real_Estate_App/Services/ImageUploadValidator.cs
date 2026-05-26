using Microsoft.AspNetCore.Http;

namespace Real_Estate_App.Services
{
    // Validates user-uploaded property images before they are written to
    // wwwroot. Without this, an uploaded ".svg"/".html" file could carry
    // script (stored XSS when served back) and a huge file is a DoS vector.
    // The allow-list on extension + content type and the size cap close both.
    public static class ImageUploadValidator
    {
        public const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/png", "image/gif", "image/webp" };

        // Returns true when the file is an acceptable image. On false, 'error'
        // holds a user-facing message. A null/empty file is treated as valid -
        // callers decide separately whether an image is required.
        public static bool IsValid(IFormFile? file, out string error)
        {
            error = string.Empty;

            if (file == null || file.Length == 0)
            {
                return true;
            }

            if (file.Length > MaxBytes)
            {
                error = "The image is too large. Please upload a file under 5 MB.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                error = "Unsupported image type. Allowed formats: JPG, PNG, GIF, WEBP.";
                return false;
            }

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                error = "The uploaded file does not appear to be a valid image.";
                return false;
            }

            return true;
        }
    }
}
