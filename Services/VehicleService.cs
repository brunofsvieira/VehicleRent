using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Repositories;
using VehicleRent.Services.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace VehicleRent.Services
{
    public class VehicleService : IVehicleService
    {
        private static readonly int[] WebPageSizes = [10, 20, 50];
        private readonly IVehicleRepository _repo;
        private readonly IRentalContractRepository _rentalContractRepo;

        public VehicleService(IVehicleRepository repo, IRentalContractRepository rentalContractRepo)
        {
            _repo = repo;
            _rentalContractRepo = rentalContractRepo;
        }

        public async Task<PagedResult<Vehicle>> GetPagedForWebAsync(int page, int pageSize)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = WebPageSizes.Contains(pageSize) ? pageSize : 10;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize);
            }

            await ApplyRentalStatusAsync(paged.Items);
            return paged;
        }

        public async Task<PagedResult<Vehicle>> GetPagedForApiAsync(int page, int pageSize)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = pageSize <= 0 || pageSize > 100 ? 10 : pageSize;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize);
            }

            await ApplyRentalStatusAsync(paged.Items);
            return paged;
        }

        public async Task<IReadOnlyList<Vehicle>> GetAllForSelectionAsync()
        {
            var vehicles = await _repo.GetAllForSelectionAsync();
            await ApplyRentalStatusAsync(vehicles);
            return vehicles;
        }

        public async Task<Vehicle?> GetByIdAsync(long id)
        {
            var vehicle = await _repo.GetByIdAsync(id);
            if (vehicle is null) return null;

            await ApplyRentalStatusAsync([vehicle]);
            return vehicle;
        }

        public async Task<Vehicle> CreateAsync(string brand, string model, string licensePlate, FuelType fuel, int manufacturingYear)
        {
            var normalizedPlate = (licensePlate ?? string.Empty).Trim().ToUpperInvariant();
            if (await _repo.ExistsByLicensePlateAsync(normalizedPlate))
            {
                throw new BusinessValidationException("License plate already exists.");
            }

            try
            {
                var entity = new Vehicle(brand, model, fuel, manufacturingYear, normalizedPlate);
                await _repo.AddAsync(entity);
                return entity;
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException("License plate already exists.");
            }
        }

        public async Task UpdateAsync(long id, string brand, string model, string licensePlate, FuelType fuel, int manufacturingYear)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null)
            {
                throw new EntityNotFoundException($"Vehicle with id {id} was not found.");
            }

            var normalizedPlate = (licensePlate ?? string.Empty).Trim().ToUpperInvariant();
            if (await _repo.ExistsByLicensePlateAsync(normalizedPlate, id))
            {
                throw new BusinessValidationException("License plate already exists.");
            }

            try
            {
                entity.UpdateVehicle(brand, model, fuel, manufacturingYear, normalizedPlate);
                await _repo.UpdateAsync(entity);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException("License plate already exists.");
            }
        }

        public async Task DeleteAsync(long id, bool ensureExists)
        {
            if (ensureExists)
            {
                var entity = await _repo.GetByIdAsync(id);
                if (entity is null)
                {
                    throw new EntityNotFoundException($"Vehicle with id {id} was not found.");
                }
            }

            await _repo.DeleteAsync(id);
        }

        private static int NormalizePage(int page)
        {
            return page <= 0 ? 1 : page;
        }

        private async Task ApplyRentalStatusAsync(IEnumerable<Vehicle> vehicles)
        {
            var rentedVehicleIds = await _rentalContractRepo.GetCurrentlyRentedVehicleIdsAsync(DateTime.UtcNow.Date);
            foreach (var vehicle in vehicles)
            {
                vehicle.SetRentalStatus(rentedVehicleIds.Contains(vehicle.Id));
            }
        }
    }
}
