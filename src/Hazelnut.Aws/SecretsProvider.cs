using Amazon;
using System.Text.Json;
using Amazon.SecretsManager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Amazon.SecretsManager.Extensions.Caching;

namespace Hazelnut.Aws
{
    /// <summary>
    /// AWS SecretsManager secret provider
    /// </summary>
    /// <typeparam name="T">Type of the options.</typeparam>
    /// <example>
    /// Services.AddSignleton(sp=> sp.GetRequiredService<SecretsProvider<typeparamref name="T"/>>()
    ///          .RegisterSecret("MySecretName",(options) => {}).Result)
    /// </example>
    public class SecretsProvider<T> : IDisposable where T : class
    {
        private readonly IOptions<T> _options;
        private readonly ILogger<SecretsProvider<T>> _logger;
        private readonly SecretsManagerCache _cacheManager;
        private readonly IAmazonSecretsManager _secretsManager;

        public SecretsProvider(IOptions<T> options, RegionEndpoint region, ILogger<SecretsProvider<T>> logger)
        {
            _options = options;
            _logger = logger;
            _secretsManager = new AmazonSecretsManagerClient(new AmazonSecretsManagerConfig { RegionEndpoint = region });
            _cacheManager = new(_secretsManager);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretName">Name of Secret as defined in Secrets Manager</param>
        /// <param name="hyderateOptions">Action to invoke on Option<typeparamref name="T"/> when options have alredy are populated from app settings but only secret values need to be populate</param>
        /// <returns> returns <typeparamref name="T"/></returns>
        public async Task<T> RegisterSecret(string secretName, Action<T>? hyderateOptions = null)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                _logger.LogInformation($"{secretName} not provided, returning default value");
                return _options.Value;
            }

            var secretText = await _cacheManager.GetSecretString(secretName);
            if (string.IsNullOrEmpty(secretText))
            {
                _logger.LogInformation($"Not secret found for {secretName}, returning default value");
                return _options.Value;
            }

            var secret = JsonSerializer.Deserialize<T>(secretText);

            // if options have alredy are populated from app settings but only secret values need to be populate
            if (hyderateOptions != null)
            {
                hyderateOptions(secret);
                return _options.Value;
            }

            // else return the secrets object return from SecretManager
            return secret;
        }

        public void Dispose()
        {
            _secretsManager.Dispose();
            _cacheManager.Dispose();
        }
    }
}
