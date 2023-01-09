//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: PscxPathInfo implementation for common resolved/literal
//              paths instance.
//
// Creation Date: September 12, 2007
//---------------------------------------------------------------------

using Pscx.Core.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Reflection;
using Pscx.Visitors;

using Wintellect.PowerCollections;

namespace Pscx.Commands
{
    public partial class PscxCmdlet
    {
        private List<Pair<PropertyInfo, PscxPathAttribute>> _boundPaths;
        private ProviderConstraintAttribute _defaultProviderConstraint = null;
        private ProviderConstraintPolicy _defaultPolicy = ProviderConstraintPolicy.EnforceOne;
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="literalPaths"></param>
        /// <returns></returns>
        protected PscxPathInfo[] GetPscxPathInfos(string[] paths, bool literalPaths)
        {
            PscxPathInfo[] pscxPaths = PscxPathInfo.GetPscxPathInfos(this.SessionState, paths, literalPaths);

            return pscxPaths;
        }

        /// <summary>
        /// Allow derived Cmdlets to override conditions of PscxPath validation at runtime.
        /// <remarks>See PscxPathCommandBase for example.</remarks>
        /// </summary>
        /// <param name="parameterName">The parameter to be validated.</param>
        /// <param name="settings">Represents the compile-time PscxPath attribute's settings.</param>
        /// <returns>True if parameter should be validated against PscxPath constraints, else false to not validate this parameter.</returns>
        protected virtual bool OnValidatePscxPath(string parameterName, IPscxPathSettings settings)
        {
            // yes, validate this parameter using the attribute constraints
            return true;
        }

        private void ValidatePscxPaths()
        {
            _boundPaths = new List<Pair<PropertyInfo, PscxPathAttribute>>();

            var visitor = new Visitor(this);
            visitor.VisitType(GetType());

            foreach (var boundPath in _boundPaths)
            {
                PropertyInfo parameter = boundPath.First;

                string parameterName = parameter.Name;
                
                // retrieve [PscxPath] attribute
                PscxPathAttribute pathAttrib = boundPath.Second;
                
                // get property value
                object value = parameter.GetValue(this, null);

                if ((value != null) &&
                    // allow runtime modification from derived classes
                    OnValidatePscxPath(parameterName, pathAttrib))
                {                    
                    WriteDebug("Validating " + parameterName);
                    PscxPathInfo[] pscxPaths = null;

                    // may be array
                    if (value is Array)
                    {
                        pscxPaths = value as PscxPathInfo[];
                    }
                    else
                    {
                        var pscxPath = value as PscxPathInfo;
                        if (pscxPath != null)
                        {
                            pscxPaths = new PscxPathInfo[] { pscxPath };
                        }
                    }

                    if (pscxPaths != null)
                    {
                        foreach (PscxPathInfo pscxPath in pscxPaths)
                        {
                            ValidatePscxPath(parameterName, pathAttrib, pscxPath);
                        }
                    }
                }
                else
                {
                    WriteDebug("Skipping " + parameterName);
                }
            }
        }

