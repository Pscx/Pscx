namespace Pscx.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public enum ProviderConstraintPolicy
    {
        /// <summary>
        /// The class-level constraint is used if it exists, else EnforceOne.
        /// </summary>
        Default,

        /// <summary>
        /// All provider constraints must pass.
        /// </summary>
        EnforceAll,

        /// <summary>
        /// Only one ProviderConstraint need pass.
        /// </summary>
        EnforceOne
    }
}