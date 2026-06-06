using Application.Returns.Dtos;

namespace Application.Returns.Commands.CreateReturn;

public sealed record CreateReturnCommand(
    long SalesInvoiceId,
    int ReturnReasonType,
    string Notes,
    IReadOnlyList<ReturnLineRequestDto> Items) : ICommand<CreateReturnResultDto>;
