// ReSharper disable once CheckNamespace

namespace System.Runtime.CompilerServices
{
    /// <summary>
    ///     Compiler fix for primary constructors for records not working
    ///     https://stackoverflow.com/a/64749403
    /// </summary>
    internal static class IsExternalInit { }
}