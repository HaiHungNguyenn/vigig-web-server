using Vigig.Common.Attribute;
using Vigig.Domain.Entities;
using Vigig.Domain.Entities.BaseEntities;
using Vigig.Domain.Interfaces;
using Vigig.Service.Models.Common;
using Vigig.Service.Models.Request.Wallet;

namespace Vigig.Service.Interfaces;

[ServiceRegister]
public interface ITransactionService
{
    Task<ServiceActionResult> GetAllAsync();

    Task<ServiceActionResult> GetById(Guid id);

    Task<ServiceActionResult> SearchTransaction(SearchUsingGet request);
    
    Task<ServiceActionResult> GetPaginatedResultAsync(BasePaginatedRequest request);

    Task ProcessTransactionAsync(CashEntity fee, Wallet wallet);
}