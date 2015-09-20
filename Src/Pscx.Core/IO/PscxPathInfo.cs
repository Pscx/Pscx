//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: PscxPathInfo implementation for common resolved/literal
//              paths instance.
//
// Creation Date: September 12, 2007
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pscx.IO
{
    /// <summary>
    /// Represents a path in PowerShell.
    /// </summary>
    [XmlInclude(typeof(InvalidPscxPathImpl))]    
    [XmlInclude(typeof(ResolvedPscxPathImpl))]
    [XmlInclude(typeof(UnresolvedPscxPathImpl))]
    public abstract partial class PscxPathInfo : IXmlSerializable
    {
        private bool _isUnresolved;
        private string _providerPath;
        private string _sourcePath;
        private PscxPathState _state = PscxPathState.Resolved;
        private PSDriveInfo _driveInfo = null;
        private ProviderInfo _providerInfo = null;
        private PathInfo _pathInfo = null;
        
        private PscxPathInfo()
        {
        }

        #region Factory Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathInfo"></param>
        /// <returns></returns>
        public static PscxPathInfo FromPathInfo(PathInfo pathInfo)
        {
            return new ResolvedPscxPathImpl(pathInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="paths"></param>
        /// <param name="literalPaths"></param>
        /// <returns></returns>
        public static PscxPathInfo[] GetPscxPathInfos(SessionState session, string[] paths, bool literalPaths)
        {
            return GetPscxPathInfos(session, paths, literalPaths, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="paths"></param>
        /// <param name="literalPaths"></param>
        /// <param name="shouldExist"></param>
        /// <returns></returns>
        public static PscxPathInfo[] GetPscxPathInfos(SessionState session, string[] paths, bool literalPaths,
                                                      bool shouldExist)
        {
            var pathInfos = new List<PscxPathInfo>();

            if (paths != null && paths.Length > 0)
            {
                if (literalPaths)
                {
                    ProcessLiteralPaths(session, paths, pathInfos, shouldExist);
                }
                else
                {
                    ProcessResolvablePaths(session, paths, pathInfos, shouldExist);
                }
            }

            return pathInfos.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="literalPath"></param>
        /// <returns></returns>
        public static PscxPathInfo GetPscxPathInfo(SessionState session, string literalPath)
        {
            return GetPscxPathInfo(session, literalPath, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="literalPath"></param>
        /// <param name="shouldExist"></param>
        /// <returns></returns>
        public static PscxPathInfo GetPscxPathInfo(SessionState session, string literalPath, bool shouldExist)
        {
            PscxPathInfo[] pscxPaths = GetPscxPathInfos(session, new string[] {literalPath}, true, shouldExist);
            return pscxPaths[0];
        }

        private static void ProcessResolvablePaths(SessionState session, IEnumerable<string> paths,
                                                   ICollection<PscxPathInfo> pathInfos)
        {
            ProcessResolvablePaths(session, paths, pathInfos, false);
        }

        private static void ProcessResolvablePaths(SessionState session, IEnumerable<string> paths,
                                                   ICollection<PscxPathInfo> pathInfos, bool shouldExist)
        {
            // handle wildcard resolving etc
            foreach (string path in paths)
            {
                try
                {
                    Collection<PathInfo> resolvedPaths = session.Path.GetResolvedPSPathFromPSPath(path);

                    foreach (PathInfo resolvedPath in resolvedPaths)
                    {
                        // save resolved path along with original input path
                        var pscxPath = new ResolvedPscxPathImpl(resolvedPath, path);
                        pathInfos.Add(pscxPath);
                    }
                }
                catch (ItemNotFoundException ex)
                {
                    PscxPathInfo pscxPath;

                    // check if input contains wildcards or should exist = true
                    if (WildcardPattern.ContainsWildcardCharacters(ex.ItemName) || shouldExist)
                    {
                        // save qualifed/resolved failed path as invalid.
                        // must be invalid because an unresolved wildcard
                        // can never be a valid path.
                        pscxPath = new InvalidPscxPathImpl(ex.ItemName)
                                       {
                                           _state = PscxPathState.Unresolved
                                       };
                    }
                    else
                    {
                        // path is not wildcard but nonetheless does not exist.
                        // pass back as a literal path.
                        pscxPath = new UnresolvedPscxPathImpl(ex.ItemName, session)
                                       {
                                           _state = PscxPathState.NotExist
                                       };
                    }
                    pathInfos.Add(pscxPath);
                }
            }
        }

        private static void ProcessLiteralPaths(SessionState session, IEnumerable<string> paths,
                                                ICollection<PscxPathInfo> pathInfos)
        {
            ProcessLiteralPaths(session, paths, pathInfos, false);
        }

        private static void ProcessLiteralPaths(SessionState session, IEnumerable<string> paths,
                                                ICollection<PscxPathInfo> pathInfos, bool shouldExist)
        {
            // store as unresolved (literal)
            foreach (string path in paths)
            {
                PscxPathInfo pscxPath;

                // NOTE: IsValid only checks for syntactical validity, not existance
                if (session.Path.IsValid(path))
                {
                    // is existance required?
                    if (shouldExist)
                    {
                        if (Exists(path, true))
                        {
                            // valid
                            pscxPath = new UnresolvedPscxPathImpl(path, session);
                        }
                        else
                        {
                            // does not exist
                            pscxPath = new InvalidPscxPathImpl(path)
                                           {
                                               _state = PscxPathState.NotExist
                                           };
                        }
                    }
                    else
                    {
                        // valid
                        pscxPath = new UnresolvedPscxPathImpl(path, session);
                    }
                }
                else
                {
                    // invalid path
                    pscxPath = new InvalidPscxPathImpl(path)
                                   {
                                       _state = PscxPathState.InvalidSyntax
                                   };
                }
                pathInfos.Add(pscxPath);
            }
        }

        #endregion

        /// <summary>
        /// Gets the provider containing this path.
        /// </summary>
        public ProviderInfo Provider
        {
            get
            {
                EnsureValid();
                return _providerInfo;
            }
        }

        /// <summary>
        /// Gets the drive containing this path.
        /// </summary>
        public PSDriveInfo Drive
        {
            get
            {
                EnsureValid();
                return _driveInfo;
            }
        }

        /// <summary>
        /// Gets the provider-internal path for the path that this PscxPathInfo object represents.
        /// If this object represents an invalid path, this property returns the errant path.
        /// <seealso cref="IsValid">PscxPathInfo.IsValid</seealso>
        /// </summary>
        public string ProviderPath
        {
            get { return _providerPath; }
        }

        /// <summary>
        /// Returns true if this instance represents an unresolved (literal) path.
        /// </summary>
        public bool IsUnresolved
        {
            get { return _isUnresolved; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsValid
        {
            get
            {
                return (_providerPath != null) && // must have internal path
                       ((_providerInfo != null) || (_driveInfo != null)); // and either one of these
            }
        }

        ///// <summary>
        ///// If invalid, returns the reason. If valid, returns "valid."
        ///// </summary>
        //public PscxPathState State
        //{
        //    get { return _state; }
        //}

        /// <summary>
        /// The unresolved or literal path that is the source of this PscxPath instance.
        /// </summary>
        public string SourcePath
        {
            get { return _sourcePath; }
        }

        /// <summary>
        /// Returns a System.Management.Automation.PathInfo object representing the resolved path.
        /// </summary>
        /// <returns>A PathInfo object representing an existing path.</returns>
        /// <exception cref="System.InvalidOperationException">An InvalidOperationException is thrown if this instance represents a literal path.</exception>
        public PathInfo ToPathInfo()
        {
            EnsureValid();
            if (_pathInfo == null)
            {
                // TODO: localize
                throw new PSInvalidOperationException(
                    "This object represents a literal (unresolved) path. A PathInfo can only represent a resolved path.");
            }
            return _pathInfo;
        }

        /// <summary>
        /// Returns a fully-qualified string representation of the PowerShell path this object represents.
        /// </summary>
        /// <returns>System.String</returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void EnsureValid()
        {
            if (IsValid == false)
            {
                throw new PSInvalidOperationException("This operation is not valid for an instance of " + GetType().Name);
            }
        }

        #region Static Utility Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pscxPath"></param>
        /// <returns></returns>
        public static bool Exists(PscxPathInfo pscxPath)
        {
            string qualifiedPath = pscxPath.ToString();
            bool isLiteral = pscxPath.IsUnresolved;

            return Exists(qualifiedPath, isLiteral);
        }

        public static bool Exists(string path, bool isLiteral)
        {
            // need to check if we should resolve wildcards
            var pathArg = new CommandArgument
                              {
                                  Name = (isLiteral) ? "LiteralPath" : "Path",
                                  Value = path
                              };

            // best way to check paths in a static context
            return PipelineHelper.ExecuteScalar<bool>(
                new Command(@"Microsoft.PowerShell.Management\Test-Path"), pathArg);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pscxPath"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public static bool Exists(PscxPathInfo pscxPath, ref PscxPathType pathType)
        {
            string qualifiedPath = pscxPath.ToString();
            pathType = PscxPathType.Unknown;
            string pathArg = (pscxPath.IsUnresolved) ? "LiteralPath" : "Path";

            if (Exists(pscxPath))
            {
                bool isLeaf = PipelineHelper.ExecuteScalar<bool>(
                    new Command("Test-Path"),
                    new CommandArgument() {Name = pathArg, Value = qualifiedPath},
                    new CommandArgument() {Name = "PathType", Value = "Leaf"});

                if (isLeaf)
                {
                    pathType = PscxPathType.Leaf;
                }
                else
                {
                    bool isContainer = PipelineHelper.ExecuteScalar<bool>(
                        new Command("Test-Path"),
                        new CommandArgument() {Name = pathArg, Value = qualifiedPath},
                        new CommandArgument() {Name = "PathType", Value = "Container"});

                    if (isContainer)
                    {
                        pathType = PscxPathType.Container;
                    }
                }
                return true;
            }
            return false;
        }

        protected static string GetDriveQualifiedPath(string path, PSDriveInfo drive)
        {
            PscxArgumentException.ThrowIfIsNullOrEmpty(path);
            PscxArgumentException.ThrowIfIsNull(drive);

            string qualifiedPath = path;
            bool unqualified = true;

            int index = path.IndexOf(':');
            if (index != -1)
            {
                if (string.Equals(path.Substring(0, index), drive.Name, StringComparison.OrdinalIgnoreCase))
                {
                    unqualified = false;
                }
            }
            if (unqualified)
            {
                const char separator = '\\';
                string format = "{0}:" + separator + "{1}";

                if (path.StartsWith(separator.ToString(), StringComparison.Ordinal))
                {
                    format = "{0}:{1}";
                }

                // strip root
                if (!String.IsNullOrEmpty(drive.Root))
                {
                    if (path.StartsWith(drive.Root))
                    {
                        path = path.Substring(drive.Root.Length + 1); // grab trailing slash
                    }
                }

                qualifiedPath = string.Format(CultureInfo.InvariantCulture, format, drive.Name, path);
            }
            return qualifiedPath;
        }

        protected static string GetProviderQualifiedPath(string path, ProviderInfo provider)
        {
            PscxArgumentException.ThrowIfIsNullOrEmpty(path);
            PscxArgumentException.ThrowIfIsNull(provider);

            string qualifiedPath = path;
            bool isProviderQualified = false;
            int index = path.IndexOf("::", StringComparison.Ordinal);
            if (index != -1)
            {
                string providerName = path.Substring(0, index);
                if (CompareProviderNames(provider.Name, providerName) == true)
                {
                    isProviderQualified = true;
                }
            }
            if (!isProviderQualified)
            {
                qualifiedPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", GetProviderFullName(provider),
                                              "::", path);
            }
            return qualifiedPath;
        }


        protected static bool CompareProviderNames(string firstName, string secondName)
        {
            string[] chunks = secondName.Split('\\');
            if (chunks.Length == 1)
            {
                // not snapin-qualified (shortname)
                return firstName.Equals(secondName, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // snapin-qualified, e.g. vendor.namespace\provider
                return firstName.Equals(chunks[1], StringComparison.OrdinalIgnoreCase);
            }
        }

        protected static string GetProviderFullName(ProviderInfo provider)
        {
            PscxArgumentException.ThrowIfIsNull(provider);

            string name = provider.Name;
            if (provider.PSSnapIn != null)
            {
                string snapInName = provider.PSSnapIn.Name;
                if (!string.IsNullOrEmpty(snapInName))
                {
                    name = string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", snapInName, provider.Name);
                }
            }
            return name;
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. 
        ///                 </param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. 
        ///                 </param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            /*
                    private bool _isUnresolved;
                    private string _providerPath;
                    private string _sourcePath;
                    private PscxPathState _state = PscxPathState.Resolved;
                    private PSDriveInfo _driveInfo = null;
                    private ProviderInfo _providerInfo = null;
                    private PathInfo _pathInfo = null;
            */
            writer.WriteStartElement(GetType().Name);
            writer.WriteAttributeString("SourcePath", _sourcePath);
            writer.WriteAttributeString("ProviderPath", _providerPath);
            writer.WriteAttributeString("NoGlobbing", _isUnresolved.ToString());
            //writer.WriteStartElement
            //writer.WriteAttributeString("HairLength", Enum.GetName(typeof(HairLength), HairLength));
            //writer.WriteAttributeString("Key", Key.ToString());
            writer.WriteEndElement();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    [Serializable]
    public enum PscxPathType
    {
        None = 0,
        /// <summary>
        /// Return only. Do not use as a parameter value.
        /// </summary>
        Unknown = 1,
        Leaf = 2,
        Container = 4,
        LeafOrContainer = Leaf | Container
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    [Serializable]
    public enum PscxPathState
    {
        Valid = 0,
        Invalid = 1,
        Resolved = 2,
        Unresolved = 4,
        ProviderConstraintFailure = 8,
        NotExist = 16,
        InvalidSyntax = 32,
        InvalidPathType = 64
    }
}