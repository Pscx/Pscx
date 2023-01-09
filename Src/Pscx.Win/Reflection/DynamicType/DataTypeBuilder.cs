//---------------------------------------------------------------------
// Author: Harley Green
//
// Description: Class to create dynamic types
//
// Creation Date: 2008/3/8
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Wintellect.PowerCollections;
using PropertyAttributes=System.Reflection.PropertyAttributes;

namespace Pscx.Win.Reflection.DynamicType
{
    public class DataTypeBuilder
    {
        private readonly ModuleBuilder _moduleBuilder;
        private static int _createdTypes;
        private static readonly object _createdTypesLock = new();

        public DataTypeBuilder(string name)
        {
            var assemblyName = new AssemblyName {Name = name};
            _moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(name);
        }

        public Type CreateType(IEnumerable<Pair<string, Type>> properties)
        {
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(GetDynamicName(), TypeAttributes.Public);
            
            AddProperties(typeBuilder,  properties);
            Type type = typeBuilder.CreateType();
            return type;
        }

        private void AddProperties(TypeBuilder builder, IEnumerable<Pair<string, Type>> properties)
        {
            foreach (var pair in properties)
            {
                AddProperty(builder, pair.First, pair.Second);

            }
        }

        private void AddProperty(TypeBuilder builder, string propertyName, Type propertyType)
        {
            string fieldName = String.Format("_{0}", propertyName);
            Type type = propertyType;
            if (type.IsValueType)
            {
                type = GetNullableType(type);
            }
            FieldBuilder fieldBuilder = builder.DefineField(fieldName, type, FieldAttributes.Private);
            PropertyBuilder propertyBuilder =
                builder.DefineProperty(propertyName, PropertyAttributes.None, type,
                                       Type.EmptyTypes);

            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName |
                                                MethodAttributes.HideBySig;

            MethodBuilder getPropertyMethodBuilder =
                builder.DefineMethod(
                    String.Format("get_{0}", propertyName),
                    getSetAttr,
                    type,
                    Type.EmptyTypes);

            ILGenerator propertyNameGetIL = getPropertyMethodBuilder.GetILGenerator();

            propertyNameGetIL.Emit(OpCodes.Ldarg_0);
            propertyNameGetIL.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyNameGetIL.Emit(OpCodes.Ret);

            MethodBuilder custNameSetPropMthdBldr =
                builder.DefineMethod(
                    String.Format("set_{0}", propertyName),
                    getSetAttr,
                    null,
                    new[] {type});

            ILGenerator propertyNameSetIL = custNameSetPropMthdBldr.GetILGenerator();

            propertyNameSetIL.Emit(OpCodes.Ldarg_0);
            propertyNameSetIL.Emit(OpCodes.Ldarg_1);
            propertyNameSetIL.Emit(OpCodes.Stfld, fieldBuilder);
            propertyNameSetIL.Emit(OpCodes.Ret);


            propertyBuilder.SetGetMethod(getPropertyMethodBuilder);
            propertyBuilder.SetSetMethod(custNameSetPropMthdBldr);
        }

        private Type GetNullableType(Type underlying)
        {
            Type nullable = typeof (Nullable<>);
            Type result = nullable.MakeGenericType(underlying);
            return result;
        }

        private static string GetDynamicName()
        {
            lock (_createdTypesLock)
            {
                return String.Format("PscxDynamicType{0}", _createdTypes++);
            }
        }
    }
}