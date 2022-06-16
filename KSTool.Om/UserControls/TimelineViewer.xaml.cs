using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coosu.Beatmap;
using Coosu.Beatmap.Sections.HitObject;
using KSTool.Om.Core;
using KSTool.Om.Objects;

namespace KSTool.Om.UserControls;

public sealed class TimelineViewerViewModel : ViewModelBase
{
    private int _editorDuration;
    private ObservableCollection<ObjectBase> _objects = new();
    private ObservableCollection<ObjectBase> _visibleObjects = new();
    private HashSet<ObjectBase> _existsObjHashSet = new();
    private int _keyMode;
    private double _editorWidth;
    private double _scrollViewerActualWidth;
    private double _scrollViewerActualHeight;
    private double _editorScaleY = 1d;

    private ObservableCollection<TimingLine> _timings = new();
    private ObservableCollection<TimingLine> _visibleTimings = new();
    private HashSet<TimingLine> _existsTimingHashSet = new();

    private ObservableCollection<TimingLine> _timingLines = new();
    private ObservableCollection<TimingLine> _visibleTimingLines = new();
    private HashSet<TimingLine> _existsTimingLineHashSet = new();
    private double _scrollViewerVerticalOffset;
    private double _endOffset;
    private double _startOffset;
    private int _editorRhythm;

    public OsuFile? OsuFile { get; set; }

    public double EditorHeight => EditorDuration * EditorScaleY;

    public double EditorWidth
    {
        get => _editorWidth;
        set => this.RaiseAndSetIfChanged(ref _editorWidth, value);
    }

    public int EditorDuration
    {
        get => _editorDuration;
        set => this.RaiseAndSetIfChanged(ref _editorDuration, value,
            additionalChangedMembers: nameof(EditorHeight)
        );
    }

    public double EditorScaleY
    {
        get => _editorScaleY;
        set
        {
            if (value.Equals(_editorScaleY)) return;
            OnPropertyChanging();
            _editorScaleY = value;

            SetVisibleObjects();
            SetVisibleTimingLines();
            SetVisibleTimings();
            ResetInterface();

            OnPropertyChanged(nameof(EditorHeight));
            OnPropertyChanged();
        }
    }

    public int EditorRhythm
    {
        get => _editorRhythm;
        set
        {
            if (value == _editorRhythm) return;
            _editorRhythm = value;

            ReloadTimings();
            SetVisibleTimingLines();
            SetVisibleTimings();
            ResetTimingInterface();

            OnPropertyChanged();
        }
    }

    public ObservableCollection<ObjectBase> Objects
    {
        get => _objects;
        set => this.RaiseAndSetIfChanged(ref _objects, value);
    }

    public ObservableCollection<ObjectBase> VisibleObjects
    {
        get => _visibleObjects;
        set => this.RaiseAndSetIfChanged(ref _visibleObjects, value);
    }

    public ObservableCollection<ObjectBase> SelectedObjects { get; } = new();

    public ObservableCollection<TimingLine> TimingLines
    {
        get => _timingLines;
        set => this.RaiseAndSetIfChanged(ref _timingLines, value);
    }

    public ObservableCollection<TimingLine> VisibleTimingLines
    {
        get => _visibleTimingLines;
        set => this.RaiseAndSetIfChanged(ref _visibleTimingLines, value);
    }

    public ObservableCollection<TimingLine> Timings
    {
        get => _timings;
        set => this.RaiseAndSetIfChanged(ref _timings, value);
    }

    public ObservableCollection<TimingLine> VisibleTimings
    {
        get => _visibleTimings;
        set => this.RaiseAndSetIfChanged(ref _visibleTimings, value);
    }

    public int KeyMode
    {
        get => _keyMode;
        set => this.RaiseAndSetIfChanged(ref _keyMode, value);
    }

    public double ScrollViewerVerticalOffset
    {
        get => _scrollViewerVerticalOffset;
        set
        {
            if (value.Equals(_scrollViewerVerticalOffset)) return;
            _scrollViewerVerticalOffset = value;
            OnPropertyChanged();
            StartOffset = (EditorHeight - (ScrollViewerVerticalOffset + ScrollViewerActualHeight)) / EditorScaleY;
            EndOffset = (EditorHeight - ScrollViewerVerticalOffset) / EditorScaleY;
            //Console.WriteLine($"StartOffset: {StartOffset}; EndOffset: {EndOffset}");
        }
    }

    public double ScrollViewerActualWidth
    {
        get => _scrollViewerActualWidth;
        set => this.RaiseAndSetIfChanged(ref _scrollViewerActualWidth, value);
    }

    public double ScrollViewerActualHeight
    {
        get => _scrollViewerActualHeight;
        set => this.RaiseAndSetIfChanged(ref _scrollViewerActualHeight, value);
    }

