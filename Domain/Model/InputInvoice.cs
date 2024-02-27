namespace Regio.Bexio.Domain.Model;
internal record InputInvoice
{
    public string? Nr { get; set; }

    public string? Datum { get; set; }

    public string? ArEmpfName { get; set; }

    public string? ArEmpfAdr1 { get; set; }

    public string? ArEmpfAdr3 { get; set; }

    public string? ArEmpfAdr4 { get; set; }

    public string? PRNr { get; set; }

    public string? Pos { get; set; }

    public decimal? GesamtInklMwst { get; set; }

    public string? KDNr { get; set; }

    public string? StornoVonNr { get; set; }

    public decimal? Brutto { get; set; }

    public double? MwstPercent { get; set; }

    public decimal? Mwst { get; set; }

    public decimal? Kundenpreis { get; set; }

    public decimal? Netto { get; set; }

    public string? Waehrung { get; set; }

    public string? ProduktName { get; set; }

    public string? Kundencode { get; set; }

}
