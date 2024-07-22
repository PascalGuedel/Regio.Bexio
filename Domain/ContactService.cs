using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Regio.Bexio.Domain.Model;
using Regio.Bexio.Infrastructure;
using Regio.Bexio.Model;

namespace Regio.Bexio.Domain;

internal interface IContactService
{
    Task<ContactPostResponseDto?> CreateContactAsync(InputInvoice inputInvoice);

    Task<IEnumerable<ContactSearchResultDto>> SearchContactAsync(string name);

    Task<IEnumerable<ContactGetDto>> GetAllContactsAsync();

    Task DeleteAllContactsAsync();

    Task DeleteContactAsync(int contactId);
}

internal class ContactService(
    ILogger<ContactService> logger,
    IBexioHttpClient bexioClient,
    IConfiguration configuration) : IContactService
{
    public async Task<ContactPostResponseDto?> CreateContactAsync(InputInvoice inputInvoice)
    {
        var contactPostDto = new ContactPostDto
        {
            name_1 = inputInvoice.ArEmpfName,
            name_2 = $"Kundennummer: {inputInvoice.KDNr}/{inputInvoice.PRNr}",
            salutation_id = configuration.GetValue<int>("Bexio:Contact:SalutationId"),
            country_id = configuration.GetValue<int>("Bexio:Contact:CountryId"),
            user_id = configuration.GetValue<int>("Bexio:UserId"),
            owner_id = configuration.GetValue<int>("Bexio:Contact:OwnerId"),
            contact_type_id = 1, // Firma
            address = inputInvoice.ArEmpfAdr1,
            postcode = int.TryParse(inputInvoice.ArEmpfAdr3, out var plz) ? plz : default,
            city = inputInvoice.ArEmpfAdr4
        };

        var response =
            await bexioClient.PostAsync<ContactPostDto, ContactPostResponseDto>("/2.0/contact", contactPostDto);

        return response;
    }

    public async Task<IEnumerable<ContactSearchResultDto>> SearchContactAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var criteria = new List<SearchCriteriaDto>
        {
            new()
            {
                criteria = "=",
                field = "name_2",
                value = $"Kundennummer: {name}"
            }
        };

        var response =
            await bexioClient.PostAsync<List<SearchCriteriaDto>, IEnumerable<ContactSearchResultDto>>(
                "/2.0/contact/search", criteria);

        return response ?? new List<ContactSearchResultDto>();
    }

    public async Task<IEnumerable<ContactGetDto>> GetAllContactsAsync()
    {
        var response =
            await bexioClient.GetAsync<IEnumerable<ContactGetDto>>("/2.0/contact?limit=2000");

        return response ?? new List<ContactGetDto>();
    }

    public async Task DeleteAllContactsAsync()
    {
        logger.LogInformation("--- Start deleting contacts");

        var contacts = await bexioClient.GetAsync<IList<ContactDto>>("/2.0/contact");

        if (contacts == null || !contacts.Any())
        {
            logger.LogInformation("No contacts to delete");
            return;
        }

        for (var index = 0; index < contacts.Count; index++)
        {
            var contact = contacts[index];
            await DeleteContactAsync(contact.id);
            logger.LogInformation("Deleted contact {actualContact}/{contactCount}", index + 1, contacts.Count);
        }
        
        logger.LogInformation("Deleted all contacts");
    }

    public async Task DeleteContactAsync(int contactId)
    {
        await bexioClient.DeleteAsync($"/2.0/contact/{contactId}");
    }
}