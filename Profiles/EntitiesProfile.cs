using AutoMapper;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;

namespace VehicleRent.Profiles
{
    public class EntitiesProfile : Profile
    {
        public EntitiesProfile()
        {
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<CreateVehicleDto, Vehicle>(MemberList.None)
                .ConstructUsing(dto => new Vehicle(dto.Brand, dto.Model, dto.Fuel, dto.ManufacturingYear, dto.LicensePlate));
        }
    }
}
