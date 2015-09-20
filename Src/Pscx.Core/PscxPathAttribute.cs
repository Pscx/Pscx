//---------------------------------------------------------------------
// Authors: Oisin Grehan
//
// Description: Runtime attribute for Pscx Cmdlet
//
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using JetBrains.Annotations;
using Pscx.Commands;
using Pscx.IO;

namespace Pscx
{
    /// <summary>
    /// Intercept and transform incoming parameter string(s) to the target field/property to PscxPathInfo(s) or fully-qualified PSPath string(s).
    /// The default transform is to PscxPathInfo(s).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class PscxPathAttribute : ArgumentTransformationAttribute, IPscxPathSettings
    {
        private bool _noGlobbing;
        private bool _asString;
        private readonly Type[] _providerTypes;
        private PscxPathType _pathType = PscxPathType.None;
        private ProviderConstraintPolicy _constraintPolicy = ProviderConstraintPolicy.Default;

        // null = not set
        internal bool? _shouldExist;

        /// <summary>
        /// Default settings of NoGlobbing = false, ShouldExist = true, No provider constraints, no PathType constraint.
        /// </summary>
        public PscxPathAttribute() {}

        /// <summary>
        /// Constructor.
        /// <remarks>Having an paramarray arg in the ctor breaks CLS compliance.</remarks>
        /// </summary>
        /// <param name="providerTypes">
        /// A list of one or more provider (or interface) types to constrain this parameter's path to.
        /// Use the ContraintPolicy property to control whether one or all constraints must be satisfied.
        /// <remarks>
        /// Example: typeof(FileSystemProvider) or typeof(IPropertyCmdletProvider).
        /// </remarks>
        /// </param>
        public PscxPathAttribute(params Type[] providerTypes)
        {
            _providerTypes = providerTypes;
        }

        public string Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Bound path(s) should be treated as literal.
        /// </summary>
        public bool NoGlobbing
        {
            get { return _noGlobbing; }
            set { _noGlobbing = value; }
        }

        /// <summary>
        /// Bound path(s) must exist, or if a wildcard source must resolve to at least one item. Defaults to false.
        /// </summary>
        public bool ShouldExist
        {
            get
            {
                // return false if not explicitly set
                return _shouldExist ?? false;
            }
            set { _shouldExist = value; }
        }

        /// <summary>
        /// Bound path should not exist.
        /// </summary>
        public bool NoClobber {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Bound path(s) should be transformed into a string (the default is into a PscxPathInfo).
        /// </summary>
        public bool AsPSPath
        {
            get { return _asString; }
            set { _asString = value; }
        }

        /// <summary>
        /// If one or more ProviderType constraints have been suppllied, this property controls whether just one or all constraints must be satisfied.
        /// </summary>
        public ProviderConstraintPolicy ConstraintPolicy
        {
            get { return _constraintPolicy; }
            set { _constraintPolicy = value; }
        }

        /// <summary>
        /// A list of one or more provider (or interface) types to constrain this parameter's path to.
        /// Use the ContraintPolicy property to control whether one or all constraints must be satisfied.
        /// <remarks>
        /// Example: typeof(FileSystemProvider) or typeof(IPropertyCmdletProvider).
        /// </remarks>
        /// </summary>
        public Type[] ProviderTypes
        {
            get { return _providerTypes; }
            //set { _providerTypes = value; }
        }

        /// <summary>
        /// Enforce the target path to be a leaf, container or either.
        /// <remarks>
        /// This implies ShouldExist = true if NoGlobbing is true, so will throw if ShouldExist is set to false.
        /// PscxPathType.Unknown is not a valid setting and will cause a run-time error.
        /// </remarks>
        /// </summary>
        public PscxPathType PathType
        {
            get { return _pathType; }
            set { _pathType = value; }
        }

        [StringFormatMethod("format")]
        private void Dump(string format, params object[] parameters)
        {
            Debug.WriteLine(String.Format(format, parameters), Tag);
        }

        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            // need to avoid transform if input object has a PSPath
            // as we want powershell's binder to grab the property
            // via valuefrompipelinebypropertyname. if we convert object
            // now to string, then powershell will bind to path via pipeline
            // instead of binding to literalpath by propertyname

            EnsureValidProperties();

            object outputData = inputData;
            
            // not sure if this is needed, perhaps Assert more appropriate?
            if (inputData != null)
            {
                // need to save ETS properties for later tests
                object originalInputData = inputData;

                inputData = Utils.UnwrapPSObject(inputData);

                Type inputType = inputData.GetType();

                Dump("inputData type: {0} value: {1};", inputType.Name, inputData);

                if (inputType.IsArray)
                {
                    // glob (Path)
                    var psPaths = new List<string>();
                    foreach (object element in ((IEnumerable)inputData))
                    {
                        PSObject ps = PSObject.AsPSObject(element);
                        if (ps.Properties.Match("PSPath").Count == 1)
                        {
                            // prevent trying to glob a fileinfo or directoryinfo with wildcard characters in the name
                            // reasoning: if it has a PSPath property, it's a singular instance
                            _noGlobbing = true;

                            psPaths.Add((string)ps.Properties["PSPath"].Value);
                        }
                    }
                    if (psPaths.Count > 0)
                    {
                        inputData = psPaths.ToArray();
                    }

                    // string array?
                    if (Array.TrueForAll((object[])inputData, e => e is string))
                    {
                        string[] result = Array.ConvertAll((object[]) inputData, e => (string) e);
                        outputData = TransformInternal(engineIntrinsics, result, true);
                    }                    
                }
                else
                {
                    PSObject ps = PSObject.AsPSObject(originalInputData);
                    if (ps.Properties.Match("PSPath").Count == 1)
                    {
                        // prevent trying to glob a fileinfo or directoryinfo with wildcard characters in the name
                        // reasoning: if it has a PSPath property, it's a singular instance
                        _noGlobbing = true;

                        // do not attempt to transform
                        inputData = (string)ps.Properties["PSPath"].Value;
                    }
                    else if (ps.Properties.Match("Path").Count == 1)
                    {
                        inputData = (string)ps.Properties["Path"].Value;
                    }

                    // plain string?

                    //string result = inputData as string;
                    if (inputData is string)
                        //if (LanguagePrimitives.TryConvertTo<string>(inputData, out result))
                    {
                        outputData = TransformInternal(engineIntrinsics, new [] { (string)inputData}, false);
                    }
                }
            }
            return outputData;
        }

        // TODO: convert these errors to Asserts aimed at developer;
        // include text for end-user to report the errors to CodePlex.
        private void EnsureValidProperties()
        {           
            // Unknown is a return value only 
            if ((PathType == PscxPathType.Unknown) ||
                (!Enum.IsDefined(typeof(PscxPathType), PathType)))
            {
                // TODO: localise
                throw new ArgumentOutOfRangeException("PathType",
                    "PscxPathAttribute initialization error: PathType can only be Leaf, " +
                    "Container or LeafOrContainer. The default value is None.");
            }

            if (PathType != PscxPathType.None)
            {
                if (ShouldExist == false)
                {
                    // TODO: localise
                    throw new ArgumentException(
                        "When enforcing a PathType, ShouldExist must not be explicitly set to false as it is impossible to know the PathType of a non-existant path.", "ShouldExist");
                }
            }
        }

        private object TransformInternal(EngineIntrinsics engineIntrinsics, string[] boundPaths, bool inputIsPathArray)
        {
            object outputData;
            SessionState session = engineIntrinsics.SessionState;
            
            PscxPathInfo[] pscxPaths = PscxPathInfo.GetPscxPathInfos(session, boundPaths, _noGlobbing);

            // NOTE: this should never happen - verify
            if (pscxPaths.Length == 0)
            {
                Trace.WriteLine("TransformInternal: path count is zero.");
                return pscxPaths;
            }

            if (_asString)
            {
                // String

                var paths = new List<string>(pscxPaths.Length);
                Array.ForEach(pscxPaths, pscxPath => paths.Add(pscxPath.ToString()));

                if (inputIsPathArray || (!_noGlobbing))
                {
                    outputData = paths;
                }
                else
                {
                    // only literal can return a scalar
                    outputData = paths[0];
                }
            }
            else
            {
                // PscxPathInfo

                if (inputIsPathArray || (!_noGlobbing))
                {
                    outputData = pscxPaths;
                }
                else
                {
                    // only literal can return a scalar
                    outputData = pscxPaths[0];
                }
            }

            return outputData;
        }
    }
}