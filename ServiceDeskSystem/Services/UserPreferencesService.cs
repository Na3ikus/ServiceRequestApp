using Microsoft.JSInterop;

namespace ServiceDeskSystem.Services
{
    public class UserPreferencesService
    {
        private readonly IJSRuntime _js;
        private readonly Dictionary<string, string> _cache = new();

        public UserPreferencesService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string?> GetSettingAsync(string key, string defaultValue = null)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                return cachedValue;
            }

            try
            {
                var value = await _js.InvokeAsync<string?>("localStorage.getItem", key);
                if (value != null)
                {
                    _cache[key] = value;
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
            _cache[key] = value;
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", key, value);
            }
            catch
            {
                // Ignore JS errors, fallback to cache
            }
        }
    }
}
