using FindBook.Core.ContributorAggregate;

namespace FindBook.UseCases.Contributors.Get;

public record GetContributorQuery(ContributorId ContributorId) : IQuery<Result<ContributorDto>>;
