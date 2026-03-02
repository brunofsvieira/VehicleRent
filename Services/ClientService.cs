using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Services
{
    public class ClientService : IClientService
    {
        private static readonly int[] WebPageSizes = [10, 20, 50];
        private readonly IClientRepository _repo;

        public ClientService(IClientRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<Client>> GetPagedForWebAsync(int page, int pageSize)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = WebPageSizes.Contains(pageSize) ? pageSize : 10;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize);
            }

            return paged;
        }

        public async Task<PagedResult<Client>> GetPagedForApiAsync(int page, int pageSize)
        {
            var normalizedPage = NormalizePage(page);
            var normalizedPageSize = pageSize <= 0 || pageSize > 100 ? 10 : pageSize;

            var paged = await _repo.GetAllAsync(normalizedPage, normalizedPageSize);
            if (paged.TotalPages > 0 && normalizedPage > paged.TotalPages)
            {
                paged = await _repo.GetAllAsync(paged.TotalPages, normalizedPageSize);
            }

            return paged;
        }

        public Task<Client?> GetByIdAsync(long id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task<Client> CreateAsync(string name, string email, string phoneNumber, string driverLicense)
        {
            var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            if (await _repo.ExistsByEmailAsync(normalizedEmail))
            {
                throw new BusinessValidationException("Email already exists.");
            }

            try
            {
                var entity = new Client(name, normalizedEmail, phoneNumber, driverLicense);
                await _repo.AddAsync(entity);
                return entity;
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException("Email already exists.");
            }
        }

        public async Task UpdateAsync(long id, string name, string email, string phoneNumber, string driverLicense)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null)
            {
                throw new EntityNotFoundException($"Client with id {id} was not found.");
            }

            var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            if (await _repo.ExistsByEmailAsync(normalizedEmail, id))
            {
                throw new BusinessValidationException("Email already exists.");
            }

            try
            {
                entity.UpdateClient(name, normalizedEmail, phoneNumber, driverLicense);
                await _repo.UpdateAsync(entity);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessValidationException(ex.Message);
            }
            catch (DbUpdateException)
            {
                throw new BusinessValidationException("Email already exists.");
            }
        }

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

            await _repo.DeleteAsync(id);
        }

        private static int NormalizePage(int page)
        {
            return page <= 0 ? 1 : page;
        }
    }
}
