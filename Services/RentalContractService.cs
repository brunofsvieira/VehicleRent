using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Services
{
    public class RentalContractService : IRentalContractService
    {
        private static readonly int[] WebPageSizes = [10, 20, 50];
        private readonly IRentalContractRepository _repo;
        private readonly IClientRepository _clientRepo;
        private readonly IVehicleRepository _vehicleRepo;

        public RentalContractService(
            IRentalContractRepository repo,
            IClientRepository clientRepo,
            IVehicleRepository vehicleRepo)
        {
            _repo = repo;
            _clientRepo = clientRepo;
            _vehicleRepo = vehicleRepo;
        }

        public async Task<PagedResult<RentalContract>> GetPagedForWebAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = WebPageSizes.Contains(pageSize) ? pageSize : 10;

            var normalizedClientId = clientId.HasValue && clientId.Value > 0 ? clientId : null;
            var normalizedVehicleId = vehicleId.HasValue && vehicleId.Value > 0 ? vehicleId : null;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize, normalizedClientId, normalizedVehicleId);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize, normalizedClientId, normalizedVehicleId);
            }

            return paged;
        }

        public async Task<PagedResult<RentalContract>> GetPagedForApiAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = pageSize <= 0 || pageSize > 100 ? 10 : pageSize;

            var normalizedClientId = clientId.HasValue && clientId.Value > 0 ? clientId : null;
            var normalizedVehicleId = vehicleId.HasValue && vehicleId.Value > 0 ? vehicleId : null;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize, normalizedClientId, normalizedVehicleId);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize, normalizedClientId, normalizedVehicleId);
            }

            return paged;
        }

        public Task<RentalContract?> GetByIdAsync(long id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task<RentalContract> CreateAsync(long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage)
        {
            await EnsureForeignEntitiesExistAsync(clientId, vehicleId);

            if (await _repo.ExistsVehicleOverlapAsync(vehicleId, rentalStartDate, rentalEndDate))
            {
                throw new BusinessValidationException(BusinessErrorCodes.RentalVehicleOverlap, "Vehicle already has an overlapping rental contract.");
            }

            try
            {
                var contract = new RentalContract(clientId, vehicleId, rentalStartDate, rentalEndDate, initialMileage);
                await _repo.AddAsync(contract);
                return contract;
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(MapRentalValidationCode(ex), ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException(BusinessErrorCodes.RentalSaveFailed, "Unable to save rental contract.");
            }
        }

        public async Task UpdateAsync(long id, long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage)
        {
            var existing = await _repo.GetByIdForWriteAsync(id);
            if (existing is null)
            {
                throw new EntityNotFoundException($"Rental contract with id {id} was not found.");
            }

            await EnsureForeignEntitiesExistAsync(clientId, vehicleId);

            if (await _repo.ExistsVehicleOverlapAsync(vehicleId, rentalStartDate, rentalEndDate, id))
            {
                throw new BusinessValidationException(BusinessErrorCodes.RentalVehicleOverlap, "Vehicle already has an overlapping rental contract.");
            }

            try
            {
                existing.UpdateContract(clientId, vehicleId, rentalStartDate, rentalEndDate, initialMileage);
                await _repo.UpdateAsync(existing);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(MapRentalValidationCode(ex), ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException(BusinessErrorCodes.RentalUpdateFailed, "Unable to update rental contract.");
            }
        }

        public async Task DeleteAsync(long id, bool ensureExists)
        {
            if (ensureExists)
            {
                var existing = await _repo.GetByIdForWriteAsync(id);
                if (existing is null)
                {
                    throw new EntityNotFoundException($"Rental contract with id {id} was not found.");
                }
            }

            await _repo.DeleteAsync(id);
        }

        private async Task EnsureForeignEntitiesExistAsync(long clientId, long vehicleId)
        {
            if (await _clientRepo.GetByIdAsync(clientId) is null)
            {
                throw new BusinessValidationException(BusinessErrorCodes.RentalClientNotFound, "Selected client does not exist.");
            }

            if (await _vehicleRepo.GetByIdAsync(vehicleId) is null)
            {
                throw new BusinessValidationException(BusinessErrorCodes.RentalVehicleNotFound, "Selected vehicle does not exist.");
            }
        }

        private static int NormalizePage(int page)
        {
            return page <= 0 ? 1 : page;
        }

        private static string MapRentalValidationCode(ArgumentException ex)
        {
            return ex.ParamName switch
            {
                "clientId" => BusinessErrorCodes.RentalClientRequired,
                "vehicleId" => BusinessErrorCodes.RentalVehicleRequired,
                "rentalStartDate" => BusinessErrorCodes.RentalStartDatePast,
                "rentalEndDate" => BusinessErrorCodes.RentalEndDateInvalid,
                "initialMileage" => BusinessErrorCodes.RentalInitialMileageInvalid,
                _ => BusinessErrorCodes.GenericValidation
            };
        }
    }
}
