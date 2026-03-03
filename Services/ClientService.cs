using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using VehicleRent.Infrastructure;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Services
{
    /// <summary>
    /// Represents the ClientService component.
    /// </summary>
    public class ClientService : IClientService
    {
        private static readonly int[] WebPageSizes = [10, 20, 50];
        private readonly IClientRepository _repo;
        private readonly IRentalContractRepository _rentalContractRepo;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientService"/> class.
        /// </summary>
        public ClientService(IClientRepository repo, IRentalContractRepository rentalContractRepo, IDistributedCache cache)
        {
            _repo = repo;
            _rentalContractRepo = rentalContractRepo;
            _cache = cache;
        }

        /// <summary>
        /// Executes the GetPagedForWebAsync operation.
        /// </summary>
        public async Task<PagedResult<Client>> GetPagedForWebAsync(int page, int pageSize, string? nameOrEmail = null, long? vehicleId = null)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = WebPageSizes.Contains(pageSize) ? pageSize : 10;

            var normalizedNameOrEmail = string.IsNullOrWhiteSpace(nameOrEmail) ? null : nameOrEmail.Trim();
            var normalizedVehicleId = vehicleId.HasValue && vehicleId.Value > 0 ? vehicleId : null;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize, normalizedNameOrEmail, normalizedVehicleId);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize, normalizedNameOrEmail, normalizedVehicleId);
            }

            return paged;
        }

        /// <summary>
        /// Executes the GetPagedForApiAsync operation.
        /// </summary>
        public async Task<PagedResult<Client>> GetPagedForApiAsync(int page, int pageSize, string? nameOrEmail = null, long? vehicleId = null)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = pageSize <= 0 || pageSize > 100 ? 10 : pageSize;

            var normalizedNameOrEmail = string.IsNullOrWhiteSpace(nameOrEmail) ? null : nameOrEmail.Trim();
            var normalizedVehicleId = vehicleId.HasValue && vehicleId.Value > 0 ? vehicleId : null;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize, normalizedNameOrEmail, normalizedVehicleId);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize, normalizedNameOrEmail, normalizedVehicleId);
            }

            return paged;
        }

        /// <summary>
        /// Executes the GetByIdAsync operation.
        /// </summary>
        public Task<Client?> GetByIdAsync(long id)
        {
            return _repo.GetByIdAsync(id);
        }

        /// <summary>
        /// Executes the GetAllForSelectionAsync operation.
        /// </summary>
        public Task<IReadOnlyList<Client>> GetAllForSelectionAsync()
        {
            return _repo.GetAllForSelectionAsync();
        }

        /// <summary>
        /// Executes the CreateAsync operation.
        /// </summary>
        public async Task<Client> CreateAsync(string name, string email, string phoneNumber, string driverLicense)
        {
            var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedDriverLicense = (driverLicense ?? string.Empty).Trim().ToUpperInvariant();
            if (await _repo.ExistsByEmailAsync(normalizedEmail))
            {
                throw new BusinessValidationException(BusinessErrorCodes.ClientEmailAlreadyExists, "Email already exists.");
            }
            if (await _repo.ExistsByDriverLicenseAsync(normalizedDriverLicense))
            {
                throw new BusinessValidationException(BusinessErrorCodes.ClientDriverLicenseAlreadyExists, "Driver license already exists.");
            }

            try
            {
                var entity = new Client(name, normalizedEmail, phoneNumber, normalizedDriverLicense);
                await _repo.AddAsync(entity);
                await InvalidateClientFilterCachesAsync();
                return entity;
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(MapClientValidationCode(ex), ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException(BusinessErrorCodes.ClientEmailOrDriverLicenseAlreadyExists, "Email or driver license already exists.");
            }
        }

        /// <summary>
        /// Executes the UpdateAsync operation.
        /// </summary>
        public async Task UpdateAsync(long id, string name, string email, string phoneNumber, string driverLicense)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null)
            {
                throw new EntityNotFoundException($"Client with id {id} was not found.");
            }

            var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedDriverLicense = (driverLicense ?? string.Empty).Trim().ToUpperInvariant();
            if (await _repo.ExistsByEmailAsync(normalizedEmail, id))
            {
                throw new BusinessValidationException(BusinessErrorCodes.ClientEmailAlreadyExists, "Email already exists.");
            }
            if (await _repo.ExistsByDriverLicenseAsync(normalizedDriverLicense, id))
            {
                throw new BusinessValidationException(BusinessErrorCodes.ClientDriverLicenseAlreadyExists, "Driver license already exists.");
            }

            try
            {
                entity.UpdateClient(name, normalizedEmail, phoneNumber, normalizedDriverLicense);
                await _repo.UpdateAsync(entity);
                await InvalidateClientFilterCachesAsync();
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(MapClientValidationCode(ex), ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException(BusinessErrorCodes.ClientEmailOrDriverLicenseAlreadyExists, "Email or driver license already exists.");
            }
        }

        /// <summary>
        /// Executes the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(long id, bool ensureExists)
        {
            if (ensureExists)
            {
                var entity = await _repo.GetByIdAsync(id);
                if (entity is null)
                {
                    throw new EntityNotFoundException($"Client with id {id} was not found.");
                }
            }
            if (await _rentalContractRepo.HasActiveRentalForClientAsync(id, DateTime.UtcNow.Date))
            {
                throw new BusinessValidationException(BusinessErrorCodes.ClientDeleteBlockedActiveRental, "Cannot delete client with active rental.");
            }

            await _repo.DeleteAsync(id);
            await InvalidateClientFilterCachesAsync();
        }

        private static int NormalizePage(int page)
        {
            return page <= 0 ? 1 : page;
        }

        private async Task InvalidateClientFilterCachesAsync()
        {
            await _cache.RemoveAsync(CacheKeys.VehicleClientFilterOptions);
            await _cache.RemoveAsync(CacheKeys.RentalContractClientFilterOptions);
        }

        private static string MapClientValidationCode(ArgumentException ex)
        {
            return ex.ParamName switch
            {
                "name" => ex.Message.Contains("at most", StringComparison.OrdinalIgnoreCase)
                    ? BusinessErrorCodes.ClientNameTooLong
                    : BusinessErrorCodes.ClientNameRequired,
                "email" => ex.Message.Contains("at most", StringComparison.OrdinalIgnoreCase)
                    ? BusinessErrorCodes.ClientEmailTooLong
                    : ex.Message.Contains("format", StringComparison.OrdinalIgnoreCase)
                        ? BusinessErrorCodes.ClientEmailInvalidFormat
                        : BusinessErrorCodes.ClientEmailRequired,
                "phoneNumber" => ex.Message.Contains("format", StringComparison.OrdinalIgnoreCase)
                    ? BusinessErrorCodes.ClientPhoneInvalidFormat
                    : BusinessErrorCodes.ClientPhoneRequired,
                "driverLicense" => BusinessErrorCodes.ClientDriverLicenseRequired,
                _ => BusinessErrorCodes.GenericValidation
            };
        }
    }
}
