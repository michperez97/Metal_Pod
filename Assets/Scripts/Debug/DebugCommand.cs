#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;

namespace MetalPod.Debugging
{
    /// <summary>
    /// Immutable descriptor for a debug console command.
    /// </summary>
    public sealed class DebugCommand
    {
        public string Name { get; }
        public string Description { get; }
        public string Usage { get; }
        public Func<string[], string> Execute { get; }

        public DebugCommand(string name, string description, string usage, Func<string[], string> execute)
        {
            Name = string.IsNullOrWhiteSpace(name) ? string.Empty : name.ToLowerInvariant();
            Description = description ?? string.Empty;
            Usage = usage ?? string.Empty;
            Execute = execute ?? (_ => string.Empty);
        }
    }
}
#endif
