using System;
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
            try
            {
                json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Assets/Data/categories.json");
                if (json == null) return null;
            } catch (Exception ex)
            {
                Logger.Log("JsonLoader", ex.ToString());
                MessageBox.Show(Locale.Get("categories_fail"), Locale.Get("guide_fill"), MessageBoxButton.OK);
                return null;
            }
            try
            {
                categories = JsonSerializer.Deserialize<Category[]>(json, options);
            }
            catch (Exception ex)
            {
                Logger.Log("JsonLoader", ex.ToString());
                MessageBox.Show(Locale.Get("categories_corrupted"), Locale.Get("guide_fill"), MessageBoxButton.OK);
                return null;
            }
            if (categories == null) return null;
            List<Category> categoriesList = categories.ToList();
            try
            {
                json_custom = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Assets/Custom/categories_custom.json");
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
                MessageBox.Show(Locale.Get("custom_categories_fail"), Locale.Get("guide_fill"), MessageBoxButton.OK);
                return null;
            }
            if (customs == null) return categories;
            foreach (Category cCategory in customs) // Перебираем аддоны
            {
                string? iName = cCategory.Internal_name;
                if (iName == null || cCategory.ElementsPaths == null) // аддон неправильно составлен
                {
                    Logger.Log("JsonLoader-Custom", "One of the categories is missing name and/or elements.");
                    continue;
                }
                bool found = false; // Проверяем что такая категория есть в базе(если нет, то это вообще новая)
                foreach (Category category in categoriesList) // Ищем такую же категорию в базе
                {
                    if (category.Internal_name != cCategory.Internal_name) continue;
                    found = true; // Категория точно не новая, а расширялка к старой
                    Logger.Log("JsonLoader-Custom", $"Category {category.Name} is overriden to {cCategory.Name} with elements: {string.Join(" ", cCategory.ElementsPaths)}.");
                    category.ElementsPaths = cCategory.ElementsPaths;
                    category.Name = cCategory.Name;
                    category.Custom = true;
                }
                if (!found) // Если категория в итоге новая, то добавляем её к списку категорий
                {
                    categoriesList.Add(new Category()
                    {
                        Internal_name = cCategory.Internal_name,
                        Name = cCategory.Name,
                        ElementsPaths = cCategory.ElementsPaths,
                        Custom = true
                    });
                    Logger.Log("JsonLoader-Custom", $"Adden category {cCategory.Name} with elements: {string.Join(" ", cCategory.ElementsPaths)}.");
                }
            }
            return categoriesList.ToArray();
        }

        public static Element[]? FillElements(Category category)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var elementsPaths = category.ElementsPaths;
            if (elementsPaths == null) return null;
            List<Element> totalElements = new();
            foreach (var elementsPath in elementsPaths)
            {
                string? json;
                try
                {
                    json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Assets/" + (category.Custom? "Custom/" : "") + $"Data/{elementsPath}.json");
                } catch (Exception ex)
                {
                    Logger.Log("JsonLoader", ex.ToString());
                    MessageBox.Show(Locale.Get("elements_fail", elementsPath), Locale.Get("guide_fill"), MessageBoxButton.OK);
                    continue;
                }
                if (json == null) continue;
                try
                {
                    Element[]? elements = JsonSerializer.Deserialize<Element[]>(json, options);
                    if (elements == null)
                    {
                        Logger.Log("JsonLoader", $"Category {elementsPath} is missing elements");
                        continue;
                    }
                    totalElements.AddRange(elements);
                }
                catch (Exception ex)
                {
                    Logger.Log("JsonLoader", ex.ToString());
                    MessageBox.Show(Locale.Get("elements_corrupted", elementsPath), Locale.Get("guide_fill"), MessageBoxButton.OK);
                    continue;
                }
            }
            if (!totalElements.Any()) return null;
            return totalElements.ToArray();
        }
    }
}