        private void ValidatePscxPath(string parameterName, PscxPathAttribute pathAttrib, PscxPathInfo pscxPath)
        {
            WriteDebug(String.Format("ValidatePscxPath: parameter {0} ; pscxPath {1}", parameterName, pscxPath));

            if (pscxPath.IsValid == false)
            {
                // todo: localize
                string description = String.Format(
                    "The path '{0}' supplied for {1} is invalid.",
                    pscxPath, parameterName);

                // path syntax error
                OnPscxPathError(parameterName, description, PscxPathState.Invalid, pscxPath);
            }

            PscxPathType pathType = PscxPathType.Unknown;

            // explicit true or unset
            if (pathAttrib.ShouldExist || (pathAttrib._shouldExist == null))
            {                
                if (pathAttrib.ShouldExist)
                {
                    // explicit true, so check existance
                    ValidateExists(parameterName, pscxPath, ref pathType);
                }
                else
                {
                    // shouldexist not specified, so grab path type via invokeprovider
                    pathType = this.InvokeProvider.Item.IsContainer(pscxPath.ToString()) ? PscxPathType.Container : PscxPathType.Leaf;
                }

                PscxPathType expectedPathType = pathAttrib.PathType;

                // do we have a path type constraint?
                if (expectedPathType != PscxPathType.None)
                {
                    ValidatePathType(parameterName, pscxPath, pathType, expectedPathType);
                }
            }
            else // shouldexist explicit false
            {
                switch (pathAttrib.PathType)
                {
                    case PscxPathType.LeafOrContainer:
                    case PscxPathType.Container:
                    case PscxPathType.Leaf:
                        //the should exist flag was explicitly set to false with a concrete path type - validate the provided path does not exist as the specified type
                        WriteVerbose(String.Format("Asserting path '{0}' of type {1} does not exist", pscxPath, pathAttrib.PathType));
                        ValidateNotExists(parameterName, pscxPath, pathAttrib.PathType);
                        break;
                    case PscxPathType.None:
                        //warn the user that argument path is marked as should not exist - it's responsibility of the subclass to handle the situation, since the should exist flag was explicitly set to false
                        WriteVerbose(String.Format("Path '{0}' of type {1} should not exist - not checked", pscxPath, pathAttrib.PathType));
                        break;
                    default:
                // NOTE: for Pscx developers
                Trace.Assert(pathAttrib.PathType == PscxPathType.None,
                    String.Format(
                             "Pscx Developer Error: {0}\n\nIf a PathType constraint is placed on a parameter, " +
                             "ShouldExist cannot be explicitly set to false. Remove " +
                             "the ShouldExist condition from the attribute, or set it " +
                             "explicitly to true.\n\nIf you are seeing this message " +
                             "as a Pscx end-user, please log an issue on " +
                             "https://github.com/danluca/Pscx/issues", CmdletName));
                        break;
                 }
            }

            // any provider constraints defined on the PscxPath attribute,
            // or on a Type-level ProviderConstraint attribute?
            if ((pathAttrib.ProviderTypes != null) || (_defaultProviderConstraint != null))
            {
                CheckProviderConstraints(pscxPath, pathAttrib, parameterName);
            }
        }

        private void ValidateExists(string parameterName, PscxPathInfo pscxPath, ref PscxPathType pathType)
        {
            WriteDebug("ValidateExists");

            bool exists = PscxPathInfo.Exists(pscxPath, ref pathType);

            if (!exists)
            {
                // TODO: localize
                string description = String.Format("The path '{0}' supplied for {1} must exist.", pscxPath, parameterName);

                // terminates by default (unless overridden)
                OnPscxPathError(parameterName, description, PscxPathState.NotExist, pscxPath);
            }
        }

