using REST_API_TEMPLATE.DTO;
using REST_API_TEMPLATE.Models;

namespace REST_API_TEMPLATE.Services
{
    public interface IImageRepository
    {
        Image Upload(Image image);
        List<Image> GetAllInfoImages();
        (byte[], string, string) DownloadFile(int Id);
    }
}
