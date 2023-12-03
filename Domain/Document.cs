using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Document_Management.Domain
{
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid DocumentId { get; set; }
        public Guid CustomerId { get; set; } // Foreign key to user
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string FilePath { get; set; } // Path or reference to the stored document content

        // Navigation property to the associated customer
        public User Customer { get; set; }
    }
}
