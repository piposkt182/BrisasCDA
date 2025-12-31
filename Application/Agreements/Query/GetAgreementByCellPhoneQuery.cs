using Application.Abstractions.Interfaces.Queries;
using Domain.Models;

namespace Application.Agreements.Query
{
    public record GetAgreementByCellPhoneQuery (string CellPHoneNumber) : IQuery<Agreement>
    {
    }
}
