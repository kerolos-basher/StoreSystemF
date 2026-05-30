using MediatR;

namespace Application.Abstractions.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse>;
