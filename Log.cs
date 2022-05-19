// Original source: https://github.com/wtfblub/Trashy

using BepInEx.Logging;

namespace StulPlugin
{
    public static class Log
    {
        private static readonly ManualLogSource Logger;

        static Log()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(StulPlugin));
        }

        public static void Debug(object value)
        {
            Logger.LogDebug(value);
        }

        public static void Debug<T>(object message)
        {
            Logger.LogDebug($"[{typeof(T).Name}] {message}");
        }

        public static void Info(object value)
        {
            Logger.LogInfo(value);
        }

        public static void Info<T>(object message)
        {
            Logger.LogInfo($"[{typeof(T).Name}] {message}");
        }

        public static void Warn(object value)
        {
            Logger.LogWarning(value);
        }

        public static void Warn<T>(object message)
        {
            Logger.LogWarning($"[{typeof(T).Name}] {message}");
        }

        public static void Error(object value)
        {
            Logger.LogError(value);
        }

        public static void Error<T>(object message)
        {
            Logger.LogError($"[{typeof(T).Name}] {message}");
        }
    }
}
