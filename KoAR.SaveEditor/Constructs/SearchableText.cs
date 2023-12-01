﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace KoAR.SaveEditor.Constructs;

public sealed class SearchableText : Control
{
    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(nameof(SearchableText.SearchText), typeof(string), typeof(SearchableText),
        new(SearchableText.SearchTextProperty_ValueChanged));

    public static readonly DependencyProperty SegmentsProperty;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(SearchableText.Text), typeof(string), typeof(SearchableText),
        new(SearchableText.TextProperty_ValueChanged));

    private static readonly DependencyPropertyKey _segmentsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SearchableText.Segments), typeof(IReadOnlyList<Segment>), typeof(SearchableText),
        new(Array.Empty<Segment>()));

    static SearchableText()
    {
        SearchableText.SegmentsProperty = SearchableText._segmentsPropertyKey.DependencyProperty;
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableText), new FrameworkPropertyMetadata(typeof(SearchableText)));
    }

    public string? SearchText
    {
        get => (string?)this.GetValue(SearchableText.SearchTextProperty);
        set => this.SetValue(SearchableText.SearchTextProperty, value);
    }

    public IReadOnlyList<Segment> Segments
    {
        get => (IReadOnlyList<Segment>)this.GetValue(SearchableText.SegmentsProperty);
        private set => this.SetValue(SearchableText._segmentsPropertyKey, value);
    }

    public string? Text
    {
        get => (string?)this.GetValue(SearchableText.TextProperty);
        set => this.SetValue(SearchableText.TextProperty, value);
    }

    private static List<Segment> ComputeSegments(string? text, string? searchText)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }
        if (string.IsNullOrEmpty(searchText) || searchText.Length > text.Length)
        {
            return [(Segment)text];
        }
        List<Segment> list = [];
        int start = 0;
        while (start < text.Length)
        {
            int index = text.IndexOf(searchText, start, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
            {
                break;
            }
            if (index != start)
            {
                list.Add(text[start..index]);
            }
            list.Add(new(text.Substring(index, searchText.Length), true));
            start = index + searchText.Length;
        }
        if (start < text.Length)
        {
            list.Add(text[start..]);
        }
        return list;
    }

    private static void SearchTextProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SearchableText control = (SearchableText)d;
        control.Segments = SearchableText.ComputeSegments(control.Text, (string?)e.NewValue);
    }

    private static void TextProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SearchableText control = (SearchableText)d;
        control.Segments = SearchableText.ComputeSegments((string?)e.NewValue, control.SearchText);
    }

    public readonly record struct Segment(string Text, bool IsMatch)
    {
        public static implicit operator Segment(string text) => new(text, false);
    }
}