using Coosu.Beatmap.Sections.HitObject;

namespace KSTool.Om.Core;

public class HitObjectComparer : IEqualityComparer<RawHitObject>
{
    private HitObjectComparer()
    {
    }

    public static IEqualityComparer<RawHitObject> Instance { get; } = new HitObjectComparer();


    public bool Equals(RawHitObject? x, RawHitObject? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.X == y.X && x.Offset == y.Offset && x.ObjectType == y.ObjectType;
    }

    public int GetHashCode(RawHitObject obj)
    {
        return HashCode.Combine(obj.X, obj.Offset, (int)obj.ObjectType);
    }
}