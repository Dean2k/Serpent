using ReModCE_ARES.Loader;
using System.Threading.Tasks;

namespace ReModCE_ARES.Core
{
    internal static class TaskExtensions
    {
        public static void NoAwait(this Task task)
        {
            task.ContinueWith(t =>
            {
                if (!t.IsFaulted || t.Exception == null) return;

                ReLogger.Msg($"Free-floating task failed: {t.Exception}");
            });
        }
    }
}
