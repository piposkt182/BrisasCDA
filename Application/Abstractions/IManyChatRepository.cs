using Domain.Models;

namespace Application.Abstractions
{
    public interface IManyChatRepository
    {
        Task<IEnumerable<User>> GetSurveyFieldsUsers();
    }
}