    public double StartOffset
    {
        get => _startOffset;
        set => this.RaiseAndSetIfChanged(ref _startOffset, value);
    }

    public double EndOffset
    {
        get => _endOffset;
        set => this.RaiseAndSetIfChanged(ref _endOffset, value);
    }

    public void Load(OsuFile osuFile)
    {
        this.OsuFile = osuFile;
        // init editors
        EditorDuration = (int)(osuFile.HitObjects.MaxTime + 3000);
        EditorScaleY = 0.25;
        EditorRhythm = 4;
    }

    public void ReloadTimings()
    {
        if (OsuFile == null) return;

        double multiple = 1d / EditorRhythm;
        //var timingIntervals = OsuFile.TimingPoints.GetInterval(0.25);
        var redLines = OsuFile.TimingPoints.TimingList
            .OrderBy(t => t.Offset)
            .Where(k => !k.IsInherit)
            .ToArray();
        var list = new List<TimingLine>();
        var timings = new List<TimingLine>();
        for (int i = 0; i < redLines.Length; i++)
        {
            decimal nextTime = Convert.ToDecimal(i == redLines.Length - 1
                ? EditorDuration
                : redLines[i + 1].Offset);
            var t = redLines[i];
            decimal decBpm = Convert.ToDecimal(t.Bpm);
            decimal decMult = Convert.ToDecimal(multiple);
            decimal interval = 60000 / decBpm * decMult;
            decimal current = Convert.ToDecimal(t.Offset);
            var count = (int)Math.Round(1 / decMult);
            var j = 0;
            while (current < nextTime)
            {
                var lineVm = new TimingLine(this)
                {
                    Offset = Convert.ToInt32(current),
                    Rhythm = j / (double)count
                };

                lineVm.ResetInterface();
                list.Add(lineVm);
                if (j == 0)
                {
                    //var lineVm2 = new TimingLine(this)
                    //{
                    //    Offset = Convert.ToInt32(current),
                    //    Rhythm = 0
                    //};

                    //lineVm2.ResetInterface();
                    timings.Add(lineVm);
                }

                current += interval;
                j++;
                if (j == count) j = 0;
            }
        }

        TimingLines = new ObservableCollection<TimingLine>(list);
        Timings = new ObservableCollection<TimingLine>(timings);
    }


    public void LoadObjects()
    {
        if (OsuFile == null) return;

        KeyMode = (int)OsuFile.Difficulty.CircleSize;
        var enumerable = OsuFile.HitObjects.HitObjectList.Select(k =>
        {
            ObjectBase objectBase;
            if (k.ObjectType == HitObjectType.Circle)
            {
                objectBase = new Note(this);
            }
            else if (k.ObjectType == HitObjectType.Hold)
            {
                var hold = new Hold(this)
                {
                    EndOffset = k.HoldEnd,
                };
                objectBase = hold;
            }
            else
            {
                throw new Exception("unknown obj type: " + k.ObjectType);
            }

            objectBase.Offset = k.Offset;
            objectBase.ComputeInterfaceFromOriginalFormat(k.X);
            return objectBase;
        });
        Objects = new ObservableCollection<ObjectBase>(enumerable);
    }

    public void SetVisibleObjects()
    {
        if (Objects.Count == 0) return;
        var startOffset = EditorHeight - (ScrollViewerVerticalOffset + ScrollViewerActualHeight) - 67;
        var endOffset = EditorHeight - ScrollViewerVerticalOffset + 67;
        var startTime = startOffset / EditorScaleY /*- 200*/;
        var endTime = endOffset / EditorScaleY /*+ 200*/;

        //Console.WriteLine("topOffset: " + topOffset + "; bottomOffset: " + bottomOffset +
        //                  "; startTime: " + startTime + "; endTime: " + endTime);
        var visibles =
            new HashSet<ObjectBase>(
                Objects.Where(k =>
                {
                    if (k is Hold h)
                        return h.EndOffset >= startTime && k.Offset < endTime;
                    return k.Offset >= startTime && k.Offset < endTime;
                })
            );
        var newObjs = visibles.Where(k => !_existsObjHashSet.Contains(k));
        var existObjs = visibles.Where(k => _existsObjHashSet.Contains(k));
        var notExistsAnyMore = _existsObjHashSet.Except(existObjs.Concat(newObjs));
        foreach (var objectBase in notExistsAnyMore)
        {
            VisibleObjects.Remove(objectBase);
        }

        foreach (var objectBase in newObjs)
        {
            VisibleObjects.Add(objectBase);
        }

        _existsObjHashSet = visibles;
    }

