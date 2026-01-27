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
            string? json, json_custom;
            Category[]? categories, customs;
            List<Category> categories_list = new();
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
                categories = JsonSerializer.Deserialize<Category[]>(json, options);
            }
            catch (Exception ex)
            {
                Logger.Log("JsonLoader", ex.ToString());
                MessageBox.Show($"Файл категорий повреждён. Обратитесь к разработчику гида. К обращению приложите файл log.txt", "Заполнение гида", MessageBoxButton.OK);
                return null;
            }
            if (categories == null) return null;
            categories_list = categories.ToList();
            try
            {
                json_custom = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Assets/categories_custom.json");
                if (json_custom == null) return null;
            }
            catch (Exception ex)
            {
                Logger.Log("JsonLoader-Custom", ex.ToString());
                return null;
            }
            try
            {
                customs = JsonSerializer.Deserialize<Category[]>(json_custom, options);
            }
            catch (Exception ex)
            {
                Logger.Log("JsonLoader-Custom", ex.ToString());
                MessageBox.Show($"Файл категорий повреждён. Обратитесь к разработчику гида. К обращению приложите файл log.txt", "Заполнение гида", MessageBoxButton.OK);
                return null;
            }
            if (customs == null) return categories;
            foreach (Category cCategory in customs) // Перебираем аддоны
            {
                string? iName = cCategory.Internal_name;
                if (iName == null || cCategory.ElementsPaths == null) continue; // аддон говно
                bool found = false; // Проверяем что такая категория есть в базе(если нет, то это вообще новая)
                foreach (Category category in categories_list) // Ищем такую же категорию в базе
                {
                    if (category.Internal_name != cCategory.Internal_name) continue;
                    found = true; // Категория точно не новая, а расширялка к старой
                    category.ElementsPaths = cCategory.ElementsPaths;
                    category.Name = cCategory.Name;
                }
                if (!found) // Если категория в итоге новая, то добавляем её к списку категорий
                {
                    categories_list.Add(new Category()
                    {
                        Internal_name = cCategory.Internal_name,
                        Name = cCategory.Name,
                        ElementsPaths = cCategory.ElementsPaths
                    });
                }
            }
            return categories_list.ToArray();
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
