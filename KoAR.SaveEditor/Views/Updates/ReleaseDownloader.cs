using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using KoAR.SaveEditor.Updates;

namespace KoAR.SaveEditor.Views.Updates
{
    using MarkdownEngine = Markdown.Xaml.Markdown;

    public sealed class ReleaseDownloader : Control
    {
        public static readonly DependencyProperty DocumentProperty;

        public static readonly DependencyProperty ReleasesProperty = DependencyProperty.Register(nameof(ReleaseDownloader.Release), typeof(IReadOnlyCollection<IReleaseInfo>), typeof(ReleaseDownloader),
            new PropertyMetadata(ReleaseDownloader.ReleasesProperty_ValueChanged));

        private static readonly DependencyPropertyKey _documentPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ReleaseDownloader.Document), typeof(FlowDocument), typeof(ReleaseDownloader),
            new PropertyMetadata(new FlowDocument(), ReleaseDownloader.DocumentProperty_ValueChanged));

        private RichTextBox? _richTextBox;

        static ReleaseDownloader()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ReleaseDownloader), new FrameworkPropertyMetadata(typeof(ReleaseDownloader)));
            ReleaseDownloader.DocumentProperty = ReleaseDownloader._documentPropertyKey.DependencyProperty;
        }

        public FlowDocument Document
        {
            get => (FlowDocument)this.GetValue(ReleaseDownloader.DocumentProperty);
            private set => this.SetValue(ReleaseDownloader._documentPropertyKey, value);
        }

        public IReadOnlyCollection<IReleaseInfo>? Release
        {
            get => (IReadOnlyCollection<IReleaseInfo>?)this.GetValue(ReleaseDownloader.ReleasesProperty);
            set => this.SetValue(ReleaseDownloader.ReleasesProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if ((this._richTextBox = this.Template.FindName("PART_RichTextBox", this) as RichTextBox) != null)
            {
                this._richTextBox.Document = this.Document;
            }
        }

        private static void DocumentProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ReleaseDownloader downloader = (ReleaseDownloader)d;
            if (downloader._richTextBox != null)
            {
                downloader._richTextBox.Document = downloader.Document;
            }
        }

        private static void ReleasesProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ReleaseDownloader downloader = (ReleaseDownloader)d;
            IReadOnlyCollection<IReleaseInfo>? releases = (IReadOnlyCollection<IReleaseInfo>?)e.NewValue;
            FlowDocument document = new FlowDocument();
            if (releases != null)
            {
                SolidColorBrush alternateBrush = new SolidColorBrush(Color.FromArgb(8, 0, 0, 0));
                alternateBrush.Freeze();
                MarkdownEngine engine = new MarkdownEngine();
                foreach (IReleaseInfo release in releases)
                {
                    Section section = new Section { Background = document.Blocks.Count % 2 == 0 ? Brushes.White : alternateBrush };
                    section.Blocks.Add(new Paragraph
                    {
                        Inlines =
                    {
                        new Run($"{release.Name}{Environment.NewLine}Released:") { FontWeight = FontWeights.SemiBold },
                        new Run($" {GetDescriptiveText(DateTime.UtcNow - release.PublishedAt)} ago."),
                    }
                    });
                    section.Blocks.AddRange(engine.Transform(release.Body).Blocks.ToList());
                    document.Blocks.Add(section);
                }
            }
            downloader.Document = document;

            static string GetDescriptiveText(TimeSpan timeSpan)
            {
                if (timeSpan.TotalDays >= 365d)
                {
                    int years = (int)Math.Floor(timeSpan.TotalDays / 365d);
                    return years > 1 ? $"{years} years" : "A year";
                }
                if (timeSpan.TotalDays >= 30d)
                {
                    int months = (int)Math.Floor(timeSpan.TotalDays / 30d);
                    return months > 1 ? $"{months} months" : "A month";
                }
                if (timeSpan.TotalDays >= 1d)
                {
                    int days = (int)Math.Floor(timeSpan.TotalDays);
                    return days > 1 ? $"{days} days" : "A day";
                }
                if (timeSpan.TotalHours >= 1d)
                {
                    int hours = (int)Math.Floor(timeSpan.TotalHours);
                    return hours > 1 ? $"{hours} hours" : "An hour";
                }
                if (timeSpan.TotalMinutes >= 1d)
                {
                    int minutes = (int)Math.Floor(timeSpan.TotalMinutes);
                    return minutes > 1 ? $"{minutes} minutes" : "A minute";
                }
                return "Seconds";
            }
        }
    }
}
