using KSTool.Om.Core;
using KSTool.Om.UserControls;

namespace KSTool.Om.Objects
{
    public class Hold : ObjectBase
    {
        protected double DesiredEndOffset;

        private double _realtimeEndY;
        private double _actualEndY;
        private int _endOffset;
        private double _desiredEndY;

        public double RealtimeEndY
        {
            get => _realtimeEndY;
            set => this.RaiseAndSetIfChanged(ref _realtimeEndY, value,
                additionalChangedMembers: nameof(RealtimeY));
        }

        public double ActualEndY
        {
            get => _actualEndY;
            set => this.RaiseAndSetIfChanged(ref _actualEndY, value);
        }

        public double DesiredEndY
        {
            get => _desiredEndY;
            set => this.RaiseAndSetIfChanged(ref _desiredEndY, value);
        }

        public override double RealtimeY { get; set; }

        public int EndOffset
        {
            get => _endOffset;
            set => this.RaiseAndSetIfChanged(ref _endOffset, value);
        }

        public Hold(TimelineViewerViewModel timelineViewerViewModel) : base(timelineViewerViewModel)
        {
        }

        public override void ResetInterface()
        {
            base.ResetInterface();
            ActualEndY = /*EditorVm.EditorHeight -*/ TimelineViewerViewModel.EditorScaleY * (EndOffset + 0);
            SetDesiredEndOffset(EndOffset);
            RealtimeEndY = ActualEndY;
        }

        public override void ApplyDesiredInterface()
        {
            EndOffset = (int)DesiredEndOffset;
            base.ApplyDesiredInterface();
        }

        public void SetDesiredEndOffset(double endOffset)
        {
            DesiredEndOffset = endOffset;
            DesiredEndY = TimelineViewerViewModel.EditorScaleY * (endOffset + 0);
        }
    }
}