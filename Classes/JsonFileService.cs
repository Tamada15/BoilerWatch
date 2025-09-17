using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
namespace WpfApp1
{
    public static class JsonFileService
    {
        private static readonly string fullPath = Path.Combine(AppContext.BaseDirectory, "Data");

        public static async Task SaveAsync<T>(T data, string fileName)
        {
            // Гарантируем существование папки
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            var filePath = Path.Combine(fullPath, fileName);
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data));
        }

        public static async Task<T> LoadAsync<T>(string fileName) where T : new()
        {
            var filePath = Path.Combine(fullPath, fileName);

            if (!File.Exists(filePath))
                return new T();
            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            }
            catch
            {
                return new T();
            }
        }

    }
}
