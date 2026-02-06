using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace SolyankaGuide.Internals
{
    internal class GitHubAutoUpdate
    {

        public static async Task<int> Update(TextBlock status)
        {
            status.Text = Locale.Get("update_check");
            var (jsonsDict, imagesDict) = await GetGitHubHashes("carefall", "SolyankaGuide", "Assets/hashes.json");
            if (jsonsDict == null || imagesDict == null)
            {
                MessageBox.Show(Locale.Get("update_time_exceeded"), Locale.Get("update"), MessageBoxButton.OK);
                return -1;
            }
            status.Text = Locale.Get("update_texts");
            var localJsons = GetLocalFiles(@"Assets/Data", true);
            if (localJsons == null)
            {
                MessageBox.Show(Locale.Get("disk_check_fail"), Locale.Get("update"), MessageBoxButton.OK);
                return -1;
            }
            var localImages = GetLocalFiles(@"Assets/Images", false);
            if (localImages == null)
            {
                MessageBox.Show(Locale.Get("disk_check_fail"), Locale.Get("update"), MessageBoxButton.OK);
                return -1;
            }
            List<string> filesToDelete = new();
            Dictionary<string, string> filesToUpdate = new();
            foreach (string localFile in localJsons)
            {
                string path = "Assets/Data/" + localFile;
                if (!jsonsDict.ContainsKey(localFile)) { 
                    filesToDelete.Add(path);
                    continue;
                }
                if (jsonsDict[localFile] != ComputeFileSha1(path)) filesToUpdate[localFile] = path;
            }
            foreach (string localFile in localImages)
            {
                string path = "Assets/Images/" + localFile;
                if (!jsonsDict.ContainsKey(localFile))
                {
                    filesToDelete.Add(path);
                    continue;
                }
                if (jsonsDict[localFile] != ComputeFileSha1(path)) filesToUpdate[localFile] = path;
            }
            foreach(string gitFile in jsonsDict.Keys)
            {
                string path = Path.Combine(@"Assets/Images", gitFile);
                if (!localJsons.Contains(gitFile)) filesToUpdate[gitFile] = path;
            }
            foreach (string gitFile in imagesDict.Keys)
            {
                string path = Path.Combine(@"Assets/Images", gitFile);
                if (!localImages.Contains(gitFile)) filesToUpdate[gitFile] = path;
            }
            if (filesToUpdate.Count > 0)
            {
                status.Text = Locale.Get("update_found_status");
                Logger.Log("Updater", $"Found new version."); // add detailed description
                var result = MessageBox.Show(Locale.Get("update_found"), Locale.Get("update"), MessageBoxButton.YesNo); // add detailed description
                if (result == MessageBoxResult.Yes)
                {
                    Logger.Log("Updater", "Installing update.");
                    var keys = filesToUpdate.Keys.ToArray();
                    int len = keys.Length;
                    for (int i = 0; i < len; i++)
                    {
                        var key = keys[i];
                        status.Text = Locale.Get("installation", $"{i + 1} / {len}");
                        await DownloadFile(key, filesToUpdate[key]);
                    }
                    foreach (var path in filesToDelete)
                    {
                        File.Delete(path);
                    }
                    return 1;
                }
                return -1;
            }
            return 0;
        }

        private static async Task<(Dictionary<string, string>? jsonsDict, Dictionary<string, string>? imagesDict)> GetGitHubHashes(string owner, string repo, string path)
        {
            string url = $"https://raw.githubusercontent.com/{owner}/{repo}/main/{path}";
            using HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SolyankaGuide");
            try
            {
                string json = await client.GetStringAsync(url);
                JObject obj = JObject.Parse(json);
                var jsonsDict = obj["jsons"]?.ToObject<Dictionary<string, string>>();
                var imagesDict = obj["images"]?.ToObject<Dictionary<string, string>>();
                return (jsonsDict, imagesDict);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                return (null, null);
            }
        }

        public static string ComputeFileSha1(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static List<string>? GetLocalFiles(string folderPath, bool json)
        {
            try
            {
                return Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Select(f => f.Replace("\\", "/").Substring(json? "Assets/Data/".Length : "Assets/Images/".Length)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                return null;
            }
        }

        public static async Task DownloadFile(string file, string destinationPath)
        {
            try
            {
                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                var data = await client.GetByteArrayAsync($"https://raw.githubusercontent.com/carefall/SolyankaGuide/main/Assets/{file}");
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                await File.WriteAllBytesAsync(destinationPath, data);
            }
            catch (Exception ex)
            {
                Logger.Log("Updater", ex.ToString());
                MessageBox.Show(Locale.Get("install_fail"), Locale.Get("update"), MessageBoxButton.OK);
            }
        }
    }
}
