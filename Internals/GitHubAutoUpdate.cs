using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Resources;

namespace SolyankaGuide.Internals
{
    internal class GitHubAutoUpdate
    {

        public static async Task<bool> Update()
        {
            var localJsons = GetLocalFiles(@"Assets/Data");
            var localImages = GetLocalFiles(@"Assets/Images");
            if (localJsons == null || localImages == null) return false;
            List<string> localFiles = new();
            localFiles.AddRange(localImages);
            localFiles.AddRange(localJsons);
            var githubJsons = await GetGitHubFolderContents("carefall", "SolyankaGuide", "Assets/Data");
            var githubImages = await GetGitHubFolderContents("carefall", "SolyankaGuide", "Assets/Images");
            if (githubJsons == null || githubJsons.Count == 0 || githubImages == null || githubImages.Count == 0) return false;
            List<GitHubContentItem> githubFiles = new();
            githubFiles.AddRange(githubImages);
            githubFiles.AddRange(githubJsons);
            Dictionary<string, string?>? hashes = await GetGitHubHashes("carefall", "SolyankaGuide", "Assets/hashes.json", GetToken());
            if (hashes == null || hashes.Count == 0) return false;
            Dictionary<string, string> updateFiles = new();
            foreach (var item in githubFiles)
            {
                bool needDownload = false;
                var localFilePath = Path.Combine("Assets", item.Path!.Substring("Assets/".Length).Replace("\\", "/"));
                if (!File.Exists(localFilePath))
                {
                    needDownload = true;
                }
                else
                {
                    string localSha = ComputeFileSha1(localFilePath);
                    if (localSha != hashes[item.Name!])
                    {
                        needDownload = true;
                    }
                }
                if (needDownload)
                {
                    updateFiles.Add(item.Download_url!, localFilePath);
                }
            }
            if (updateFiles.Count > 0)
            {
                Logger.Log("Updater", "Found new version.");
                var result = MessageBox.Show("Найдена новая версия программы. Желаете установить?", "Автообновление", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Logger.Log("Updater", "Installing update.");
                    foreach (var item in updateFiles)
                    {
                        await DownloadFile(item.Key, item.Value);
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        private static string GetToken()
        {
            Uri uri = new("pack://application:,,,/Internals/token.txt");
            StreamResourceInfo info = Application.GetResourceStream(uri);
            using Stream stream = info.Stream;
            if (stream == null)
            {
                Logger.Log("Updater", "No token provided.");
                return "token";
            }
            using StreamReader reader = new(stream);
            string content = reader.ReadToEnd();
            return content;
        }

        private static async Task<Dictionary<string, string?>?> GetGitHubHashes(string owner, string repo, string path, string token)
        {
            string url = $"https://raw.githubusercontent.com/{owner}/{repo}/main/{path}";
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SolyankaGuide");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token);
            }
            string json = await client.GetStringAsync(url);
            JObject obj = JObject.Parse(json);
            return obj.Properties().ToDictionary(prop => prop.Name, prop => (string?)prop.Value);
        }

        public static async Task<List<GitHubContentItem>?> GetGitHubFolderContents(string owner, string repo, string path)
        {
            try
            {
                var url = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SolyankaGuide");
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<List<GitHubContentItem>>(response);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                MessageBox.Show("Не удалось получить доступ в репозиторий. Обратитесь к разработчику гида. К обращению прикрепите log.txt", "Автообновление", MessageBoxButton.OK);
                return null;
            }
        }

        public static string ComputeFileSha1(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static List<string>? GetLocalFiles(string folderPath)
        {
            try
            {
                return Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Select(f => f.Replace("\\", "/")).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                MessageBox.Show("Ошибка проверки файлов на диске. Обратитесь к разработчику гида. К обращению прикрепите log.txt", "Автообновление", MessageBoxButton.OK);
                return null;
            }
        }

        public static async Task DownloadFile(string url, string destinationPath)
        {
            try
            {
                using var client = new HttpClient();
                var data = await client.GetByteArrayAsync(url);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await File.WriteAllBytesAsync(destinationPath, data);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                MessageBox.Show("Не удалось загрузить и записать файл с GitHub. Обратитесь к разработчику гида. К обращению прикрепите log.txt", "Автообновление", MessageBoxButton.OK);
            }
        }

        public class GitHubContentItem
        {
            public string? Name { get; set; }
            public string? Path { get; set; }
            public string? Sha { get; set; }
            public string? Type { get; set; }
            public string? Download_url { get; set; }
        }
    }
}
