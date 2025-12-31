using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Agreements.Query;
using Domain.Models;

namespace Application.Agreements.QueryHandler
{
    public class GetAgreementByCellPhoneHandler : IQueryHandler<GetAgreementByCellPhoneQuery, Agreement>
    {
        private readonly IAgreementRepository _agreementsRepository;
        public GetAgreementByCellPhoneHandler(IAgreementRepository agreementsRepository)
        {
            _agreementsRepository = agreementsRepository;
        }
        public async Task<Agreement> HandleAsync(GetAgreementByCellPhoneQuery query, CancellationToken cancellationToken = default)
        {
            return await _agreementsRepository.GetAgreementByCellPhone(query.CellPHoneNumber);
        }
    }
}
