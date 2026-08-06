using Microsoft.JSInterop;

namespace ServiceDeskSystem.Services
{
    public class UserPreferencesService
    {
        private readonly IJSRuntime _js;
        private readonly Dictionary<string, string> _cache = new ();

        public UserPreferencesService(IJSRuntime js)
        {
            this._js = js;
        }

        public async Task<string?> GetSettingAsync(string key, string? defaultValue = null)
        {
            if (this._cache.TryGetValue(key, out var cachedValue))
            {
                return cachedValue;
            }

            try
            {
                var value = await this._js.InvokeAsync<string?>("localStorage.getItem", key);
                if (value != null)
                {
                    this._cache[key] = value;
                    return value;
                }
            }
            catch
            {
                // Ignore prerendering/JS errors
            }

            return defaultValue;
        }

        public async Task SetSettingAsync(string key, string value)
        {
            this._cache[key] = value;
            try
            {
                await this._js.InvokeVoidAsync("localStorage.setItem", key, value);
            }
            catch
            {
                // Ignore JS errors, fallback to cache
            }
        }
    }
}
