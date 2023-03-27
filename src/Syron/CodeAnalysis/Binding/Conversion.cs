using Syron.CodeAnalysis.Symbols;

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class Conversion
    {
        public static readonly Conversion Identity = new Conversion(true, true, true);
        public static readonly Conversion Implicit = new Conversion(true, false, true);
        public static readonly Conversion Explicit = new Conversion(true, false, false);
        public static readonly Conversion None = new Conversion(false, false, false);

        private Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsImplicit = isImplicit;
            IsIdentity = isIdentity;
        }

        public bool IsIdentity { get; }
        public bool Exists { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;

        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            // Identity
            if (from == to)
                return Conversion.Identity;

            if (from == TypeSymbol.Bool || from == TypeSymbol.Int)
            {
                if (to == TypeSymbol.String)
                    return Conversion.Explicit;
            }

            if (from == TypeSymbol.String)
            {
                if (to == TypeSymbol.Int)
                    return Conversion.Explicit;
            }

            return Conversion.None;
        }
    }
}