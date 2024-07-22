namespace Regio.Bexio.Domain.Model;

public record GroupedContact(
    string Name1,
    string? Name2,
    int MinId,
    IList<int> Ids);