        private void ValidateNotExists(string parameterName, PscxPathInfo pscxPath, PscxPathType pathType = PscxPathType.None) {
            WriteDebug("ValidateNotExists");

            PscxPathType foundPathType = PscxPathType.Unknown;
            bool exists = PscxPathInfo.Exists(pscxPath, ref foundPathType);

            if (exists && (pathType == PscxPathType.Unknown || pathType == PscxPathType.None)) {
                // TODO: localize
                string description = String.Format("The path '{0}' supplied for {1} must NOT exist.", pscxPath, parameterName);

                // terminates by default (unless overridden)
                OnPscxPathError(parameterName, description, PscxPathState.Invalid, pscxPath);
            }

            if (exists && ((foundPathType & pathType) == foundPathType)) {
                // TODO: localize
                string description = String.Format("The path '{0}' supplied for {1} must NOT exist as {2}.", pscxPath, parameterName, pathType);

                // terminates by default (unless overridden)
                OnPscxPathError(parameterName, description, PscxPathState.Invalid, pscxPath);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="pathAttrib"></param>
        /// <param name="pscxPath"></param>
        protected void CheckProviderConstraints(PscxPathInfo pscxPath, PscxPathAttribute pathAttrib, string parameterName)
        {
            WriteDebug("CheckProviderConstraints");

            // get cmdlet-level policy.
            ProviderConstraintPolicy effectivePolicy = this._defaultPolicy;

            if (pathAttrib != null && (pathAttrib.ConstraintPolicy != ProviderConstraintPolicy.Default))
            {
                effectivePolicy = pathAttrib.ConstraintPolicy;
            }

            var violations = new Collection<Pair<PscxPathInfo, Type>>();
            var constraints = new List<Type>();

            if (_defaultProviderConstraint != null)
            {
                constraints.AddRange(_defaultProviderConstraint.ProviderTypes);
            }

            if (pathAttrib != null && (pathAttrib.ProviderTypes != null))
            {
                constraints.AddRange(pathAttrib.ProviderTypes);
            }

            // TODO: localize
            string description = String.Format(
                "The path '{0}' supplied for {1} is not contained in a provider compatible with {2}.",
                pscxPath, parameterName, this.CmdletName);

            bool constrained = false;

            Type providerType = pscxPath.Provider.ImplementingType;

            foreach (Type typeConstraint in constraints)
            {
                if (!typeConstraint.IsAssignableFrom(providerType))
                {
                    constrained = true;

                    if (effectivePolicy == ProviderConstraintPolicy.EnforceAll)
                    {
                        // enforcing all, so notify now.

                        // write verbose reason for fail
                        WriteVerbose(String.Format("The constraint check for the interface or base class '{0}' against the provider '{1}' failed.", typeConstraint,
                                                   pscxPath.Provider.Name));

                        // terminating error
                        OnPscxPathError(parameterName, description,
                                        PscxPathState.ProviderConstraintFailure, pscxPath);
                    }
                    else
                    {
                        // enforcing only one; a subsequent check may pass.
                        violations.Add(new Pair<PscxPathInfo, Type>(pscxPath, typeConstraint));
                    }
                }
                else
                {
                    constrained = false;

                    if (effectivePolicy == ProviderConstraintPolicy.EnforceOne)
                    {
                        // we passed at least one, so stop checking.
                        break;
                    }
                }
            }

            // if all checks failed (and enforcing all), then fail.
            if (constrained && (effectivePolicy == ProviderConstraintPolicy.EnforceOne))
            {
                foreach (Pair<PscxPathInfo, Type> violation in violations)
                {
                    // write out verbose reason for failure.
                    WriteVerbose(String.Format("The constraint check for the interface or base class '{0}' against the provider '{1}' failed.", violation.Second,
                                               pscxPath.Provider.Name));
                }

                // terminating error
                OnPscxPathError(parameterName, description, PscxPathState.ProviderConstraintFailure, pscxPath);
            }
        }

        private void ValidatePathType(string parameterName, PscxPathInfo pscxPath, PscxPathType pathType, PscxPathType expectedPathType)
        {
            WriteDebug("ValidatePathType: expecting " + pathType);

            if ((pathType & expectedPathType) != pathType)
            {
                // TODO: localize
                string description = String.Format("Path type is {0}; expecting {1}", pathType, expectedPathType);

                // path type violation (terminating)
                OnPscxPathError(parameterName, description, PscxPathState.InvalidPathType, pscxPath);
            }
        }

        /// <summary>
        /// Terminating error.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="description"></param>
        /// <param name="reason"></param>
        /// <param name="pscxPath"></param>
        protected virtual void OnPscxPathError(string parameterName, string description, PscxPathState reason, PscxPathInfo pscxPath)
        {
            string errorMessage = String.Format("{0}'s {1} parameter has an invalid path of '{2}': {3}",
                this.CmdletName, parameterName, pscxPath.SourcePath, description);

            var exception = new PSArgumentException(errorMessage, parameterName);

            this.ThrowTerminatingError(
                new ErrorRecord(exception, reason.ToString(), ErrorCategory.InvalidArgument, parameterName));
        }

        private class Visitor : CmdletReflectionVisitor
        {
            private readonly PscxCmdlet _cmdlet;

            internal Visitor(PscxCmdlet cmdlet)
            {
                _cmdlet = cmdlet;
            }

            public override void VisitMemberAttribute(object attribute)
            {
                if (attribute is PscxPathAttribute)
                {
                    var _boundPath = new Pair<PropertyInfo, PscxPathAttribute>();
                    _boundPath.First = CurrentProperty;
                    _boundPath.Second = attribute as PscxPathAttribute;
                    _cmdlet._boundPaths.Add(_boundPath);
                }
                base.VisitMemberAttribute(attribute);
            }

            public override void VisitTypeAttribute(object attribute)
            {
                if (attribute is ProviderConstraintAttribute)
                {
                    _cmdlet._defaultProviderConstraint = attribute as ProviderConstraintAttribute;
                    _cmdlet._defaultPolicy = _cmdlet._defaultProviderConstraint.ConstraintPolicy;
                }
                base.VisitTypeAttribute(attribute);
            }
        }
    }
}
