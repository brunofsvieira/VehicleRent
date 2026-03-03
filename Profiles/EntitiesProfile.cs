using AutoMapper;
using System;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;

namespace VehicleRent.Profiles
{
    /// <summary>
    /// Represents the EntitiesProfile component.
    /// </summary>
    public class EntitiesProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitiesProfile"/> class.
        /// </summary>
        public EntitiesProfile()
        {
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<CreateVehicleDto, Vehicle>(MemberList.None)
                .ConstructUsing(dto => new Vehicle(dto.Brand, dto.Model, dto.Fuel, dto.ManufacturingYear, dto.LicensePlate));

            CreateMap<Client, ClientDto>();
            CreateMap<CreateClientDto, Client>(MemberList.None)
                .ConstructUsing(dto => new Client(dto.Name, dto.Email, dto.PhoneNumber, dto.DriverLicense));

            CreateMap<RentalContract, RentalContractDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : string.Empty))
                .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src => src.Client != null ? src.Client.Email : string.Empty))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src =>
                    src.Vehicle != null ? src.Vehicle.LicensePlate : string.Empty))
                .ForMember(dest => dest.IsFinished, opt => opt.MapFrom(src => src.RentalEndDate.Date < DateTime.UtcNow.Date));
        }
    }
}
