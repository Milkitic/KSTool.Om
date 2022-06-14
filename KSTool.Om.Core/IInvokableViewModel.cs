namespace KSTool.Om.Core;

public interface IInvokableViewModel
{
    internal void RaisePropertyChanged(string propertyName);
    internal void RaisePropertyChanging(string propertyName);
}