using Domain.Models;

namespace Application.Abstractions
{
    public interface IAgreementRepository
    {
        Task<Agreement> GetAgreementByCellPhone(string cellPhoneNumber);
        Task<List<Agreement>> GetAllAgreements();
    }
}
