using Document_Management.Data.Models;
using Document_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Claims;

namespace Document_Management.Web.Controllers
{
    [Authorize] // Ensure that only authenticated users can access this endpoint
    [ApiController]
    [Route("api/documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // get logged user id and match role
        private Guid GetUserIdFromClaims(string role)
        {
            var claims = HttpContext.User.Claims;
            var userClaim = claims.FirstOrDefault(c => c.Type == "UserId");
            var userRoleClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);


            if (userClaim != null && Guid.TryParse(userClaim.Value, out var userId))
            {
                // only return user id if match with role
                if(userRoleClaim.Value == role)
                {
                    return userId;
                }
                else
                {
                    throw new InvalidOperationException("Your role dosen't permitt this action.");
                }   
            }

            // throw exception user not found
            throw new InvalidOperationException("User ID not found in claims.");
        }

        //save file to disk
        private string SaveFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // Ensure the uploads folder exists
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique file name
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitDocument([FromForm] DocumentSubmissionModel model)
        {
            // Validate the model here (e.g., file type, size, etc.)
            if (ModelState.IsValid)
            {
                // Get customerId authentication
                var customerId = GetUserIdFromClaims("Customer");

                // Save the file to a physical location or cloud storage
                var filePath = SaveFile(model.File);

                // Call the document service to store information in the database
                var document = await _documentService.SubmitDocument(
                    customerId,
                    model.File.FileName,
                    model.File.ContentType,
                    model.File.Length,
                    filePath
                );

                // implement notification call here
                // still on todo list

                // return information on successfull document upload
                return Ok(new { DocumentId = document.DocumentId, FileName = document.FileName });
            }
            else
            {
                // Model validation failed, return validation errors
                return BadRequest(ModelState);
            }


        }

        [HttpGet("download/{documentId}")]
        public async Task<IActionResult> DownloadDocument(Guid documentId)
        {
            // Get business unit authentication
            _ = GetUserIdFromClaims("Unit");

            // Retrieve the document from the database
            var document = await _documentService.GetDocumentById(documentId);

            if (document == null)
            {
                return NotFound();
            }

            // Return the file as a FileStreamResult
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsFolder, document.FilePath);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(stream, document.FileType, document.FileName);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetDocumentsByCustomerId(Guid customerId)
        {
            // Retrieve the documents associated with the customer from the database
            var documents = await _documentService.GetDocumentsByCustomerId(customerId);

            if (documents == null || !documents.Any())
            {
                return NotFound();
            }

            // Return the list of documents
            return Ok(documents);
        }

        [HttpGet("download-all/{customerId}")]
        public async Task<IActionResult> DownloadAllDocumentsByCustomerId(Guid customerId)
        {
            // Get business unit authentication
            _ = GetUserIdFromClaims("Unit");

            // Retrieve the documents associated with the customer from the database
            var documents = await _documentService.GetDocumentsByCustomerId(customerId);

            if (documents == null || !documents.Any())
            {
                return NotFound();
            }

            var memoryStream = new MemoryStream();
            // Create a new zip archive
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add each document to the zip archive
                foreach (var document in documents)
                {
                    var entry = archive.CreateEntry(document.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var filePath = Path.Combine(uploadsFolder, document.FilePath);
                    using (var entryStream = entry.Open())
                    using (var fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            // Set the position of the memory stream to the beginning
            memoryStream.Position = 0;

            // Return the zip file
            return File(memoryStream, "application/zip", "AllDocuments.zip");
        }
    }

}
