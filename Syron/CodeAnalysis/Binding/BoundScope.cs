using System.Collections.Immutable;

namespace Syron.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        // This is a field that will be used to store the variables that are declared in the scope.
        // The key is the name of the variable and the value is the variable itself.
        private readonly Dictionary<string, VariableSymbol> _variables = new Dictionary<string, VariableSymbol>();

        // This is a constructor that will be used to initialize the Parent field.
        // The parent field will be used to store the parent scope of the current scope.
        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        // This is a property that will be used to return the parent scope of the current scope.
        public BoundScope Parent { get; }

        // This is a method that will be used to declare a variable in the current scope.
        // If the variable is already declared in the current scope, the method will return false.
        public bool TryDeclare(VariableSymbol variable)
        {
            if (_variables.ContainsKey(variable.Name))
                return false;

            _variables.Add(variable.Name, variable);
            return true;
        }

        // This is a method that will be used to lookup a variable in the current scope and all of its parent scopes.
        public bool TryLookup(string name, out VariableSymbol variable)
        {
            if (_variables.TryGetValue(name, out variable))
                return true;

            if (Parent != null)
                return Parent.TryLookup(name, out variable);

            return false;
        }

        // This is a method that will be used to return an ImmutableArray of all the variables declared in the current scope.
        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            return _variables.Values.ToImmutableArray();
        }
    }
}
