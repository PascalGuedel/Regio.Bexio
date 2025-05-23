﻿namespace Regio.Bexio.Model;
internal record InvoicePostResultDto
{
    public int? id { get; set; }
    public string? document_nr { get; set; }
    public object? title { get; set; }
    public int? contact_id { get; set; }
    public object? contact_sub_id { get; set; }
    public int? user_id { get; set; }
    public object? project_id { get; set; }
    public int? logopaper_id { get; set; }
    public int? language_id { get; set; }
    public int? bank_account_id { get; set; }
    public int? currency_id { get; set; }
    public int? payment_type_id { get; set; }
    public string? header { get; set; }
    public string? footer { get; set; }
    public string? total_gross { get; set; }
    public string? total_net { get; set; }
    public string? total_taxes { get; set; }
    public string? total_received_payments { get; set; }
    public string? total_credit_vouchers { get; set; }
    public string? total_remaining_payments { get; set; }
    public string? total { get; set; }
    public float? total_rounding_difference { get; set; }
    public int? mwst_type { get; set; }
    public bool? mwst_is_net { get; set; }
    public bool? show_position_taxes { get; set; }
    public string? is_valid_from { get; set; }
    public string? is_valid_to { get; set; }
    public string? contact_address { get; set; }
    public int? kb_item_status_id { get; set; }
    public object? reference { get; set; }
    public object? api_reference { get; set; }
    public object? viewed_by_client_at { get; set; }
    public string? updated_at { get; set; }
    public int? esr_id { get; set; }
    public int? qr_invoice_id { get; set; }
    public string? template_slug { get; set; }
    public Tax[]? taxs { get; set; }
    public string? network_link { get; set; }
    public ResultPosition[]? positions { get; set; }
}

internal class Tax
{
    public string? percentage { get; set; }
    public string? value { get; set; }
}

internal class ResultPosition
{
    public int? id { get; set; }
    public string? amount { get; set; }
    public int? unit_id { get; set; }
    public int? account_id { get; set; }
    public string? unit_name { get; set; }
    public int? tax_id { get; set; }
    public string? tax_value { get; set; }
    public string? text { get; set; }
    public string? unit_price { get; set; }
    public string? discount_in_percent { get; set; }
    public string? position_total { get; set; }
    public string? pos { get; set; }
    public int? internal_pos { get; set; }
    public bool? is_optional { get; set; }
    public string? type { get; set; }
    public object? parent_id { get; set; }
}
