using System.Windows;
using KSTool.Om.Core;
using KSTool.Om.UserControls;

namespace KSTool.Om.Objects
{
    public abstract class ObjectBase : ViewModelBase
    {
        protected readonly TimelineViewerViewModel TimelineViewerViewModel;
        protected double DesiredOffset;
        protected int DesiredColumn;

        public ObjectBase(TimelineViewerViewModel timelineViewerViewModel)
        {
            TimelineViewerViewModel = timelineViewerViewModel;
        }

        private double _realtimeX;
        private double _realtimeY;
        private double _actualX;
        private double _actualY;
        private double _desiredX;
        private double _desiredY;
        private int _offset;
        private int _column;
        private double _realtimeWidth;
        private Point _realtimeWidthPoint;

        /// <summary>
        /// 0 ~ 3
        /// </summary>
        public int Column
        {
            get => _column;
            set
            {
                this.RaiseAndSetIfChanged(ref _column, value);
                DesiredColumn = value;
            }
        }

        public int Offset
        {
            get => _offset;
            set
            {
                this.RaiseAndSetIfChanged(ref _offset, value);
                DesiredOffset = value;
            }
        }

        public double RealtimeX
        {
            get => _realtimeX;
            set => this.RaiseAndSetIfChanged(ref _realtimeX, value);
        }

        public virtual double RealtimeY
        {
            get => _realtimeY;
            set => this.RaiseAndSetIfChanged(ref _realtimeY, value);
        }

        public double ActualX
        {
            get => _actualX;
            set => this.RaiseAndSetIfChanged(ref _actualX, value);
        }

        public double ActualY
        {
            get => _actualY;
            set => this.RaiseAndSetIfChanged(ref _actualY, value);
        }

        public double DesiredX
        {
            get => _desiredX;
            set => this.RaiseAndSetIfChanged(ref _desiredX, value);
        }

        public double DesiredY
        {
            get => _desiredY;
            set => this.RaiseAndSetIfChanged(ref _desiredY, value);
        }

        public double RealtimeWidth
        {
            get => _realtimeWidth;
            set => this.RaiseAndSetIfChanged(ref _realtimeWidth, value);
        }

        public Point RealtimeWidthPoint
        {
            get => _realtimeWidthPoint;
            set => this.RaiseAndSetIfChanged(ref _realtimeWidthPoint, value);
        }

        public virtual void ComputeInterfaceFromOriginalFormat(int x)
        {
            var total = 512;
            var columnWidth = total / TimelineViewerViewModel.KeyMode;
            var columnWidthHalf = columnWidth / 2;
            var column = (x - columnWidthHalf) / columnWidth;

            Column = column;
            ResetInterface();
        }


        public virtual void ResetInterface()
        {
            ActualX = TimelineViewerViewModel.EditorWidth * Column / TimelineViewerViewModel.KeyMode;
            ActualY =/* EditorVm.EditorHeight -*/ TimelineViewerViewModel.EditorScaleY * (Offset + 0);
            RealtimeX = ActualX;
            RealtimeY = ActualY;
            SetDesiredInterface(Column, Offset);
            RealtimeWidth = TimelineViewerViewModel.EditorWidth / TimelineViewerViewModel.KeyMode;
            RealtimeWidthPoint = new Point(RealtimeWidth, 0);
        }

        public void SetDesiredInterface(int column, double offset)
        {
            DesiredOffset = offset;
            DesiredColumn = column;
            DesiredX = TimelineViewerViewModel.EditorWidth * column / TimelineViewerViewModel.KeyMode;
            DesiredY = TimelineViewerViewModel.EditorScaleY * (offset + 0);
        }

        public virtual void ApplyDesiredInterface()
        {
            Offset = (int)DesiredOffset;
            Column = DesiredColumn;
            ResetInterface();
        }
    }
}