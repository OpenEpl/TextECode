using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems
{
    public enum ProgramElemKind
    {
        Type,
        Method,
        Var,
        Constant
    }

    public struct ProgramElemName : IEquatable<ProgramElemName>
    {
        public static ProgramElemName Type(string name) => new(ProgramElemKind.Type, name);
        public static ProgramElemName Method(string name) => new(ProgramElemKind.Method, name);
        public static ProgramElemName Var(string name) => new(ProgramElemKind.Var, name);
        public static ProgramElemName Constant(string name) => new(ProgramElemKind.Constant, name);

        public ProgramElemName(ProgramElemKind kind, string name)
        {
            Kind = kind;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public ProgramElemKind Kind { get; }
        public string Name { get; }

        public override bool Equals(object obj)
        {
            return obj is ProgramElemName name && Equals(name);
        }

        public bool Equals(ProgramElemName other)
        {
            return Kind == other.Kind && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Name);
        }

        public override string ToString()
        {
            return $"<{Kind}:{Name}>";
        }

        public static bool operator ==(ProgramElemName left, ProgramElemName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProgramElemName left, ProgramElemName right)
        {
            return !(left == right);
        }
    }
}
