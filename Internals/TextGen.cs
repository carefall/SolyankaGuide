using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolyankaGuide.Internals
{

    internal class TextGen
    {

        public static event Action<string, string, int, int>? SwitchDescription;
        public static event Action<BitmapImage>? MaximizeImage;

        private static readonly SolidColorBrush spoilerColor = (SolidColorBrush) new BrushConverter().ConvertFromString("#1a1a1a")!;
        public static TextBlock GetText(string[] text, bool centered, double width)
        {
            var tb = new TextBlock
            {
                Width = width,
                Foreground = Brushes.White,
                FontSize = 24,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = centered ? TextAlignment.Center : TextAlignment.Left,
                Focusable = false,
                Margin = new Thickness(0, 5, 0, 0)
            };

            tb.SizeChanged += (s, e) =>
            {
                foreach (var inline in tb.Inlines)
                {
                    if (inline == null) continue;
                    if (inline is not InlineUIContainer ic) continue;
                    if (ic.Child is not Grid g) continue;
                    if (g.Children.Count == 0 || g.Children[0] is not Image img) continue;
                    g.Width = tb.Width;
                    img.Width = tb.ActualWidth / 3;
                }
            };

            // Combined regex to find all supported tags in a single pass. Named groups identify which tag matched.
            string pattern = @"%l=(?<link>.*?)%(?<linkText>.*?)%el%|%gl=(?<glParts>.*?)%(?<glText>.*?)%egl%|%img=(?<imgSrc>.*?)%(?<imgText>.*?)%eimg%|%s%(?<spoilerText>.*?)%es%";
            var regex = new Regex(pattern);

            foreach (var rawLine in text)
            {
                if (tb.Inlines.Count > 0)
                    tb.Inlines.Add("\n");

                string line = rawLine ?? string.Empty;
                int lastIndex = 0;
                var matches = regex.Matches(line);
                foreach (Match match in matches)
                {
                    if (match.Index > lastIndex)
                    {
                        string before = line.Substring(lastIndex, match.Index - lastIndex);
                        if (!string.IsNullOrEmpty(before)) tb.Inlines.Add(new Run(before));
                    }

                    if (match.Groups["link"].Success)
                    {
                        string url = match.Groups["link"].Value;
                        string words = match.Groups["linkText"].Value;
                        var hyperlink = new Hyperlink(new Run(words))
                        {
                            Foreground = Brushes.Aqua,
                            TextDecorations = TextDecorations.Underline
                        };
                        hyperlink.Click += (s, e) => UrlOpener.OpenUrl(url);
                        tb.Inlines.Add(hyperlink);
                    }
                    else if (match.Groups["glParts"].Success)
                    {
                        string firstGroup = match.Groups["glParts"].Value;
                        string words = match.Groups["glText"].Value;
                        var parts = firstGroup.Split('/');
                        bool valid = parts.Length is >= 3 and <= 4;
                        if (valid)
                        {
                            if (!int.TryParse(parts[2], out int eId) || eId < 0) valid = false;
                            if (valid && parts.Length == 4)
                            {
                                if (!int.TryParse(parts[3], out int dId) || dId < 0) valid = false;
                            }
                        }
                        if (!valid)
                        {
                            // If invalid, output the raw matched text as-is
                            tb.Inlines.Add(new Run(match.Value));
                        }
                        else
                        {
                            var hyperlink = new Hyperlink(new Run(words))
                            {
                                Foreground = Brushes.Yellow,
                                FontWeight = FontWeights.Bold
                            };
                            hyperlink.Click += (s, e) =>
                            {
                                SwitchDescription?.Invoke(parts[0], parts[1], int.Parse(parts[2]), parts.Length == 4 ? int.Parse(parts[3]) : -1);
                            };
                            tb.Inlines.Add(hyperlink);
                        }
                    }
                    else if (match.Groups["imgSrc"].Success)
                    {
                        // Use legacy ImageInline behavior: show alt text as hyperlink that opens the image.
                        string src = match.Groups["imgSrc"].Value;
                        string alt = match.Groups["imgText"].Value;
                        try
                        {
                            var bmi = ImageLoader.LoadImage(src);
                            var hyperlink = new Hyperlink(new Run(alt))
                            {
                                Foreground = Brushes.Aqua,
                                TextDecorations = TextDecorations.Underline
                            };
                            hyperlink.Click += (s, e) => MaximizeImage?.Invoke(bmi);
                            tb.Inlines.Add(hyperlink);
                        }
                        catch
                        {
                            // on failure show the alt/text without hyperlink
                            tb.Inlines.Add(new Run(alt));
                        }
                    }
                    else if (match.Groups["spoilerText"].Success)
                    {
                        string words = match.Groups["spoilerText"].Value;
                        var hyperlink = new Hyperlink(new Run(words))
                        {
                            Foreground = spoilerColor,
                            Background = spoilerColor,
                            TextDecorations = null,
                            Cursor = Cursors.Arrow,
                            Tag = false
                        };

                        hyperlink.Click += (s, e) =>
                        {
                            // On click: reveal permanently — make text white and remove background.
                            if (hyperlink.Inlines.FirstInline is Run r)
                            {
                                r.Foreground = Brushes.White;
                            }
                            hyperlink.Background = Brushes.Transparent;
                            hyperlink.Tag = true; // mark as revealed
                        };

                        hyperlink.MouseEnter += (s, e) =>
                        {
                            // If already revealed, do nothing on hover.
                            if (hyperlink.Tag is bool clicked && clicked) return;
                            // On hover before click keep text color the same as spoiler (i.e. equal to background),
                            // so set both to a lighter shade but equal values.
                            var hoverBrush = Brushes.Gray;
                            hyperlink.Background = hoverBrush;
                            if (hyperlink.Inlines.FirstInline is Run r)
                            {
                                r.Foreground = hoverBrush;
                            }
                        };

                        hyperlink.MouseLeave += (s, e) =>
                        {
                            // If already revealed, do nothing on leave.
                            if (hyperlink.Tag is bool clicked && clicked) return;
                            // Revert both background and text to hidden color.
                            hyperlink.Background = spoilerColor;
                            if (hyperlink.Inlines.FirstInline is Run r)
                            {
                                r.Foreground = spoilerColor;
                            }
                        };
                        tb.Inlines.Add(hyperlink);
                    }

                    lastIndex = match.Index + match.Length;
                }

                if (lastIndex < line.Length)
                {
                    string tail = line.Substring(lastIndex);
                    if (!string.IsNullOrEmpty(tail)) tb.Inlines.Add(new Run(tail));
                }

                // If there were no matches at all, ensure the whole line is added
                if (matches.Count == 0 && !string.IsNullOrEmpty(line))
                    tb.Inlines.Add(new Run(line));
            }

            return tb;
        }
    }
}
