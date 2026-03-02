using VehicleRent.Models.Entities;
using System.Threading.Tasks;

namespace VehicleRent.Repositories
{
    public interface IVehicleRepository
    {
        /// <summary>
        /// Asynchronously adds a new vehicle to the repository.
        /// </summary>
        /// <remarks>The method validates the vehicle before adding it to the data store. Ensure that all
        /// required vehicle properties are set to valid values prior to calling this method.</remarks>
        /// <param name="vehicle">The vehicle to add to the repository. Cannot be null and must have all required properties set.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        Task AddAsync(Vehicle vehicle);

        /// <summary>
        /// Asynchronously retrieves a vehicle by its unique identifier.
        /// </summary>
        /// <remarks>This method may return null if no vehicle with the specified id exists.</remarks>
        /// <param name="id">The unique identifier of the vehicle to retrieve. Must be a positive value.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the vehicle if found; otherwise,
        /// null.</returns>
        Task<Vehicle?> GetByIdAsync(long id);

        /// <summary>
        /// Retrieves a page of vehicles and the total count.
        /// </summary>
        /// <param name="page">1-based page index.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Paged result with items and total count.</returns>
        Task<PagedResult<Vehicle>> GetAllAsync(int page, int pageSize);
        Task<IReadOnlyList<Vehicle>> GetAllForSelectionAsync();
        Task<bool> ExistsByLicensePlateAsync(string licensePlate, long? excludingId = null);

        /// <summary>
        /// Asynchronously updates the specified vehicle in the data store.
        /// </summary>
        /// <remarks>The update operation may include validation and persistence of the vehicle data.
        /// Callers should ensure that the vehicle object contains valid and complete information before invoking this
        /// method.</remarks>
        /// <param name="vehicle">The vehicle entity containing the updated information. This parameter must not be null.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateAsync(Vehicle vehicle);

        /// <summary>
        /// Deletes the resource identified by the specified unique identifier asynchronously.
        /// </summary>
        /// <remarks>Ensure that the resource exists before calling this method to avoid exceptions. This
        /// method does not return a value upon completion.</remarks>
        /// <param name="id">The unique identifier of the resource to delete. Must be a positive value.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteAsync(long id);
    }
}
