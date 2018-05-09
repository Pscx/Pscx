using Pscx.IO;

namespace Pscx.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class PscxInputObjectPathSettings
    {
        public PscxInputObjectPathSettings(bool fileAsPath, bool dirAsPath)
        {
            ProcessFileInfoAsPath = fileAsPath;
            ProcessDirectoryInfoAsPath = dirAsPath;
        }

        public PscxInputObjectPathSettings(bool fileAsPath, bool dirAsPath, bool constrainInputObject)
        {
            ProcessFileInfoAsPath = fileAsPath;
            ProcessDirectoryInfoAsPath = dirAsPath;
            ConstrainInputObjectByPSPath = constrainInputObject;
        }

        public bool ProcessFileInfoAsPath;
        public bool ProcessDirectoryInfoAsPath;
        public bool ConstrainInputObjectByPSPath;

        /// <summary>
        /// 
        /// </summary>
        public bool? LiteralPathShouldExist;

        /// <summary>
        /// 
        /// </summary>
        public PscxPathType LiteralPathPathType;

        /// <summary>
        /// 
        /// </summary>
        public bool? PathShouldExist;

        /// <summary>
        /// 
        /// </summary>
        public PscxPathType PathPathType;
    }
}