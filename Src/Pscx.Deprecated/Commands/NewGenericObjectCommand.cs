//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: Pscx\New-Object
//
// Features:
//  - 100% compatible with native New-Object plus...
//  - Create generic types, e.g. Dictionary<Int,String>
//  - Create arbitrary delegates/generic delegates bound to ScriptBlocks
//
// Syntax:
//  - $dict = pscx\new-object collections.generic.dictionary -of int,string 15
//  - $action = pscx\new-object action -of string { [diagnostics.debug]::writeline($args[0], "string"); }
//  - $converter = pscx\new-object converter -of string,int { [int]::parse($args[0]) }
//
// Creation Date: Oct 2, 2007
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Pscx.Commands;

namespace Pscx.Deprecated.Commands
{
    [Cmdlet(VerbsCommon.New, PscxNouns.GenericObject, DefaultParameterSetName = "Net")]
    public class NewGenericObjectCommand : PscxCmdlet
    {
        private object[] _arguments;
        private string _comObject;
        private SwitchParameter _strict;
        private string _typeName;
        private Type[] _typeArguments = new Type[0];

        // Properties
        [Parameter(ParameterSetName = "Net", Mandatory = true, Position = 0)]
        public string TypeName
        {
            get { return this._typeName; }
            set { this._typeName = value; }
        }

        [Parameter(ParameterSetName = "Net", Mandatory = false, Position = 1)]
        public object[] ArgumentList
        {
            get { return this._arguments; }
            set { this._arguments = value; }
        }

        [Alias("Of")]
        [Parameter(ParameterSetName = "Net", Mandatory = false, Position = 2)]
        public Type[] TypeArgument
        {
            get { return _typeArguments; }
            set { _typeArguments = value; }
        }

        [Parameter(ParameterSetName = "Com", Mandatory = true, Position = 0)]
        public string ComObject
        {
            get { return this._comObject; }
            set { this._comObject = value; }
        }

        [Parameter(ParameterSetName = "Com")]
        public SwitchParameter Strict
        {
            get { return this._strict; }
            set { this._strict = value; }
        }

