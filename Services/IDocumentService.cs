using Document_Management.Domain;

namespace Document_Management.Services
{
    public interface IDocumentService
    {
        Task<Document> SubmitDocument(Guid customerId, string fileName, string fileType, long fileSize, string filePath);
        Task<Document> GetDocumentById(Guid documentId);
        Task<List<Document>> GetDocumentsByCustomerId(Guid customerId);
    }

}
