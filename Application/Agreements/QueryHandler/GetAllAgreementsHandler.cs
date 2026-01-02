using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Agreements.Query;
using Domain.Models;

namespace Application.Agreements.QueryHandler
{
    public class GetAllAgreementsHandler : IQueryHandler<GetAllAgreementsQuery, IEnumerable<Agreement>>
    {
        private readonly IAgreementRepository _repository;
        public GetAllAgreementsHandler(IAgreementRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<Agreement>> HandleAsync(GetAllAgreementsQuery query, CancellationToken cancellationToken = default)
        {
            var agreements = await _repository.GetAllAgreements();
            return agreements.DistinctBy(a => a.Name);
        }
    }
}
