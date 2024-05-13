using Microsoft.AspNetCore.Http;
namespace REST_API_TEMPLATE.DTO
{
    public class ImageUpLoadRequestDTO
    {
        public IFormFile File { get; set; }
        public string FileName { get; set; }
        public string FileDescription { get; set; }
    }
}
