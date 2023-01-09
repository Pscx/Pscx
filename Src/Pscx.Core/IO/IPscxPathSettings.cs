using Pscx.Commands;
using System;

namespace Pscx.Core.IO
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPscxPathSettings
    {
        /// <summary>
        /// Bound path(s) should be not have wildcards expanded.
        /// </summary>
        bool NoGlobbing { get; set; }

        /// <summary>
        /// Bound path(s) must exist, or if a wildcard source must resolve to at least one item. Defaults to false.
        /// </summary>
        bool ShouldExist { get; set; }

        /// <summary>
        /// If one or more ProviderType constraints have been supplied, this property controls whether just one or all constraints must be satisfied.
        /// </summary>
        ProviderConstraintPolicy ConstraintPolicy { get; set; }

        /// <summary>
        /// A list of one or more provider (or interface) types to constrain this parameter's path to.
        /// Use the ContraintPolicy property to control whether one or all constraints must be satisfied.
        /// <remarks>
        /// Example: typeof(FileSystemProvider) or typeof(IPropertyCmdletProvider).
        /// </remarks>
        /// </summary>
        Type[] ProviderTypes {
            get;
            //set { _providerTypes = value; }
        }

        /// <summary>
        /// Enforce the target path to be a leaf, container or either.
        /// <remarks>
        /// This implies ShouldExist = true if NoGlobbing is true, so will throw if ShouldExist is set to false.
        /// PscxPathType.Unknown is not a valid setting and will cause a run-time error.
        /// </remarks>
        /// </summary>
        PscxPathType PathType { get; set; }
    }
}