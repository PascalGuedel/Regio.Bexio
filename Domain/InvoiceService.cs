using System.Globalization;
using Microsoft.Extensions.Configuration;
using Regio.Bexio.Domain.Constants;
using Regio.Bexio.Domain.Model;
using Regio.Bexio.Infrastructure;
using Regio.Bexio.Model;

namespace Regio.Bexio.Domain;

internal interface IInvoiceService
{
    Task CreateInvoiceAsync(InputInvoice invoice, int contactId);

    Task<bool> IsInvoiceExistingAsync(string title);
    
    Task<IList<InvoiceGetDto>> GetInvoicesAsync();

    Task SetInvoiceToDraftAsync(int invoiceId);

    Task DeleteInvoiceAsync(int invoiceId);

    Task UpdateInvoiceAsync(InvoicePutDto invoice, int invoiceId);
}

internal class InvoiceService(
    IConfiguration configuration,
    IBexioHttpClient bexioClient) : IInvoiceService
{
    // Maximal supported value of bexio
    private const int INVOICE_GET_LIMIT = 2000;
    
    public async Task CreateInvoiceAsync(InputInvoice invoice, int contactId)
    {
        var invoiceDto = new InvoicePostDto
        {
            contact_id = contactId,
            title = BexioConstants.INVOICE_PREFIX + invoice.Nr,
            bank_account_id = configuration.GetValue<int>("Bexio:AccountId"),
            currency_id = configuration.GetValue<int>("Bexio:CurrencyId"),
            user_id = configuration.GetValue<int>("Bexio:UserId"),
            payment_type_id = configuration.GetValue<int>("Bexio:PaymentTypeId"),
            mwst_type = configuration.GetValue<int>("Bexio:MwstType"),
            mwst_is_net = configuration.GetValue<bool>("Bexio:MwstIsNet"),
            is_valid_from = invoice.Datum,
            is_valid_to = DateTime
                .TryParseExact(invoice.Datum, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var validTo)
                ? validTo.AddMonths(1).ToString("dd.MM.yyyy")
                : null,
            logopaper_id = configuration.GetValue<int>("Bexio:LogopaperId"),
            language_id = configuration.GetValue<int>("Bexio:LanguageId"),
            positions =
            [
                new Position
                {
                    amount = "1",
                    unit_id = configuration.GetValue<int>("Bexio:PositionUnitId"),
                    account_id = configuration.GetValue<int>("Bexio:PositionAccountId"),
                    tax_id = configuration.GetValue<int>("Bexio:TaxId"),
                    unit_price = invoice.Kundenpreis?.ToString(CultureInfo.InvariantCulture),
                    discount_in_percent = "0",
                    type = configuration.GetValue<string>("Bexio:PositionType")
                }
            ]
        };

        // Create Invoice
        var postResult =
            await bexioClient.PostAsync<InvoicePostDto, InvoicePostResultDto>("/2.0/kb_invoice", invoiceDto);

        // Update Invoice status (issue invoice)
        await bexioClient.PostAsync<InvoicePostDto, InvoicePostIssueResultDto>(
            $"/2.0/kb_invoice/{postResult!.id}/issue", invoiceDto);
    }

    public async Task<bool> IsInvoiceExistingAsync(string title)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);

        var criteria = new List<SearchCriteriaDto>
        {
            new()
            {
                criteria = "=",
                field = "title",
                value = title
            }
        };

        var response =
            await bexioClient.PostAsync<List<SearchCriteriaDto>, IEnumerable<InvoiceSearchResultDto>>(
                "/2.0/kb_invoice/search", criteria);

        return response?.Any() ?? false;
    }

    public async Task<IList<InvoiceGetDto>> GetInvoicesAsync()
    {
        return await bexioClient.GetAsync<IList<InvoiceGetDto>>($"/2.0/kb_invoice?limit={INVOICE_GET_LIMIT}") ?? new List<InvoiceGetDto>();
    }

    public async Task SetInvoiceToDraftAsync(int invoiceId)
    {
        await bexioClient.PostAsync($"/2.0/kb_invoice/{invoiceId}/revert_issue");
    }

    public async Task DeleteInvoiceAsync(int invoiceId)
    {
        await bexioClient.DeleteAsync($"/2.0/kb_invoice/{invoiceId}");
    }

    public async Task UpdateInvoiceAsync(InvoicePutDto invoice, int invoiceId)
    {
        await bexioClient.PostAsync($"/2.0/kb_invoice/{invoiceId}", invoice);
    }
}