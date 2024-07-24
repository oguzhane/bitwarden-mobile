using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using AndroidX.Activity.Result;

namespace Bit.Droid
{
    public partial class MainActivity : MauiAppCompatActivity
    {
        private string stamp = "";
        private static readonly ConcurrentDictionary<int, TaskCompletionSource<ActivityResult>> _oneTimeActivityListeners = new ConcurrentDictionary<int, TaskCompletionSource<ActivityResult>>();

        public void StartActivityForResult<T>(Intent intent, TaskCompletionSource<T> taskCompletionSource)
        {
            int requestCode = Math.Abs((int)DateTime.UtcNow.Ticks);
            _oneTimeActivityListeners[requestCode] = taskCompletionSource as TaskCompletionSource<ActivityResult>;

            StartActivityForResult(intent, requestCode);
        }
    }
}
