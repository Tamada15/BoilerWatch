using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace WpfApp1
{
    public class HTTP_Client
    {
        string Token;
        string ID;
        private bool _isConnected = false;

        public bool IsReady => _isConnected && !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(ID);
        public bool IsConnected => _isConnected;

        public async Task<bool> Connect(CancellationToken cancellationToken = default)
        {
            try
            {
                var client = new RestClient("https://api.owencloud.ru");
                var request = new RestRequest("v1/auth/open", Method.Post);
                request.AddJsonBody(new
                {
                    login = "YOUR_LOGIN",
                    password = "YOUR_PASSWORD"
                });

                var response = await client.ExecuteAsync(request, cancellationToken);

                if (!response.IsSuccessful)
                {
                    _isConnected = false;
                    return false;
                }

                string json = response.Content;
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                Token = dict["token"].ToString();
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"Connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Get_ID(CancellationToken cancellationToken = default)
        {
            if (!_isConnected)
            {
                MessageBox.Show("Не удалось подключиться к API OwenCloud. Проверьте:\n1. Интернет-соединение\n2. Логин и пароль\n3. Доступность сервева");
                return false;
            }

            try
            {
                var client = new RestClient("https://api.owencloud.ru");
                var request = new RestRequest("/v1/device/index", Method.Post);
                request.AddHeader("Authorization", "Bearer " + Token);
                var response = await client.ExecuteAsync(request, cancellationToken);

                if (!response.IsSuccessful) return false;

                string json = response.Content;
                ID = JsonDocument.Parse(json)
                    .RootElement
                    .EnumerateArray()
                    .FirstOrDefault()
                    .GetProperty("id")
                    .GetInt32()
                    .ToString();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get_ID failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Parameters>> Get_Param()
        {
            if (!_isConnected) return new List<Parameters>();

            try
            {
                var client = new RestClient("https://api.owencloud.ru");
                var request = new RestRequest("/v1/device/" + ID, Method.Post);
                request.AddHeader("Authorization", "Bearer " + Token);
                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful || !LooksLikeJson(response.Content))
                    return new List<Parameters>();

                string json = response.Content;
                var parameters = JsonDocument.Parse(json)
                       .RootElement
                       .GetProperty("parameters")
                       .EnumerateArray()
                        .Select(p =>
                        {
                            string id = p.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : string.Empty;
                            string name = p.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : string.Empty;
                            string value = p.TryGetProperty("value", out var valueProp) ? valueProp.GetString() : string.Empty;

                            return new Parameters(id, name, value);
                        }).ToList();

                return parameters;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get_Param failed: {ex.Message}");
                return new List<Parameters>();
            }
        }

        public async Task<List<ParameterDataResponse>> Get_Last_Param()
        {
            if (!_isConnected) return new List<ParameterDataResponse>();

            try
            {
                var client = new RestClient("https://api.owencloud.ru");
                var request = new RestRequest("/v1/parameters/data", Method.Post);
                request.AddHeader("Authorization", "Bearer " + Token);
                var timezoneOffset = "+8:00";
                request.AddJsonBody(new
                {
                    ids = new[] { 43184389, 43184512, 43183789, 43184151, 43184371, 43183778, 43183693, 43183917 },
                    start = $"{DateTime.Now.AddDays(-30):yyyy-MM-dd HH:mm:ss}GMT{timezoneOffset}",
                    end = $"{DateTime.Now.AddDays(+1):yyyy-MM-dd HH:mm:ss}GMT{timezoneOffset}",
                    step = 3600
                });

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful) return new List<ParameterDataResponse>();

                string json = response.Content;
                return JsonConvert.DeserializeObject<List<ParameterDataResponse>>(json)
                       ?? new List<ParameterDataResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get_Last_Param failed: {ex.Message}");
                return new List<ParameterDataResponse>();
            }
        }

        public async Task<List<EventData>> Get_Events()
        {
            if (!_isConnected) return new List<EventData>();

            try
            {
                var client = new RestClient("https://api.owencloud.ru");
                var request = new RestRequest("/v1/device/events-log/" + ID, Method.Post);
                request.AddHeader("Authorization", "Bearer " + Token);
                var timezoneOffset = "+8:00";
                request.AddJsonBody(new
                {
                    start = $"{DateTime.Now.AddDays(-30):yyyy-MM-dd HH:mm:ss}GMT{timezoneOffset}",
                    end = $"{DateTime.Now.AddDays(+1):yyyy-MM-dd HH:mm:ss}GMT{timezoneOffset}",
                });

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful) return new List<EventData>();

                string json = response.Content;
                return JsonConvert.DeserializeObject<List<EventData>>(json)
                       ?? new List<EventData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get_Events failed: {ex.Message}");
                return new List<EventData>();
            }
        }

        private bool LooksLikeJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            char c = content.TrimStart()[0];
            return c == '{' || c == '[';
        }
    }



    public class Parameters
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Value { get; set; }

        public Parameters(string Id, string Name, string Value)
        {
            this.Id = Id;
            this.Name = Name;
            this.Value = Value;
        }
    }

    public class ParameterDataResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("values")]
        public List<ParameterValue> Values { get; set; } = new List<ParameterValue>();
    }

    public class ParameterValue
    {
        [JsonProperty("d")]
        public long Timestamp { get; set; }

        [JsonProperty("v")]
        public string Value { get; set; } = string.Empty;

        [JsonProperty("e")]
        public string Error { get; set; } = string.Empty;

        [JsonProperty("f")]
        public string FormattedValue { get; set; } = string.Empty;

        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).DateTime;
    }

    public class EventData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("event_id")]
        public long EventId { get; set; }

        [JsonProperty("start_dt")]
        public long StartDt { get; set; }

        [JsonProperty("end_dt")]
        public long? EndDt { get; set; }

        [JsonProperty("read_dt")]
        public long? ReadDt { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<object> Data { get; set; }

        [JsonProperty("device_id")]
        public long DeviceId { get; set; }

        [JsonProperty("is_critical")]
        public int IsCritical { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        // Дополнительные вычисляемые свойства для удобства
        public DateTime StartDateTime =>
        DateTimeOffset.FromUnixTimeSeconds(StartDt).ToLocalTime().DateTime;

        public DateTime? EndDateTime => EndDt.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(EndDt.Value).ToLocalTime().DateTime
            : null;

        public DateTime? ReadDateTime => ReadDt.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(ReadDt.Value).ToLocalTime().DateTime
            : null;

        public string eventType => IsCritical == 1 ? "Авария" : "Событие";
        public string IsActive => !EndDt.HasValue ? "Активное" : "Завершено";
    }

}
