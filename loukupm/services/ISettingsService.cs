using loukupm.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace loukupm.Services
{
    /// <summary>
    /// Service for managing application settings from the backend API.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Fetches all settings from the API.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>List of all available settings.</returns>
        /// <exception cref="SettingsServiceException">Thrown on API errors or network issues.</exception>
        Task<List<SettingItem>> GetSettingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a single setting via PATCH request.
        /// Implements optimistic update with rollback on failure.
        /// </summary>
        /// <param name="key">The setting key to update.</param>
        /// <param name="value">The new value for the setting.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>True if the update was successful; false otherwise.</returns>
        /// <exception cref="SettingsServiceException">Thrown on API errors (not including validation failures).</exception>
        Task<bool> UpdateSettingAsync(string key, bool value, CancellationToken cancellationToken = default);
    }
}
