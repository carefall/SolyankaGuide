using System.IO;
using System.Text.Json;
using System.Windows;

namespace SolyankaGuide.Internals
{
    internal static class JsonLoader
    {
        public static Category[]? FillCategories()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            string? json;
            try
            {
                json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Assets/Data/categories.json");
                if (json == null) return null;
            } catch (Exception ex)
            {
                Logger.Log("JsonLoader", ex.ToString());
                MessageBox.Show($"Не удалось обработать файл категорий. Обратитесь к разработчику гида. К обращению приложите файл log.txt", "Заполнение гида", MessageBoxButton.OK);
                return null;
            }
            try
            {
                return JsonSerializer.Deserialize<Category[]>(json, options);
            }
            catch (Exception ex)
            {
                Logger.Log("JsonLoader", ex.ToString());
                MessageBox.Show($"Файл категорий повреждён. Обратитесь к разработчику гида. К обращению приложите файл log.txt", "Заполнение гида", MessageBoxButton.OK);
                return null;
            }
        }

        public static Element[]? FillElements(string[]? elementsPaths)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (elementsPaths == null) return null;
            List<Element> totalElements = new();
            foreach (var elementsPath in elementsPaths)
            {
                string? json;
                try
                {
                    json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + $"/Assets/Data/{elementsPath}");
                } catch (Exception ex)
                {
                    Logger.Log("JsonLoader", ex.ToString());
                    MessageBox.Show($"Не удалось обработать файл {elementsPath}. Обратитесь к разработчику гида. К обращению приложите файл log.txt", "Заполнение гида", MessageBoxButton.OK);
                    continue;
                }
                if (json == null) continue;
                try
                {
                    Element[]? elements = JsonSerializer.Deserialize<Element[]>(json, options);
                    if (elements == null) continue;
                    totalElements.AddRange(elements);
                }
                catch (Exception ex)
                {
                    Logger.Log("JsonLoader", ex.ToString());
                    MessageBox.Show($"Файл {elementsPath} повреждён. Обратитесь к разработчику гида. К обращению приложите файл log.txt", "Заполнение гида", MessageBoxButton.OK);
                    continue;
                }
            }
            if (!totalElements.Any()) return null;
            return totalElements.ToArray();
        }
    }
}
