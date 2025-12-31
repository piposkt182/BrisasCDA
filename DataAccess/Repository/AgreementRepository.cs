using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class AgreementRepository : IAgreementRepository
    {
        private readonly CDABrisasDbContext _dbContext;
        public AgreementRepository(CDABrisasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Agreement> GetAgreementByCellPhone(string cellPhoneNumber) => await _dbContext.Agreements.Where(a => a.CellPhoneNumber == cellPhoneNumber).FirstOrDefaultAsync();
    }
}
