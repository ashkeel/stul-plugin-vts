using System;
using System.Threading.Tasks;

namespace StulPlugin
{
    public class TimerUtils
    {
        public static async void RunAfter(int millisecond, Action action)
        {
            await Task.Delay(millisecond);
            action();
        }
    }
}