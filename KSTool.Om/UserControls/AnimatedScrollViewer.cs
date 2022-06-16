using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace KSTool.Om.UserControls;

public class AnimatedScrollViewer : ScrollViewer
{
    /// <summary>
    /// ��������
    /// </summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        "Orientation",
        typeof(Orientation),
        typeof(AnimatedScrollViewer),
        new PropertyMetadata(Orientation.Vertical));

    /// <summary>
    /// �Ƿ���Ӧ�����ֲ���
    /// </summary>
    public static readonly DependencyProperty CanMouseWheelProperty = DependencyProperty.Register(
        "CanMouseWheel",
        typeof(bool),
        typeof(AnimatedScrollViewer),
        new PropertyMetadata(true));

    /// <summary>
    /// �Ƿ�֧��ƽ������
    /// </summary>
    public static readonly DependencyProperty IsSmoothScrollingEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsSmoothScrollingEnabled",
            typeof(bool),
            typeof(AnimatedScrollViewer),
            new PropertyMetadata(true));

    /// <summary>
    /// ��ֱ��������
    /// </summary>
    public static readonly DependencyProperty VerticalScrollingDistanceProperty = DependencyProperty.Register(
        "VerticalScrollingDistance",
        typeof(double),
        typeof(AnimatedScrollViewer),
        new PropertyMetadata(120d));

    /// <summary>
    /// ˮƽ��������
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollingDistanceProperty = DependencyProperty.Register(
        "HorizontalScrollingDistance",
        typeof(double),
        typeof(AnimatedScrollViewer),
        new PropertyMetadata(120d));

    /// <summary>
    /// ��ǰ��ֱ����ƫ��
    /// </summary>
    internal static readonly DependencyProperty CurrentVerticalOffsetProperty = DependencyProperty.Register(
        "CurrentVerticalOffset",
        typeof(double),
        typeof(AnimatedScrollViewer),
        new PropertyMetadata(0d, OnCurrentVerticalOffsetChanged));

    private static void OnCurrentVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimatedScrollViewer ctl && e.NewValue is double v)
        {
            ctl.ScrollToVerticalOffset(v);
        }
    }

    /// <summary>
    /// ��ǰˮƽ����ƫ��
    /// </summary>
    internal static readonly DependencyProperty CurrentHorizontalOffsetProperty = DependencyProperty.Register(
        "CurrentHorizontalOffset",
        typeof(double),
        typeof(AnimatedScrollViewer),
        new PropertyMetadata(0d, OnCurrentHorizontalOffsetChanged));

    private static void OnCurrentHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimatedScrollViewer ctl && e.NewValue is double v)
        {
            ctl.ScrollToHorizontalOffset(v);
        }
    }

    private double _totalVerticalOffset;
    private double _totalHorizontalOffset;
    private readonly List<Action> _verticalDictionary = new();
    private readonly List<Action> _horizontalDictionary = new();

    private readonly CubicEase _scrollEasing = new() { EasingMode = EasingMode.EaseOut };

    public AnimatedScrollViewer()
    {
        ScrollChanged += Vertical_ScrollChanged;
        ScrollChanged += Horizontal_ScrollChanged;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// �Ƿ���Ӧ�����ֲ���
    /// </summary>
    public bool CanMouseWheel
    {
        get => (bool)GetValue(CanMouseWheelProperty);
        set => SetValue(CanMouseWheelProperty, value);
    }

    /// <summary>
    /// �Ƿ�֧�ֹ���
    /// </summary>
    public bool IsSmoothScrollingEnabled
    {
        get => (bool)GetValue(IsSmoothScrollingEnabledProperty);
        set => SetValue(IsSmoothScrollingEnabledProperty, value);
    }

    /// <summary>
    /// ��ֱ��������
    /// </summary>
    public double VerticalScrollingDistance
    {
        get => (double)GetValue(VerticalScrollingDistanceProperty);
        set => SetValue(VerticalScrollingDistanceProperty, value);
    }

    /// <summary>
    /// ˮƽ��������
    /// </summary>
    public double HorizontalScrollingDistance
    {
        get => (double)GetValue(HorizontalScrollingDistanceProperty);
        set => SetValue(HorizontalScrollingDistanceProperty, value);
    }

    /// <summary>
    /// ��ǰ��ֱ����ƫ��
    /// </summary>
    internal double CurrentVerticalOffset
    {
        get => (double)GetValue(CurrentVerticalOffsetProperty);
        set => SetValue(CurrentVerticalOffsetProperty, value);
    }

    /// <summary>
    /// ��ǰˮƽ����ƫ��
    /// </summary>
    internal double CurrentHorizontalOffset
    {
        get => (double)GetValue(CurrentHorizontalOffsetProperty);
        set => SetValue(CurrentHorizontalOffsetProperty, value);
    }

    public void ScrollToVerticalOffsetWithAnimation(double offset, double milliseconds = 500,
        Action? onCompleted = null)
    {
        _totalVerticalOffset = offset;
        var verticalAnimation = new DoubleAnimation(offset, new Duration(TimeSpan.FromMilliseconds(milliseconds)))
        {
            EasingFunction = _scrollEasing,
            FillBehavior = FillBehavior.HoldEnd
        };

        if (_verticalDictionary.Count > 0)
        {
            _verticalDictionary[_verticalDictionary.Count - 1] = null;
        }

        _verticalDictionary.Add(() =>
        {
            ScrollToVerticalOffset(offset);
            CurrentVerticalOffset = offset;
            _totalVerticalOffset = offset;
            _verticalDictionary.Clear();
            BeginAnimation(CurrentVerticalOffsetProperty, null, HandoffBehavior.SnapshotAndReplace);
            ScrollChanged += Vertical_ScrollChanged;
            onCompleted?.Invoke();
        });

        var thisIndex = _verticalDictionary.Count - 1;
        verticalAnimation.Completed += (_, _) =>
        {
            if (thisIndex > _verticalDictionary.Count - 1) return;
            ((Action?)_verticalDictionary[thisIndex])?.Invoke();
        };

        ScrollChanged -= Vertical_ScrollChanged;
        BeginAnimation(CurrentVerticalOffsetProperty, verticalAnimation, HandoffBehavior.Compose);
    }

    public void ScrollToHorizontalOffsetWithAnimation(double offset, double milliseconds = 500,
        Action? onCompleted = null)
    {
        _totalHorizontalOffset = offset;
        var horizontalAnimation = new DoubleAnimation(offset, new Duration(TimeSpan.FromMilliseconds(milliseconds)))
        {
            EasingFunction = _scrollEasing,
            FillBehavior = FillBehavior.HoldEnd
        };

        if (_horizontalDictionary.Count > 0)
        {
            _horizontalDictionary[_horizontalDictionary.Count - 1] = null;
        }

        _horizontalDictionary.Add(() =>
        {
            ScrollToHorizontalOffset(offset);
            CurrentHorizontalOffset = offset;
            _totalHorizontalOffset = offset;
            _horizontalDictionary.Clear();
            BeginAnimation(CurrentHorizontalOffsetProperty, null, HandoffBehavior.SnapshotAndReplace);
            ScrollChanged += Horizontal_ScrollChanged;
            onCompleted?.Invoke();
        });

        var thisIndex = _horizontalDictionary.Count - 1;
        horizontalAnimation.Completed += (_, _) =>
        {
            if (thisIndex > _horizontalDictionary.Count - 1) return;
            ((Action?)_horizontalDictionary[thisIndex])?.Invoke();
        };

        ScrollChanged -= Horizontal_ScrollChanged;
        BeginAnimation(CurrentHorizontalOffsetProperty, horizontalAnimation, HandoffBehavior.Compose);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!CanMouseWheel) return;
        if (IsSmoothScrollingEnabled)
        {
            e.Handled = true;
            if (Orientation == Orientation.Vertical)
            {
                var delta = Math.Sign(e.Delta) * VerticalScrollingDistance;
                _totalVerticalOffset = Math.Min(Math.Max(0, _totalVerticalOffset - delta), ScrollableHeight);
                ScrollToVerticalOffsetWithAnimation(_totalVerticalOffset);
            }
            else
            {
                var delta = Math.Sign(e.Delta) * HorizontalScrollingDistance;
                _totalHorizontalOffset = Math.Min(Math.Max(0, _totalHorizontalOffset - delta), ScrollableWidth);
                ScrollToHorizontalOffsetWithAnimation(_totalHorizontalOffset);
            }
        }
        else
        {
            if (Orientation == Orientation.Vertical)
            {
                base.OnMouseWheel(e);
            }
            else
            {
                _totalHorizontalOffset = HorizontalOffset;
                CurrentHorizontalOffset = HorizontalOffset;
                _totalHorizontalOffset = Math.Min(Math.Max(0, _totalHorizontalOffset - e.Delta), ScrollableWidth);
                CurrentHorizontalOffset = _totalHorizontalOffset;
            }
        }
    }

    private void Vertical_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.VerticalChange == 0) return;
        CurrentVerticalOffset = e.VerticalOffset;
        _totalVerticalOffset = e.VerticalOffset;
    }

    private void Horizontal_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange == 0) return;
        CurrentHorizontalOffset = e.HorizontalOffset;
        _totalHorizontalOffset = e.HorizontalOffset;
    }
}