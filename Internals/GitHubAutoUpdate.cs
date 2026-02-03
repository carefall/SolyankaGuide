using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;

namespace SolyankaGuide.Internals
{
    internal class GitHubAutoUpdate
    {

        public static async Task<int> Update(TextBlock status)
        {
            var localJsons = GetLocalFiles(@"Assets/Data");
            var localImages = GetLocalFiles(@"Assets/Images");
            if (localJsons == null || localImages == null)
            {
                MessageBox.Show(Locale.Get("disk_check_fail"), Locale.Get("update"), MessageBoxButton.OK);
                return -1;
            }
            status.Text = Locale.Get("update_check");
            List<string> localFiles = new();
            localFiles.AddRange(localImages);
            localFiles.AddRange(localJsons);
            Dictionary<string, string?>? hashes = await GetGitHubHashes("carefall", "SolyankaGuide", "Assets/hashes.json");
            if (hashes == null || hashes.Count == 0)
            {
                MessageBox.Show(Locale.Get("update_time_exceeded"), Locale.Get("update"), MessageBoxButton.OK);
                return -1;
            }
            status.Text = Locale.Get("update_file_check");
            Dictionary<string, string> updateFiles = new();
            var githubJsons = await GetGitHubFolderContents("carefall", "SolyankaGuide", "Assets/Data");
            var githubImages = await GetGitHubFolderContents("carefall", "SolyankaGuide", "Assets/Images");
            if (githubJsons == null || githubJsons.Count == 0 || githubImages == null || githubImages.Count == 0) 
            {
                MessageBox.Show(Locale.Get("update_time_exceeded"), Locale.Get("update"), MessageBoxButton.OK);
                return -1;
            }
            status.Text = Locale.Get("update_comparison");
            List<GitHubContentItem> githubFiles = new();
            githubFiles.AddRange(githubImages);
            githubFiles.AddRange(githubJsons);
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
                status.Text = Locale.Get("update_found_status");
                Logger.Log("Updater", $"Found new version with changed files: {string.Join(" ", updateFiles.Keys)}.");
                var result = MessageBox.Show(Locale.Get("update_found"), Locale.Get("update"), MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Logger.Log("Updater", "Installing update.");
                    var keys = updateFiles.Keys.ToArray();
                    for(int i = 0; i < keys.Length; i++)
                    {
                        var key = keys[i];
                        status.Text = Locale.Get("installation", $"{i + 1} / {keys.Length}");
                        await DownloadFile(key, updateFiles[key]);
                    }
                    return 1;
                }
                return -1;
            }
            return 0;
        }

        private static async Task<Dictionary<string, string?>?> GetGitHubHashes(string owner, string repo, string path)
        {
            string url = $"https://raw.githubusercontent.com/{owner}/{repo}/main/{path}";
            using HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SolyankaGuide");
            try
            {
                string json = await client.GetStringAsync(url);
                JObject obj = JObject.Parse(json);
                return obj.Properties().ToDictionary(prop => prop.Name, prop => (string?)prop.Value);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                return null;
            }
        }

        public static async Task<List<GitHubContentItem>?> GetGitHubFolderContents(string owner, string repo, string path)
        {
            try
            {
                var url = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}";
                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SolyankaGuide");
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<List<GitHubContentItem>>(response);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
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
                return null;
            }
        }

        public static async Task DownloadFile(string url, string destinationPath)
        {
            try
            {
                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                var data = await client.GetByteArrayAsync(url);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await File.WriteAllBytesAsync(destinationPath, data);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                MessageBox.Show(Locale.Get("install_fail"), Locale.Get("update"), MessageBoxButton.OK);
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
