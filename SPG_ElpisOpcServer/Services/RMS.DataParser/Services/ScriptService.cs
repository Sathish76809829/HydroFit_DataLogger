using FluidScript;
using FluidScript.Compiler;
using FluidScript.Runtime;
using RMS.Service.Abstractions;
using RMS.Service.Abstractions.Models;
using RMS.Service.Abstractions.Services;
using RMS.Service.Models;
using System;
using System.Collections.Generic;

namespace RMS.DataParser.Services
{
    /// <summary>
    /// Script service for RMS which will execute basic math calculation
    /// </summary>
    public class ScriptService : IScriptService
    {
        private readonly RuntimeCompiler compiler;

        private readonly ILocalVariables variables;

        public ScriptService()
        {
            compiler = new RuntimeCompiler();
            variables = compiler.Locals;
        }

        public IDictionary<string, object> Values => variables;

        public IScriptScope Begin(SignalSet<DataSendModel> values)
        {
            var scope = new ScriptScope(values, compiler.EnterScope());
            variables.DeclareVariable("sid", (Func<FluidScript.String, Any>)scope.FindById);
            return scope;
        }

        public object Invoke(string script)
        {
            var statement = Parser.GetStatement(script);
            return compiler.Invoke(statement);
        }

        readonly struct ScriptScope : IScriptScope, ISignalComparer</*int*/string>
        {
            public readonly SignalSet<DataSendModel> Values;
            private readonly ILocalScopeVariables context;

            public IDictionary<string, object> Variables => context.Locals;

            public ScriptScope(SignalSet<DataSendModel> values, ILocalScopeVariables context)
            {
                Values = values;
                this.context = context;
            }

            public Any FindById(FluidScript.String sid)
            {
                if (Values.TryGetValue(sid, this, out var value))
                {
                    return new Any(value.DataValue);
                }
                return new Integer(0);
            }

            public void Dispose()
            {
                context.Dispose();
            }

            public bool Equals(ISignalModel x, /*int*/string y)
            {
                return x.SignalId == y;
            }

            public int GetHashCode(/*int*/string item)
            {
                return item.GetHashCode(); ;
            }
        }
    }
}
