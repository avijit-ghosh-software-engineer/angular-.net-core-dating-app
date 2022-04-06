using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Repository
{
    public interface IDatingRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<User> GetUser(int id);
        Task<PageList<User>> GetUsers(UserParams userParams);
        Task<bool> SaveAll();
        Task<string> FileUpload(int UserID, IFormFile file);
        Task<Photo> GetPhoto(int Id);

        Task<Photo> GetMainPhotoForUser(int userId);
        Task<bool> FileDelete(Photo photo);

        Task<Like> GetLike(int userId, int recipientId);

        Task<Message> GetMessage(int id);
        Task<PageList<Message>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
    }
}