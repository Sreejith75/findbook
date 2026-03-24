using FindBook.Core.ContributorAggregate;
using Vogen;

namespace FindBook.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
internal partial class VogenEfCoreConverters;
