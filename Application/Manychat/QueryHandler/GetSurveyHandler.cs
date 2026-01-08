using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Manychat.Queries;
using Domain.Models;

namespace Application.Manychat.QueryHandler
{
    public class GetSurveyHandler : IQueryHandler<GetSurveyQuery, IEnumerable<SurveyResult>>
    {
        private readonly IUserRepository _userRepository;
        public GetSurveyHandler(IUserRepository userRepository)
        {
                _userRepository = userRepository;
        }
        public async Task<IEnumerable<SurveyResult>> HandleAsync(GetSurveyQuery query, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetSurveyUser();
        }
    }
}
