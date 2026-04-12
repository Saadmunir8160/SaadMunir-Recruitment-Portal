using Application.Commands.Product.Create;
using Application.DTOs;
using Application.DTOs.Support;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<CreateProductCommand, Product>()
            // .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            // .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            // .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            // .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            // .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
            // .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            // .ReverseMap();
            //CreateMap<CreateProductCommand, Product>().ReverseMap();
            
            // DealerVehicle mappings
            CreateMap<DealerVehicle, DealerVehicleDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedDate ?? src.CreatedDate));
            
            CreateMap<CreateDealerVehicleDTO, DealerVehicle>()
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.Type)) // Map to legacy field
                .ForMember(dest => dest.VehicleID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Dealer, opt => opt.Ignore())
                .ForMember(dest => dest.DealerOrders, opt => opt.Ignore());
            
            CreateMap<UpdateDealerVehicleDTO, DealerVehicle>()
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.Type)) // Map to legacy field
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Dealer, opt => opt.Ignore())
                .ForMember(dest => dest.DealerOrders, opt => opt.Ignore());

            // Support ticket mappings
            CreateMap<SupportTicket, SupportTicketDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TicketId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedDate ?? src.CreatedDate))
                .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Messages.Count))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

            CreateMap<SupportTicket, SupportTicketDetailDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TicketId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedDate ?? src.CreatedDate))
                .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Messages.Count))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
                .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages));

            CreateMap<SupportTicketAttachment, SupportTicketAttachmentDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AttachmentId));

            CreateMap<SupportTicketMessage, SupportTicketMessageDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MessageId));

            CreateMap<CreateSupportTicketDTO, SupportTicket>()
                .ForMember(dest => dest.TicketId, opt => opt.Ignore())
                .ForMember(dest => dest.TicketNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DealerId, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Dealer, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.Messages, opt => opt.Ignore());
        }
    }
}
