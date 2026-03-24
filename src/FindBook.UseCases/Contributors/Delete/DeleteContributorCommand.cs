using FindBook.Core.ContributorAggregate;

namespace FindBook.UseCases.Contributors.Delete;

public record DeleteContributorCommand(ContributorId ContributorId) : ICommand<Result>;
