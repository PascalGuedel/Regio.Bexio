using AutoMapper;
using Regio.Bexio.Model;

namespace Regio.Bexio.Automapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ContactPostResponseDto, ContactSearchResultDto>();
        CreateMap<InvoiceGetDto, InvoicePutDto>();
    }
}
