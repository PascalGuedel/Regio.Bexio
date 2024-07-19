using System.Collections.Immutable;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Regio.Bexio.Domain.Model;
using Regio.Bexio.Model;

namespace Regio.Bexio.Domain;

internal interface ICleanupService
{
    Task DeleteAllInvoicesAndContactsAsync();

    Task CleanupContactsAsync();
}

internal class CleanupService(
    ILogger<InvoiceService> logger,
    IMapper mapper,
    IContactService contactService,
    IInvoiceService invoiceService) : ICleanupService
{
    public async Task DeleteAllInvoicesAndContactsAsync()
    {
        Console.WriteLine("Do you really want to delete all contacts and invoices? (yes/no)");
        var answer = Console.ReadLine();

        if (answer != "yes")
        {
            return;
        }

        logger.LogInformation("Start delete data");

        try
        {
            await DeleteAllInvoicesAsync();
            await contactService.DeleteAllContactsAsync();

            logger.LogInformation("Data successfully deleted");
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Error during deleting data");
        }
    }

    private async Task DeleteAllInvoicesAsync()
    {
        logger.LogInformation("--- Start deleting invoices");

        var invoices = await invoiceService.GetInvoicesAsync();
        if (!invoices.Any())
        {
            logger.LogInformation("No invoices to delete");
            return;
        }

        for (var index = 0; index < invoices.Count; index++)
        {
            var invoice = invoices[index];

            await invoiceService.SetInvoiceToDraftAsync(invoice.id.GetValueOrDefault());

            await invoiceService.DeleteInvoiceAsync(invoice.id.GetValueOrDefault());
            logger.LogInformation("Deleted invoice {actualInvoice}/{invoiceCount}", index + 1, invoices.Count);
        }

        logger.LogInformation("Deleted all invoices");
    }

    public async Task CleanupContactsAsync()
    {
        logger.LogInformation("--- Start cleaning up contacts");

        var contacts = (await contactService.GetAllContactsAsync()).ToImmutableList();
        var groupedContacts = contacts
            .GroupBy(c => (c.name_1, c.name_2))
            .Select(g => new GroupedContact(
                g.Key.name_1,
                g.Key.name_2,
                g.Min(c => c.id),
                g.Select(c => c.id).ToImmutableList()))
            .ToImmutableList();

        var invoices = (await invoiceService.GetInvoicesAsync()).ToImmutableList();
        var invoicesToUpdate = GetInvoicesToUpdate(invoices, groupedContacts).ToImmutableList();

        await UpdateInvoicesAsync(invoicesToUpdate, invoices);

        await DeleteContactsAsync(groupedContacts, contacts);

        logger.LogInformation("Cleanup contacts completed");
    }

    private async Task DeleteContactsAsync(
        ImmutableList<GroupedContact> groupedContacts, 
        ImmutableList<ContactGetDto> contacts)
    {
        var minIdContacts = groupedContacts.Select(c => c.MinId).ToList();
        var allContactIds = contacts.Select(c => c.id).ToList();
        var contactIdsToDelete = allContactIds.Except(minIdContacts).ToList();

        if (contactIdsToDelete.Count == 0)
        {
            return;
        }

        Console.WriteLine(
            $"Do you really want to delete the unused contacts ({contactIdsToDelete.Count} from {allContactIds.Count})? (yes/no)");
        var answer = Console.ReadLine();

        if (answer != "yes")
        {
            return;
        }

        var contactCount = 1;
        foreach (var contactId in contactIdsToDelete)
        {
            await contactService.DeleteContactAsync(contactId);
            logger.LogInformation("Deleted contact {contactCount}/{contactIdsToDeleteCount}", contactCount,
                contactIdsToDelete.Count);
            contactCount++;
        }
    }

    private IImmutableList<(InvoiceGetDto, int)> GetInvoicesToUpdate(
        IImmutableList<InvoiceGetDto> invoices, 
        IImmutableList<GroupedContact> groupedContacts)
    {
        var invoicesToUpdate = new List<(InvoiceGetDto, int)>();
        
        foreach (var invoice in invoices)
        {
            if (groupedContacts.Any(i => i.MinId == invoice.contact_id))
            {
                continue;
            }

            var relevantContact = groupedContacts
                .FirstOrDefault(c => c.Ids.Contains(invoice.contact_id.GetValueOrDefault()));

            if (relevantContact == null)
            {
                continue;
            }

            invoicesToUpdate.Add((invoice, relevantContact.MinId));
        }

        return invoicesToUpdate.ToImmutableList();
    }

    private async Task UpdateInvoicesAsync(
        IImmutableList<(InvoiceGetDto, int)> invoicesToUpdate,
        IImmutableList<InvoiceGetDto> invoices)
    {
        Console.WriteLine(
            $"Do you really want to update all invoices with wrong contact relations " +
            $"({invoicesToUpdate.Count} from {invoices.Count} invoices) and archive the contacts? (yes/no)");
        var answer = Console.ReadLine();

        if (answer != "yes")
        {
            return;
        }

        var invoiceUpdateCount = 1;
        foreach (var invoiceToUpdate in invoicesToUpdate)
        {
            var putModel = mapper.Map<InvoicePutDto>(invoiceToUpdate.Item1);
            await invoiceService.UpdateInvoiceAsync(putModel, invoiceToUpdate.Item1.id.GetValueOrDefault());
            logger.LogInformation(
                "Updated invoice {invoiceUpdateCount}/{invoicesToUpdateCount}", 
                invoiceUpdateCount, invoicesToUpdate.Count);
            invoiceUpdateCount++;
        }
    }
}