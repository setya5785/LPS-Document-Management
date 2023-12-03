using Document_Management.Data;
using Document_Management.Domain;
using Microsoft.EntityFrameworkCore;

namespace Document_Management.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _dbContext;

        public DocumentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Document> SubmitDocument(Guid customerId, string fileName, string fileType, long fileSize, string filePath)
        {
            var document = new Document
            {
                CustomerId = customerId,
                FileName = fileName,
                FileType = fileType,
                FileSize = fileSize,
                UploadDate = DateTime.UtcNow,
                FilePath = filePath
                // Set other properties as needed
            };

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            return document;
        }

        public async Task<Document> GetDocumentById(Guid documentId)
        {
            // Retrieve the document from the database
            return await _dbContext.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId);
        }

        public async Task<List<Document>> GetDocumentsByCustomerId(Guid customerId)
        {
            // Retrieve the documents associated with the customer from the database
            return await _dbContext.Documents
                .Where(d => d.CustomerId == customerId)
                .ToListAsync();
        }
    }

}
