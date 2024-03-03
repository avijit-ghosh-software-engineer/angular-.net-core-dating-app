using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.API.Entities
{
    public class UserLike
    {
        [NotMapped]
        public AppUser SourceUser { get; set; }
        public int SourceUserId { get; set; }
        [NotMapped]
        public AppUser LikedUser { get; set; }
        public int LikedUserId { get; set; }
    }
}