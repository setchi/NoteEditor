using System;
using System.Collections.Generic;

public class DisposableHolder : IDisposable
{
    protected List<IDisposable> disposables = new List<IDisposable>();

    protected void Disposable(params IDisposable[] disposables)
    {
        this.disposables.AddRange(disposables);
    }

    public void Dispose()
    {
        disposables.ForEach(disposable => disposable.Dispose());
        disposables.Clear();
    }
}
