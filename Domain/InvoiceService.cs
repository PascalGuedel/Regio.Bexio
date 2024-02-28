using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Regio.Bexio.Domain.Constants;
using Regio.Bexio.Domain.Model;
using Regio.Bexio.Infrastructure;
using Regio.Bexio.Model;

namespace Regio.Bexio.Domain;
internal interface IInvoiceService
{
    Task CreateInvoiceAsync(InputInvoice invoice, int contactId);

    Task<bool> IsInvoiceExistingAsync(string title);

    Task DeleteAllInvoicesAsync();
}

internal class InvoiceService(
    ILogger<InvoiceService> logger,
    IConfiguration configuration,
    IBexioHttpClient bexioClient) : IInvoiceService
{
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
                .TryParseExact(invoice.Datum, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var validTo)
                    ? validTo.AddMonths(1).ToString("dd.MM.yyyy") : null,
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
        HttpContent httpContent = new StringContent(JsonSerializer.Serialize(invoiceDto));
        var postResult = await bexioClient.PostAsync<InvoicePostResultDto>("/2.0/kb_invoice", httpContent);

        // Update Invoice status (issue invoice)
        await bexioClient.PostAsync<InvoicePostIssueResultDto>($"/2.0/kb_invoice/{postResult!.id}/issue", httpContent);
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

        HttpContent httpContent = new StringContent(JsonSerializer.Serialize(criteria));
        var response = await bexioClient.PostAsync<IEnumerable<InvoiceSearchResultDto>>("/2.0/kb_invoice/search", httpContent);

        return response?.Any() ?? false;
    }

    public async Task DeleteAllInvoicesAsync()
    {
        logger.LogInformation("--- Start deleting invoices");

        var invoices = await bexioClient.GetAsync<IList<InvoiceGetDto>>("/2.0/kb_invoice");
        if (invoices == null || !invoices.Any())
        {
            logger.LogInformation("No invoices to delete");
            return;
        }

        for (var index = 0; index < invoices.Count; index++)
        {
            var invoice = invoices[index];
            await bexioClient.DeleteAsync($"/2.0/kb_invoice/{invoice.id}");
            logger.LogInformation("Deleted invoice {actualInvoice}/{invoiceCount}", index + 1, invoices.Count);
        }

        logger.LogInformation("Deleted all invoices");
    }
}