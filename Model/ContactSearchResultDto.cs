namespace Regio.Bexio.Model;

internal class ContactSearchResultDto
{
    public int? id { get; set; }
    public string? nr { get; set; }
    public int? contact_type_id { get; set; }
    public string? name_1 { get; set; }
    public object? name_2 { get; set; }
    public int? salutation_id { get; set; }
    public object? salutation_form { get; set; }
    public object? title_id { get; set; }
    public object? birthday { get; set; }
    public string? address { get; set; }
    public string? postcode { get; set; }
    public string? city { get; set; }
    public int? country_id { get; set; }
    public string? mail { get; set; }
    public string? mail_second { get; set; }
    public string? phone_fixed { get; set; }
    public string? phone_fixed_second { get; set; }
    public string? phone_mobile { get; set; }
    public string? fax { get; set; }
    public string? url { get; set; }
    public string? skype_name { get; set; }
    public string? remarks { get; set; }
    public object? language_id { get; set; }
    public bool? is_lead { get; set; }
    public string? contact_group_ids { get; set; }
    public object? contact_branch_ids { get; set; }
    public int? user_id { get; set; }
    public int? owner_id { get; set; }
    public string? updated_at { get; set; }
}
