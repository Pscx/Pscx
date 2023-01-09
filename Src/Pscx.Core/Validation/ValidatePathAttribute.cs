using Pscx.Core.IO;
using System;
using System.Management.Automation;

namespace Pscx.Validation
{
    /// <summary>
    /// INCOMPLETE
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    internal sealed class ValidatePathAttribute : ValidateArgumentsAttribute
    {
        private bool _noGlobbing;
        private bool? _shouldExist;
        private readonly Type[] _providerTypes;
        private PscxPathType _pathType = PscxPathType.None;

        /// <summary>
        /// Bound path(s) should be treated as literal and wildcards will not be expanded.
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
                // we need to know if it is defaulted or not
                // (see EnsureValidProperties)
                return _shouldExist ?? false;
            }
            set { _shouldExist = value; }
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
        public ValidatePathAttribute(params Type[] providerTypes)
        {
            _providerTypes = providerTypes;
        }

        #region Overrides of ValidateArgumentsAttribute

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            EnsureValidProperties();

            // not sure if this is needed, perhaps Assert more appropriate?
            if (arguments != null)
            {
                Type inputType = arguments.GetType();

                // TODO: if we were more powershelly here, would would try to coerce args
                // to string or string[] ourselves using langprimitives

                if ((inputType == typeof(string[])) ||
                    inputType == typeof(string))
                {
                    if (inputType.IsArray)
                    {
                        string[] result;
                        if (LanguagePrimitives.TryConvertTo<string[]>(arguments, out result))
                        {
                            ValidateInternal(engineIntrinsics, result);
                        }
                    }
                    else
                    {
                        string result;
                        if (LanguagePrimitives.TryConvertTo<string>(arguments, out result))
                        {
                            ValidateInternal(engineIntrinsics, new string[] {result});
                        }
                    }
                }
                else
                {
                    throw new ValidationMetadataException("Argument must be of type string or string[] array.");
                }
            }
        }

        private void ValidateInternal(EngineIntrinsics intrinsics, string[] result)
        {
            throw new NotImplementedException();
        }

        #endregion

        // TODO: convert these errors to Asserts aimed at developer; include text for end-user to report the errors to CodePlex.        
        private void EnsureValidProperties()
        {
            // Unknown is a return value only 
            if ((PathType == PscxPathType.Unknown) ||
                (!Enum.IsDefined(typeof(PscxPathType), PathType)))
            {
                // TODO: localise
                throw new ArgumentOutOfRangeException("PathType",
                    "ValidatePathAttribute initialization error: PathType can only be Leaf, " +
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
    }
}
