using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using REST_API_TEMPLATE.Data;
using REST_API_TEMPLATE.DTO;
using REST_API_TEMPLATE.Models;

namespace REST_API_TEMPLATE.Services
{
    public class LocalImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnviroment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _appDbContext;

        public LocalImageRepository(IWebHostEnvironment webHostEnviroment, IHttpContextAccessor httpContextAccessor, AppDbContext appDbContext)
        {
            _webHostEnviroment = webHostEnviroment;
            _httpContextAccessor = httpContextAccessor;
            _appDbContext = appDbContext;
        }
        
        public Image Upload(Image image)
        {
            var localFilePath = Path.Combine(_webHostEnviroment.ContentRootPath,"Images",$"{image.FileName}{image.FileExtension}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            image.File.CopyTo(stream);

            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{image.FileName}{image.FileExtension}";
            image.FilePath = urlFilePath;

            _appDbContext.Images.Add(image);
            _appDbContext.SaveChanges();
            return image;
        }

        public List<Image> GetAllInfoImages()
        {
            var allImages = _appDbContext.Images.ToList();
            return allImages;
        }

        public (byte[],string,string) DownloadFile(int Id)
        {
            try
            {
                var FileById = _appDbContext.Images.Where(x => x.Id == Id).FirstOrDefault();
                var path = Path.Combine(_webHostEnviroment.ContentRootPath,"Images", $"{FileById.FileName}{FileById.FileExtension}");
                var stream = File.ReadAllBytes(path);
                var fileName = FileById.FileName + FileById.FileExtension;
                return (stream, "application/octet-sream", fileName);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
