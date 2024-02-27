namespace Regio.Bexio.Model;

internal class InvoicePostDto
{
    public object? title { get; set; }
    public int contact_id { get; set; }
    public object? contact_sub_id { get; set; }
    public int user_id { get; set; }
    public object? pr_project_id { get; set; }
    public int logopaper_id { get; set; }
    public int language_id { get; set; }
    public int bank_account_id { get; set; }
    public int currency_id { get; set; }
    public int payment_type_id { get; set; }
    public string? header { get; set; }
    public string? footer { get; set; }
    public int mwst_type { get; set; }
    public bool mwst_is_net { get; set; }
    public bool show_position_taxes { get; set; }
    public string? is_valid_from { get; set; }
    public string? is_valid_to { get; set; }
    public object? reference { get; set; }
    public object? api_reference { get; set; }
    public Position[]? positions { get; set; }
}
public class Position
{
    public string? amount { get; set; }
    public int unit_id { get; set; }
    public int account_id { get; set; }
    public int tax_id { get; set; }
    public string? text { get; set; }
    public string? unit_price { get; set; }
    public string? discount_in_percent { get; set; }
    public string? type { get; set; }
}