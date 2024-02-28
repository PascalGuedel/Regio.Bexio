namespace Regio.Bexio.Model;
internal record SearchCriteriaDto
{
    public string? field { get; set; }

    public string? value { get; set; }

    public string? criteria { get; set; }
}
