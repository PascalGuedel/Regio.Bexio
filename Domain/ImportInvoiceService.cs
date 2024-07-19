using AutoMapper;
using Microsoft.Extensions.Logging;
using Regio.Bexio.Domain.Constants;
using Regio.Bexio.Domain.Model;
using Regio.Bexio.Model;
// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Regio.Bexio.Domain;

internal interface IImportInvoiceService
{
    Task ImportInvoicesAsync();
}

internal class ImportInvoiceService(
    ILogger<ImportInvoiceService> logger,
    IMapper mapper,
    IContactService contactService,
    IInvoiceService invoiceService,
    IExcelService excelService) : IImportInvoiceService
{
    public async Task ImportInvoicesAsync()
    {
        logger.LogInformation("--- Start import invoices");

        try
        {
            var inputInvoices = excelService.GetInputInvoices().ToList();
            logger.LogInformation("Found {NumberOfInvoices} invoices in excel", inputInvoices.Count);

            for (var index = 0; index < inputInvoices.Count; index++)
            {
                logger.LogInformation("Process {actualInvoice}/{numberOfInvoices}", index + 1, inputInvoices.Count);

                var inputInvoice = inputInvoices[index];
                await ProcessInvoice(inputInvoice);
            }
        }
        catch (HttpRequestException hre)
        {
            logger.LogError(hre, "HttpRequest failed during import invoices");
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Error during import invoices");
        }
    }

    private async Task ProcessInvoice(InputInvoice inputInvoice)
    {
        // Validation existing Invoice
        var isInvoiceExisting = await invoiceService.IsInvoiceExistingAsync(BexioConstants.INVOICE_PREFIX + inputInvoice.Nr);
        if (isInvoiceExisting)
        {
            logger.LogInformation("Invoice {Nr} already exists", inputInvoice.Nr);
            return;
        }

        // Create or find contact
        var contactName = $"{inputInvoice.KDNr}/{inputInvoice.PRNr}";
        var existingContacts = (await contactService.SearchContactAsync(contactName)).ToList();
        ContactSearchResultDto contact;
        if (existingContacts.Count == 0)
        {
            logger.LogInformation("Contact {contactName} does not exists and will be created", contactName);
            var createdContact = await contactService.CreateContactAsync(inputInvoice);
            contact = mapper.Map<ContactSearchResultDto>(createdContact);
        }
        else if (existingContacts.Count > 1)
        {
            logger.LogWarning("Multiple contacts with name {contactName} found", contactName);
            contact = existingContacts[0];
        }
        else
        {
            logger.LogInformation("Contact {contactName} already exists", contactName);
            contact = existingContacts[0];
        }

        // Create Invoice
        await invoiceService.CreateInvoiceAsync(inputInvoice, contact.id.GetValueOrDefault());
    }
}
