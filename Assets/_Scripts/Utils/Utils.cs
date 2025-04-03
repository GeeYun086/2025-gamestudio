using System;

namespace GravityGame.Utils
{
    public static class Utils
    {
        /// <summary>
        ///     Simple utility for when you need to execute some additional code on an object in a single line.
        ///     Useful, for example, when you are not allowed to make a new line (like in a switch expression).
        ///     <code>
        ///         return vector.With(vec => vec.y = 0) // example usage
        ///     </code>
        /// </summary>
        public static T With<T>(this T arg, Action<T> action)
        {
            action(arg);
            return arg;
        }
    }
}