    public void SetVisibleTimings()
    {
        if (Timings.Count == 0) return;
        var startOffset = EditorHeight - (ScrollViewerVerticalOffset + ScrollViewerActualHeight) - 67;
        var endOffset = EditorHeight - ScrollViewerVerticalOffset + 67;
        var startTime = startOffset / EditorScaleY /*- 200*/;
        var endTime = endOffset / EditorScaleY /*+ 200*/;

        //Console.WriteLine("topOffset: " + topOffset + "; bottomOffset: " + bottomOffset +
        //                  "; startTime: " + startTime + "; endTime: " + endTime);
        var visibles =
            new HashSet<TimingLine>(
                Timings.Where(k => k.Offset >= startTime && k.Offset < endTime)
            );
        var newTimings = visibles.Where(k => !_existsTimingHashSet.Contains(k));
        var existTimings = visibles.Where(k => _existsTimingHashSet.Contains(k));
        var notExistsAnyMore = _existsTimingHashSet.Except(existTimings.Concat(newTimings));
        foreach (var objectBase in notExistsAnyMore)
        {
            VisibleTimings.Remove(objectBase);
        }

        foreach (var objectBase in newTimings)
        {
            VisibleTimings.Add(objectBase);
        }

        _existsTimingHashSet = visibles;
    }

    public void SetVisibleTimingLines()
    {
        if (TimingLines.Count == 0) return;
        var startOffset = EditorHeight - (ScrollViewerVerticalOffset + ScrollViewerActualHeight) - 67;
        var endOffset = EditorHeight - ScrollViewerVerticalOffset + 67;
        var startTime = startOffset / EditorScaleY /*- 200*/;
        var endTime = endOffset / EditorScaleY /*+ 200*/;

        //Console.WriteLine("topOffset: " + topOffset + "; bottomOffset: " + bottomOffset +
        //                  "; startTime: " + startTime + "; endTime: " + endTime);
        var visibles =
            new HashSet<TimingLine>(
                TimingLines.Where(k => k.Offset >= startTime && k.Offset < endTime)
            );
        var newTimings = visibles.Where(k => !_existsTimingLineHashSet.Contains(k));
        var existTimings = visibles.Where(k => _existsTimingLineHashSet.Contains(k));
        var notExistsAnyMore = _existsTimingLineHashSet.Except(existTimings.Concat(newTimings));
        foreach (var objectBase in notExistsAnyMore)
        {
            VisibleTimingLines.Remove(objectBase);
        }

        foreach (var objectBase in newTimings)
        {
            VisibleTimingLines.Add(objectBase);
        }

        _existsTimingLineHashSet = visibles;
    }

    public void ResetInterface()
    {
        EditorWidth = ScrollViewerActualWidth;
        foreach (var visibleObject in Objects)
        {
            visibleObject.ResetInterface();
        }

        ResetTimingInterface();
    }

    public void ResetTimingInterface()
    {
        foreach (var visibleObject in Timings)
        {
            visibleObject.ResetInterface();
        }

        foreach (var visibleObject in TimingLines)
        {
            visibleObject.ResetInterface();
        }
    }
}

/// <summary>
/// Editor.xaml 的交互逻辑
/// </summary>
public partial class TimelineViewer : UserControl
{
    private readonly TimelineViewerViewModel _viewModel;
    public TimelineViewer()
    {
        InitializeComponent();
        DataContext = _viewModel = new TimelineViewerViewModel();
        double startOffset = 0;
        _viewModel.PropertyChanging += (s, e) =>
        {
            if (e.PropertyName == nameof(TimelineViewerViewModel.EditorScaleY))
            {
                startOffset = _viewModel.StartOffset;
                //Console.WriteLine($"Last StartOffset is: {_startOffset}. Ready to reposition.");
            }
        };
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TimelineViewerViewModel.EditorScaleY))
            {
                var viewModelEditorScaleY = _viewModel.EditorHeight -
                                            _viewModel.EditorScaleY * (startOffset) -
                                            _viewModel.ScrollViewerActualHeight;
                ScrollViewer.ScrollToVerticalOffset(viewModelEditorScaleY);
            }
        };
    }

    public void Load(OsuFile osuFile)
    {
        _viewModel.Load(osuFile);
        _viewModel.EditorWidth = ScrollViewer.ViewportWidth;
        _viewModel.ScrollViewerActualWidth = ScrollViewer.ViewportWidth;
        _viewModel.ScrollViewerActualHeight = ScrollViewer.ActualHeight;
        _viewModel.ReloadTimings();
        _viewModel.LoadObjects();
        ScrollViewer.ScrollToBottom();
    }

    private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        _viewModel.ScrollViewerVerticalOffset = ScrollViewer.VerticalOffset;
        _viewModel.SetVisibleObjects();
        _viewModel.SetVisibleTimingLines();
        _viewModel.SetVisibleTimings();

    }

    private void ScrollViewer_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _viewModel.ScrollViewerActualWidth = ScrollViewer.ViewportWidth;
        _viewModel.ScrollViewerActualHeight = ScrollViewer.ActualHeight;
        _viewModel.ResetInterface();
    }
}