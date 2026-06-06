using Application.Products.Dtos;

namespace Application.ReturnReasons.Commands.CreateReturnReason;

public sealed record CreateReturnReasonCommand(string Name, bool IsReturnToStock) : ICommand<ReturnReasonLookupDto>;
