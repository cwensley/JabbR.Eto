using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;

namespace JabbR.Desktop
{
    public static class AsyncExtensions
    {

        public static Task<T> Catch<T>(this Task<T> task, Action<Exception> action)
        {
            task.ContinueWith(t => {
                action(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static Task ThenOnUI<T>(this Task<T> task, Action<T> action)
        {
            return ThenOnUI(task, t => {
                action(t); 
                return (object)null;
            });
        }

        public static Task<TResult> ThenOnUI<T, TResult>(this Task<T> task, Func<T, TResult> action)
        {
            var tcs = new TaskCompletionSource<TResult>();
            task.ContinueWith(t => {
                if (t.IsFaulted)
                    tcs.SetException(t.Exception);
                else if (t.IsCanceled)
                    tcs.SetCanceled();
                else if (t.IsCompleted)
                {
                    Application.Instance.AsyncInvoke(() => {
                        try
                        {
                            tcs.TrySetResult(action(t.Result));
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    });
                }
            });
            return tcs.Task;
        }
    }
}
