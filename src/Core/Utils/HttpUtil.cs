using Blish_HUD;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Nekres.ProofLogix.Core.Utils {
    internal static class HttpUtil {

        public static bool TryParseJson<T>(string json, out T result) {
            bool success = true;
            var settings = new JsonSerializerSettings {
                Error = (_, args) => {
                    success = false; 
                    args.ErrorContext.Handled = true;
                },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(json, settings);
            return success;
        }

        public static async Task<T> RetryAsync<T>(string url, int retries = 2, int delayMs = 10000, Logger logger = null) {

            logger ??= Logger.GetLogger(typeof(HttpUtil));

            var request = url.AllowHttpStatus(HttpStatusCode.OK).WithTimeout(10);

            try {
                var response = await request.GetAsync();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            } catch (Exception e) {

                if (retries > 0) {
                    logger.Warn(e, $"Failed to request data. Retrying in {delayMs / 1000} second(s) (remaining retries: {retries}).");
                    await Task.Delay(delayMs);
                    return await RetryAsync<T>(url, retries - 1, delayMs, logger);
                }
                
                //TODO: Consider adjusting exception behaviour and log levels.
                switch (e) {
                    case FlurlHttpTimeoutException:
                        logger.Warn(e, e.Message);
                        break;
                    case FlurlHttpException:
                        logger.Warn(e, e.Message);
                        break;
                    case JsonReaderException:
                        logger.Warn(e, e.Message);
                        break;
                    default: 
                        logger.Error(e, e.Message);
                        break;
                }
            }

            return default;
        }
    }
}
