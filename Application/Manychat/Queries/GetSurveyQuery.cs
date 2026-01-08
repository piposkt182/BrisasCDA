using Application.Abstractions.Interfaces.Queries;
using Domain.Models;

namespace Application.Manychat.Queries
{
    public record GetSurveyQuery : IQuery<IEnumerable<SurveyResult>>
    {
    }
}
