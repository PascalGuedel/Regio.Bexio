namespace Regio.Bexio.Model;

public record InvoicePutDto
{
    public string? title { get; set; }
    public int? contact_id { get; set; }
    public object? contact_sub_id { get; set; }
    public int? user_id { get; set; }
    public object? pr_project_id { get; set; }
    public int? logopaper_id { get; set; }
    public int? language_id { get; set; }
    public int? bank_account_id { get; set; }
    public int? currency_id { get; set; }
    public int? payment_type_id { get; set; }
    public string? header { get; set; }
    public string? footer { get; set; }
    public int? mwst_type { get; set; }
    public bool? mwst_is_net { get; set; }
    public bool? show_position_taxes { get; set; }
    public string? is_valid_from { get; set; }
    public string? is_valid_to { get; set; }
    public object? reference { get; set; }
    public object? api_reference { get; set; }
    public string? template_slug { get; set; }
}


