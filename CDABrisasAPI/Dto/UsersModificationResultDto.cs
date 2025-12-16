using Application.Utilities.Dtos;
using Domain.Models;

namespace CDABrisasAPI.Dto
{
    public class UsersModificationResultDto
    {
        public bool HasChanges { get; set; }
        public int ModifiedCount { get; set; }
        public IEnumerable<User> Users { get; set; } = [];
    }
}
