using ICSharpCode.CodeCompletion;

namespace QuickSharpApp
{
    class ScriptProvider : ICSharpScriptProvider
    {
        public string GetUsing()
        {
            return "" +
                "using System; " +
                "using System.Collections.Generic; " +
                "using System.Linq; " +
                "using System.Text; ";
        }
        
        public string GetVars() => null;

        public string GetNamespace() => null;
    }
}
