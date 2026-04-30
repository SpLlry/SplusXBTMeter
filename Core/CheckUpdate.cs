using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SplusXBTMeter.Core
{
    public class CheckUpdate
    {
        private static readonly HttpClient _httpClient = new();
        public class ApiResult<T>
        {
            public int Code { get; set; } = 0;
            public string Msg { get; set; } = "ok";
            public T? Data { get; set; }
        }
        public class UpdateData
        {
            public string Version { get; set; } = "";
            public string Body { get; set; } = "";
            public string DownloadUrl { get; set; } = "";
            public bool HasUpdate { get; set; }
        }
        public static async Task<ApiResult<UpdateData>> GetUpdateInfo()
        {
            var result = new ApiResult<UpdateData>();

            try
            {
                // 1. 本地版本
                var localVersion = Assembly.GetExecutingAssembly().GetName().Version
                                   ?? new Version("0.0.0");
                string localVer = $"{localVersion.Major}.{localVersion.Minor}.{localVersion.Build}";

                // 2. 请求 Gitee
                string apiUrl = AppInfo.GiteeLastReleases;
                string json = await _httpClient.GetStringAsync(apiUrl);
                Console.WriteLine($"Gitee{json}");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                var data = JsonSerializer.Deserialize<GiteeRelease>(json, options);
                if (data == null)
                {
                    result.Code = 1;
                    result.Msg = "解析更新信息失败";
                    return result;
                }

                // 3. 版本对比
                Version serverVersion = new(data.TagName);
                Version currentVersion = new Version(localVer);
                if (serverVersion <= new Version(localVer))
                {
                    result.Code = 100;
                    result.Msg = "已经是最新版本了";
                    return result;
                }
                var updateData = new UpdateData
                {
                    Version = data.TagName,
                    Body = data.Body,
                    HasUpdate = serverVersion > currentVersion
                };

                // 4. 下载地址
                if (data.Assets?.Length > 0)
                {
                    var asset = JsonDocument.Parse(
                        JsonSerializer.Serialize(data.Assets[0])
                    ).RootElement;

                    updateData.DownloadUrl =
                        asset.GetProperty("browser_download_url").GetString() ?? "";
                }

                if (string.IsNullOrEmpty(updateData.DownloadUrl))
                {
                    updateData.DownloadUrl =AppInfo.GiteeReleases;
                }

                result.Data = updateData;
                return result;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Msg = ex.Message;
                return result;
            }
        }

        public class GiteeRelease
        {
            [JsonPropertyName("tag_name")]
            public string TagName { get; set; } = "0.0.0";

            [JsonPropertyName("body")]
            public string Body { get; set; } = "暂无更新日志";

            [JsonPropertyName("assets")]
            public object[] Assets { get; set; } = [];

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }
    }
}