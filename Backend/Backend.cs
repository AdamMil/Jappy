using System;

namespace Jappy.Backend
{

static class Utilities
{
  public static void Dispose<T>(ref T disposable) where T : class, IDisposable
  {
    if(disposable != null)
    {
      disposable.Dispose();
      disposable = null;
    }
  }
}

} // namespace Jappy.Backend