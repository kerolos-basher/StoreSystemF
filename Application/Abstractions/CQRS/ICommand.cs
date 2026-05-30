using MediatR;

namespace Application.Abstractions.CQRS;

public interface ICommand : IRequest;

public interface ICommand<out TResponse> : IRequest<TResponse>;
