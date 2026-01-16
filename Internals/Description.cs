using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SolyankaGuide.Internals
{
    internal class Description
    {
        public Description(string buttonName, string header, string text, bool center, string imagePath, SubButton[] subButtons)
        {
            this.buttonName = buttonName;
            this.header = header;
            this.text = text;
            this.center = center;
            this.imagePath = imagePath;
            this.subButtons = subButtons;
        }

        public string buttonName { get; set; }

        public string header { get; set; }
        public string text { get; set; }

        public bool center { get; set; }
        public string imagePath { get; set; }

        public SubButton[] subButtons { get; set; }

        public static TextBlock GetText(string text, bool center, double width)
        {
            var tb = new TextBlock
            {
                Width = width,
                Foreground = Brushes.White,
                FontSize = 24,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = center? TextAlignment.Center : TextAlignment.Justify,
            };
            string[] lines = text.Split("\n");
            for (int i = 0; i < lines.Length; i++)
            {
                if (i != 0) tb.Inlines.Add("\n");
                var line = lines[i];
                bool hasHyper = Hyperlink(line, out string output, out string link, out string word);
                if (hasHyper)
                {
                    int markerIndex = output.IndexOf(word);
                    string before = output.Substring(0, markerIndex);
                    string after = output.Substring(markerIndex + word.Length);
                    var textBlock = new TextBlock();
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    if (!string.IsNullOrEmpty(before))
                        textBlock.Inlines.Add(new Run(before + " "));
                    var hyperlink = new Hyperlink(new Run(word))
                    {
                        Foreground = Brushes.Aqua,
                        TextDecorations = TextDecorations.Underline,
                    };
                    hyperlink.Click += (s, e) =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = link,
                            UseShellExecute = true
                        });
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
            Regex regex = new Regex(pattern);
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
