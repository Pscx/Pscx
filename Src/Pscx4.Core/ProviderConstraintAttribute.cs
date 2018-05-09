//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Runtime attribute for Pscx Cmdlet
//
//---------------------------------------------------------------------

using System;
using Pscx.Commands;

namespace Pscx
{
    /// <summary>
    /// Constrains this Cmdlet class to one or more provider(s). Any parameters decorated with PscxPath attributes will inherit this constraint. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ProviderConstraintAttribute : Attribute
    {
        private readonly Type[] _providerTypes;
        private ProviderConstraintPolicy _constraintPolicy = ProviderConstraintPolicy.EnforceOne;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerTypes"></param>
        public ProviderConstraintAttribute(params Type[] providerTypes)
        {
            // NOTE: a params ctor argument breaks CLS compliancy
            _providerTypes = providerTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        public Type[] ProviderTypes
        {
            get { return _providerTypes; }
            //set { _providerTypes = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ProviderConstraintPolicy ConstraintPolicy
        {
            get { return _constraintPolicy; }
            set { _constraintPolicy = value; }
        }
    }
}