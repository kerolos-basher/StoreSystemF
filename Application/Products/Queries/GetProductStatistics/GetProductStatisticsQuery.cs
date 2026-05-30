using Application.Products.Dtos;

namespace Application.Products.Queries.GetProductStatistics;

public sealed record GetProductStatisticsQuery(int LowStockThreshold = 10) : IQuery<ProductStatisticsDto>;