        protected override void BeginProcessing()
        {            
            base.BeginProcessing();

            string warning = String.Format(Properties.Resources.DeprecatedCmdlet_F2, CmdletName, "New-Object");
            WriteWarning(warning);

            object instance = null;

            switch (base.ParameterSetName)
            {
                case "Net":
                    instance = CreateManagedInstance();
                    break;

                case "Com":
                    instance = CreateUnmanagedInstance();
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            WriteObject(instance);
        }

        private object CreateManagedInstance()
        {
            int typeArgsCount = _typeArguments.Length;
            string typeName = this._typeName;
            Type type = null;
            object instance = null;

            if (typeArgsCount > 0)
            {
                typeName = GetGenericTypeScriptDefinition(this._typeName, typeArgsCount, ref type);
            }
            else
            {
                LanguagePrimitives.TryConvertTo<Type>(typeName, out type);
            }
            WriteVerbose("TypeName: " + typeName);

            if (type == null)
            {
                // invalid type name                        
                ThrowTerminatingError(
                    new ErrorRecord(
                        new ArgumentOutOfRangeException(
                            "TypeName", "Invalid TypeName."),
                        "InvalidTypeName", ErrorCategory.InvalidArgument, typeName)
                    );
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                instance = CreateDelegate(type);
            }
            else
            {
                WriteDebug("Delegating to built-in New-Object.");
                instance = InvokeNativeNewObject(typeName);
            }
            return instance;
        }

        private Delegate CreateDelegate(Type type)
        {
            if (type.IsAbstract)
            {
                PscxArgumentOutOfRangeException.Throw("Type {0} is abstract.", type.Name);
            }
            // need a target for delegate
            PscxArgumentException.ThrowIf(this._arguments.Length == 0,
                                          "Please supply a ScriptBlock as an argument.");

            // must be a scriptblock
            ScriptBlock target = this._arguments[0] as ScriptBlock;
            PscxArgumentException.ThrowIfIsNull(target, "The only argument in ArgumentList must be a ScriptBlock.");

            Delegate invoker = GetScriptBlockDelegate(target, type);

            return invoker;
        }

        private string GetGenericTypeScriptDefinition(string typeName, int typeArgsCount, ref Type closedType)
        {
            // TODO: account for generic type arrays, e.g. collection<string>[]
            string openTypeName = String.Format("{0}`{1}", typeName, typeArgsCount);

            Type genericType = null;
            if (!LanguagePrimitives.TryConvertTo<Type>(openTypeName, out genericType))
            {
                PscxArgumentException.Throw("Generic Type {0} not found.", openTypeName);
            }

            string closedTypeName = CreateClosedTypeName(openTypeName);

            if (!LanguagePrimitives.TryConvertTo<Type>(closedTypeName, out closedType))
            {
                PscxArgumentException.Throw("Could not construct Closed Type using the provided arguments.");
            }

            WriteDebug("AssemblyQualifiedName: " + closedType.AssemblyQualifiedName);
            WriteVerbose("TypeName: " + closedTypeName);

            return closedTypeName;
        }

        private object InvokeNativeNewObject(string typeName)
        {
            Command cmd = new Command(@"Microsoft.PowerShell.Utility\New-Object", false);

            if (base.ParameterSetName == "Net")
            {
                WriteDebug("ParameterSet Net");
                cmd.Parameters.Add("TypeName", typeName);
                cmd.Parameters.Add("ArgumentList", _arguments);
            }
            else
            {
                WriteDebug("ParameterSet COM");
                cmd.Parameters.Add("ComObject", typeName);
                if (this.Strict)
                {
                    cmd.Parameters.Add("Strict");
                }
            }

            PSObject instance = null;

            instance = PipelineHelper.ExecuteScalar<PSObject>(
                pipeline => pipeline.Commands.Add(cmd));
            
            return instance;
        }

        // [.collection``1[[system.string]][]] => collection<string>[]
        // [collections.objectmodel.collection``1[[system.string]]]
        // [collections.generic.dictionary``2[[system.string],[system.int32]]]
        private string CreateClosedTypeName(string openTypeName)
        {
            StringBuilder builder = new StringBuilder(openTypeName);
            builder.Append("[");
            foreach (Type typeArgument in this._typeArguments)
            {
                builder.AppendFormat("[{0}],", typeArgument.FullName);
            }
            builder.Remove(builder.Length - 1, 1); // trim last comma
            builder.Append("]");

            string closedTypeName = builder.ToString();

            return closedTypeName;
        }

        // handle COM
        private object CreateUnmanagedInstance()
        {
            return InvokeNativeNewObject(this._comObject);
        }

        // this took blood, sweat and tears AND my first born.
        private static Delegate GetScriptBlockDelegate(ScriptBlock block, Type delegateType)
        {
            PscxArgumentException.ThrowIfIsNull(block, "block");
            PscxArgumentException.ThrowIfIsNull(delegateType, "block");

            bool isDelegate = typeof (Delegate).IsAssignableFrom(delegateType);
            PscxArgumentException.ThrowIf((!isDelegate) || (delegateType.IsAbstract),
                                          "Invalid delegateType: {0}", delegateType.Name);

            MethodInfo invoke = delegateType.GetMethod("Invoke");
            Debug.Assert(invoke != null, "delegate invoke != null");

            ParameterInfo[] parameters = invoke.GetParameters();
            Type returnType = invoke.ReturnParameter.ParameterType;

            List<Type> args = new List<Type>();
            args.Add(typeof(ScriptBlock)); // first argument is instance
            foreach (ParameterInfo parameter in parameters)
            {
                args.Add(parameter.ParameterType);
            }

            DynamicMethod method = new DynamicMethod(String.Empty, returnType, args.ToArray(),
                    typeof(ScriptBlock).Module);

            ILGenerator ilgen = method.GetILGenerator();

            LocalBuilder[] locals = new LocalBuilder[2]
                {
                    ilgen.DeclareLocal(typeof (object[]), true),
                    ilgen.DeclareLocal(typeof (object[]), true)
                };
            
            ilgen.Emit(OpCodes.Ldc_I4, parameters.Length);
            ilgen.Emit(OpCodes.Newarr, typeof(object));
            ilgen.Emit(OpCodes.Stloc, 1);

            for (int index = 1; index < args.Count; index++)
            {
                ilgen.Emit(OpCodes.Ldloc, 1);
                EmitFastPushInt(ilgen, (index - 1)); // e.g. Ldc_I4_1
                ilgen.Emit(OpCodes.Ldarg, index);
                if (args[index].IsValueType)
                {
                    ilgen.Emit(OpCodes.Box, args[index]);
                }
                ilgen.Emit(OpCodes.Stelem_Ref);
            }
            ilgen.Emit(OpCodes.Ldloc_1);
            ilgen.Emit(OpCodes.Stloc_0);

            ilgen.Emit(OpCodes.Ldarg_0); // this
            ilgen.Emit(OpCodes.Ldloc_0); // object[] args for script block 
            ilgen.EmitCall(OpCodes.Callvirt, typeof(ScriptBlock).GetMethod("InvokeReturnAsIs"), null);

            if (invoke.ReturnType == typeof(void))
            {
                ilgen.Emit(OpCodes.Pop);
            }
            else
            {
                // need to convert return type to the target delegate's return type
                // ...
            }

            ilgen.Emit(OpCodes.Ret);

            return method.CreateDelegate(delegateType, block);
        }

        private static void EmitFastPushInt(ILGenerator ilgen, int value)
        {
            // emit the optimum opcode for lesser ints
            switch (value)
            {
                case -1:
                    ilgen.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    ilgen.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    ilgen.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    ilgen.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    ilgen.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    ilgen.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    ilgen.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    ilgen.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    ilgen.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    ilgen.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            // for bigger values emit the short or long opcode
            if (value > -129 && value < 128)
            {
                ilgen.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                ilgen.Emit(OpCodes.Ldc_I4, value);
            }
        }

 #if DEBUG
        public object InstanceMethod(string arg1, string arg2, int arg3)
        {
            object[] args = new object[] { arg1, arg2, arg3 };
            return this.InvokeWithArgs(args);
        }

        public object InstanceMethod(string arg1, string arg2)
        {
            object[] args = new object[] { arg1, arg2 };
            return this.InvokeWithArgs(args);
        }

        public void InstanceMethod(string arg)
        {
            object[] args = new object[] {arg};
            this.InvokeWithArgs(args);
        }

        public void InstanceMethod(string arg, int arg2)
        {
            object[] args = new object[] { arg, arg2 };
            this.InvokeWithArgs(args);
        }

        public object InvokeWithArgs(params object[] parameters)
        {
            return new int[] {1, 2, 3};
        }
#endif
    }
}

