using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SolyankaGuide.Internals
{
    internal class TextGen
    {
        public static TextBlock GetText(string[] text, bool centered, double width)
        {

            var tb = new TextBlock
            {
                Width = width,
                Foreground = Brushes.White,
                FontSize = 24,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = centered ? TextAlignment.Center : TextAlignment.Justify,
                Focusable = false
            };
            string[] lines = text;
            for (int i = 0; i < lines.Length; i++)
            {
                if (i != 0) tb.Inlines.Add("\n");
                var line = lines[i];
                bool hasHyper = Hyperlink(line, out string output, out string link, out string word);
                if (hasHyper)
                {
                    int markerIndex = output.IndexOf(word);
                    string before = output[..markerIndex];
                    string after = output[(markerIndex + word.Length)..];
                    var textBlock = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap
                    };
                    if (!string.IsNullOrEmpty(before))
                        textBlock.Inlines.Add(new Run(before + " "));
                    var hyperlink = new Hyperlink(new Run(word))
                    {
                        Foreground = Brushes.Aqua,
                        TextDecorations = TextDecorations.Underline,
                    };
                    hyperlink.Click += (s, e) =>
                    {
                        UrlOpener.OpenUrl(link);
                    };
                    textBlock.Inlines.Add(hyperlink);
                    if (!string.IsNullOrEmpty(after))
                        textBlock.Inlines.Add(new Run(after));
                    tb.Inlines.Add(textBlock);
                }
                else
                {
                    tb.Inlines.Add(line);
                }
            }
            return tb;

        }

        private static bool Hyperlink(string input, out string output, out string link, out string wordToReplace)
        {
            string pattern = @"%l=(.*?)%(.*?)%el%";
            Regex regex = new(pattern);
            Match match = regex.Match(input);
            if (match.Success)
            {
                string firstGroup = match.Groups[1].Value;
                string secondGroup = match.Groups[2].Value;
                output = input.Replace("%el%", "").Replace($"%l={firstGroup}%", "");
                wordToReplace = secondGroup;
                link = firstGroup;
                return true;
            }
            link = "";
            wordToReplace = "";
            output = input;
            return false;
        }
    }
}
