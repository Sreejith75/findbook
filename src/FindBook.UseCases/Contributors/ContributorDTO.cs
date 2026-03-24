using FindBook.Core.ContributorAggregate;

namespace FindBook.UseCases.Contributors;
public record ContributorDto(ContributorId Id, ContributorName Name, PhoneNumber PhoneNumber);
