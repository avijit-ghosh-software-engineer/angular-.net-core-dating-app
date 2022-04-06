using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Repository.Implementation {
    public class DatingRepository : IDatingRepository {
        private readonly DataContext dataContext;
        private readonly ImagesPathSettings ImagesPathSettings;
        [Obsolete]
        private readonly IHostingEnvironment hostingEnvironment;
        public DatingRepository (DataContext _dataContext, IHostingEnvironment _hostingEnvironment, ImagesPathSettings _ImagesPathSettings) {
            dataContext = _dataContext;
            hostingEnvironment = _hostingEnvironment;
            ImagesPathSettings = _ImagesPathSettings;
        }
        public void Add<T> (T entity) where T : class {
            dataContext.Add (entity);
        }
        public void Delete<T> (T entity) where T : class {
            dataContext.Remove (entity);
        }
        public async Task<User> GetUser (int id) {
            var user = await dataContext.Users.Include (x => x.Photos).FirstOrDefaultAsync (x => x.Id == id);
            return user;
        }
        public async Task<PageList<User>> GetUsers (UserParams userParams) {
            var users = dataContext.Users.Include (x => x.Photos)
                .OrderByDescending (x => x.LastActive).AsQueryable ();
            users = users.Where (x => x.Gender == userParams.Gender && x.Id != userParams.UserId);

            if (userParams.Likers) {
                var userLikers = await GetUserLikes (userParams.UserId, userParams.Likers);
                users = users.Where (x => userLikers.Contains (x.Id));
            }
            if (userParams.Likees) {
                var userLikees = await GetUserLikes (userParams.UserId, userParams.Likers);
                users = users.Where (x => userLikees.Contains (x.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99) {
                var minDob = DateTime.Today.AddYears (-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears (-userParams.MinAge);
                users = users.Where (x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
            }
            if (!string.IsNullOrEmpty (userParams.OrderBy)) {
                switch (userParams.OrderBy) {
                    case "created":
                        users = users.OrderByDescending (x => x.Created);
                        break;
                    default:
                        users = users.OrderByDescending (x => x.LastActive);
                        break;
                }
            }

            return await PageList<User>.CreateAsync (users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes (int id, bool likers) {
            var user = await dataContext.Users.Include (x => x.Likers).Include (x => x.Likees).FirstOrDefaultAsync (x => x.Id == id);
            if (likers) {
                return user.Likers.Where (x => x.LikeeId == id).Select (x => x.LikerId);
            } else {
                return user.Likees.Where (x => x.LikerId == id).Select (x => x.LikeeId);
            }
        }

        public async Task<bool> SaveAll () {
            return await dataContext.SaveChangesAsync () > 0;
        }

        [Obsolete]
        public async Task<string> FileUpload (int UserID, IFormFile file) {
            string uniqueFileName = null;
            string uploadPath = Path.Combine (hostingEnvironment.WebRootPath, ImagesPathSettings.UserImages + UserID);
            if (!Directory.Exists (uploadPath)) {
                Directory.CreateDirectory (uploadPath);
            }
            uniqueFileName = Guid.NewGuid ().ToString () + "_" + file.FileName;
            string filePath = Path.Combine (uploadPath, uniqueFileName);
            using (var fileStream = new FileStream (filePath, FileMode.Create)) {
                await file.CopyToAsync (fileStream);
            }
            return uniqueFileName;
        }

        public async Task<Photo> GetPhoto (int Id) {
            var photo = await dataContext.Photos.FirstOrDefaultAsync (x => x.Id == Id);
            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser (int userId) {
            return await dataContext.Photos.Where (x => x.UserId == userId).FirstOrDefaultAsync (x => x.IsMain);
        }

        [Obsolete]
        public async Task<bool> FileDelete (Photo photo) {
            string uploadPath = Path.Combine (hostingEnvironment.WebRootPath, ImagesPathSettings.UserImages + photo.UserId);
            var file = photo.Url.Split ('/') [photo.Url.Split ('/').Length - 1];
            string filePath = Path.Combine (uploadPath, file);
            if ((System.IO.File.Exists (filePath))) {
                System.IO.File.Delete (filePath);
            }
            dataContext.Photos.Remove (photo);

            return await dataContext.SaveChangesAsync () > 0;
        }

        public async Task<Like> GetLike (int userId, int recipientId) {
            return await dataContext.Likes.FirstOrDefaultAsync (x => x.LikerId == userId && x.LikeeId == recipientId);
        }

        public async Task<Message> GetMessage (int id) {
            return await dataContext.Messages.FirstOrDefaultAsync (x => x.Id == id);
        }
        public async Task<PageList<Message>> GetMessagesForUser (MessageParams messageParams) {
            var messages = dataContext.Messages.Include (x => x.Sender).ThenInclude (x => x.Photos)
                .Include (x => x.Recipient).ThenInclude (x => x.Photos).AsQueryable ();
            switch (messageParams.MessageContainer) {
                case "Inbox":
                    messages = messages.Where (x => x.RecipientId == messageParams.UserId && x.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where (x => x.SenderId == messageParams.UserId && x.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where (x => x.RecipientId == messageParams.UserId && x.IsRead == false && x.RecipientDeleted == false);
                    break;
            }
            messages = messages.OrderByDescending (x => x.MessageSent);
            return await PageList<Message>.CreateAsync (messages, messageParams.PageNumber, messageParams.PageSize);
        }
        public async Task<IEnumerable<Message>> GetMessageThread (int userId, int recipientId) {
            var messages = await dataContext.Messages.Include (x => x.Sender).ThenInclude (x => x.Photos)
                .Include (x => x.Recipient).ThenInclude (x => x.Photos).Where (x => x.RecipientId == userId && x.SenderId == recipientId && x.RecipientDeleted == false ||
                    x.RecipientId == recipientId && x.SenderId == userId && x.SenderDeleted == false).OrderByDescending (x => x.MessageSent).ToListAsync ();
            return messages;
        }
    }
}