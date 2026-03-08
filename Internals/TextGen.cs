using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SolyankaGuide.Internals
{
    internal static class TextGen
    {
        public static event Action<string, string, int, int>? SwitchDescription;
        public static event Action<BitmapImage>? MaximizeImage;

        private static readonly SolidColorBrush SpoilerBrush = new(Color.FromRgb(26, 26, 26));
        private static readonly Brush LinkBrush = Brushes.Aqua;
        private static readonly Brush GuideBrush = Brushes.Yellow;
        private static readonly Brush HoverBrush = Brushes.Gray;
        private static readonly ConcurrentDictionary<string, BitmapImage> ImageCache = new();


        public static TextBlock GetText(string[] lines, bool centered, double width)
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
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                    tb.Inlines.Add(new LineBreak());

                ParseLine(lines[i], tb);
            }
            return tb;
        }

        private static void ParseLine(string line, TextBlock tb)
        {
            int pos = 0;
            while (pos < line.Length)
            {
                int percent = line.IndexOf('%', pos);

                if (percent < 0)
                {
                    tb.Inlines.Add(new Run(line[pos..]));
                    break;
                }
                if (percent > pos) tb.Inlines.Add(new Run(line[pos..percent]));
                if (TryParseTag(line, percent, out int newPos, out Inline inline))
                {
                    tb.Inlines.Add(inline);
                    pos = newPos;
                }
                else
                {
                    tb.Inlines.Add(new Run("%"));
                    pos = percent + 1;
                }
            }
        }

        private static bool TryParseTag(string line, int start, out int newPos, out Inline inline)
        {
            inline = null!;
            newPos = start;
            if (line.AsSpan(start).StartsWith("%img="))
                return TryParseImage(line, start, out newPos, out inline);
            if (line.AsSpan(start).StartsWith("%gl="))
                return TryParseGuide(line, start, out newPos, out inline);
            if (line.AsSpan(start).StartsWith("%l="))
                return TryParseLink(line, start, out newPos, out inline);
            if (line.AsSpan(start).StartsWith("%s%"))
                return TryParseSpoiler(line, start, out newPos, out inline);
            return false;
        }

        private static bool TryParseImage(string line, int start, out int newPos, out Inline inline)
        {
            inline = null!;
            newPos = start;
            int pathEnd = line.IndexOf('%', start + 5);
            if (pathEnd < 0) return false;
            int close = line.IndexOf("%eimg%", pathEnd);
            if (close < 0) return false;
            string path = line[(start + 5)..pathEnd];
            string text = line[(pathEnd + 1)..close];
            inline = CreateImageLink(path, text);
            newPos = close + 6;
            return true;
        }

        private static bool TryParseGuide(string line, int start, out int newPos, out Inline inline)
        {
            inline = null!;
            newPos = start;
            int dataEnd = line.IndexOf('%', start + 4);
            if (dataEnd < 0) return false;
            int close = line.IndexOf("%egl%", dataEnd);
            if (close < 0) return false;
            string data = line[(start + 4)..dataEnd];
            string text = line[(dataEnd + 1)..close];
            inline = CreateGuideLink(data, text);
            newPos = close + 5;
            return true;
        }

        private static bool TryParseLink(string line, int start, out int newPos, out Inline inline)
        {
            inline = null!;
            newPos = start;
            int urlEnd = line.IndexOf('%', start + 3);
            if (urlEnd < 0) return false;
            int close = line.IndexOf("%el%", urlEnd);
            if (close < 0) return false;
            string url = line[(start + 3)..urlEnd];
            string text = line[(urlEnd + 1)..close];
            inline = CreateHyperLink(url, text);
            newPos = close + 4;
            return true;
        }

        private static bool TryParseSpoiler(string line, int start, out int newPos, out Inline inline)
        {
            inline = null!;
            newPos = start;
            int close = line.IndexOf("%es%", start + 3);
            if (close < 0) return false;
            string text = line[(start + 3)..close];
            inline = CreateSpoiler(text);
            newPos = close + 4;
            return true;
        }

        private static Inline CreateImageLink(string path, string text)
        {
            var img = ImageCache.GetOrAdd(path, LoadImage);
            var link = new Hyperlink(new Run(text))
            {
                Foreground = LinkBrush,
                Tag = img
            };
            link.Click += OnImageClick;
            return link;
        }

        private static void OnImageClick(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link && link.Tag is BitmapImage img) MaximizeImage?.Invoke(img);
        }

        private static BitmapImage LoadImage(string path)
        {
            var img = ImageLoader.LoadImage(path);
            img.Freeze();
            return img;
        }

        private static Inline CreateGuideLink(string data, string text)
        {
            if (!TryParseGuideData(data, out var parsed)) return new Run(text);
            var link = new Hyperlink(new Run(text))
            {
                Foreground = GuideBrush,
                FontWeight = FontWeights.Bold,
                Tag = parsed
            };
            link.Click += OnGuideClick;
            return link;
        }

        private static void OnGuideClick(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link && link.Tag is GuideData g) SwitchDescription?.Invoke(g.Category, g.ElementsList, g.ElementId, g.DescriptionId);
        }

        private static Inline CreateHyperLink(string url, string text)
        {
            var link = new Hyperlink(new Run(text))
            {
                Foreground = LinkBrush,
                TextDecorations = TextDecorations.Underline,
                Tag = url
            };
            link.Click += OnUrlClick;
            return link;
        }

        private static void OnUrlClick(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link && link.Tag is string url) UrlOpener.OpenUrl(url);
        }

        private static Inline CreateSpoiler(string text)
        {
            var link = new Hyperlink(new Run(text))
            {
                Foreground = SpoilerBrush,
                Background = SpoilerBrush,
                Cursor = Cursors.Arrow,
                TextDecorations = null, // убрали подчёркивание
                Tag = false // false = не раскрыт
            };
            link.Click += OnSpoilerClick;
            link.MouseEnter += OnSpoilerEnter;
            link.MouseLeave += OnSpoilerLeave;
            return link;
        }

        private static void OnSpoilerClick(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                link.Foreground = Brushes.White;
                link.Background = Brushes.Transparent;
                link.Tag = true; // помечаем как раскрытый
            }
        }

        private static void OnSpoilerEnter(object sender, MouseEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                if (link.Tag is bool opened && opened) return;
                link.Foreground = HoverBrush;
                link.Background = HoverBrush;
            }
        }

        private static void OnSpoilerLeave(object sender, MouseEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                if (link.Tag is bool opened && opened) return;
                link.Foreground = SpoilerBrush;
                link.Background = SpoilerBrush;
            }
        }

        private static bool TryParseGuideData(string data, out GuideData result)
        {
            result = default;
            var parts = data.Split('/');
            if (parts.Length < 3 || parts.Length > 4) return false;
            if (!int.TryParse(parts[2], out int entryId) || entryId < 0) return false;
            int descId = -1;
            if (parts.Length == 4 && (!int.TryParse(parts[3], out descId) || descId < 0)) return false;
            result = new GuideData(parts[0], parts[1], entryId, descId);
            return true;
        }

        private readonly struct GuideData
        {
            public readonly string Category;
            public readonly string ElementsList;
            public readonly int ElementId;
            public readonly int DescriptionId;

            public GuideData(string cat, string list, int elemId, int descId)
            {
                Category = cat;
                ElementsList = list;
                ElementId = elemId;
                DescriptionId = descId;
            }
        }
    }
}