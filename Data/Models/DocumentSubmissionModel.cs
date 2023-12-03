using System.ComponentModel.DataAnnotations;

namespace Document_Management.Data.Models
{
    public class DocumentSubmissionModel
    {
        [Required(ErrorMessage = "File is required.")]
        [MaxFileSize(2L * 1024 * 1024 * 1024, ErrorMessage = "File size cannot exceed 2 GB.")] //set hard limit of 2GB size for file upload
        [AllowedFileExtensions(new[] { ".xlsx", ".pdf" }, ErrorMessage = "Invalid file type. Supported types: .xlsx, .pdf")]
        public IFormFile File { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;

        public MaxFileSizeAttribute(long maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file != null && file.Length > _maxFileSize)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;

        public AllowedFileExtensionsAttribute(string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file != null)
            {
                var fileExtension = Path.GetExtension(file.FileName);

                if (!_allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }

}
