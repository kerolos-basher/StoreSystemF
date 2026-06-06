namespace Application.ReturnReasons.Commands.UpdateReturnReason;

public sealed record UpdateReturnReasonCommand(int Id, string Name, bool IsReturnToStock) : ICommand;
