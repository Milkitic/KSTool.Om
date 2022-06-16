using System.Windows;
using KSTool.Om.UserControls;

namespace KSTool.Om.Objects
{
    public class TimingLine : ObjectBase
    {
        public double Rhythm { get; set; }

        public TimingLine(TimelineViewerViewModel timelineViewerViewModel) : base(timelineViewerViewModel)
        {
        }

        public override void ComputeInterfaceFromOriginalFormat(int x)
        {
            Column = -1;
        }

        public override void ResetInterface()
        {
            RealtimeX = 0;
            RealtimeY = TimelineViewerViewModel.EditorScaleY * (Offset + 0);
            RealtimeWidth = TimelineViewerViewModel.EditorWidth;
            RealtimeWidthPoint = new Point(TimelineViewerViewModel.EditorWidth, 0);
        }
    }
}