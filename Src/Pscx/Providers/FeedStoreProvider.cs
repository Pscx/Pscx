using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Microsoft.Feeds.Interop;

namespace Pscx.Providers
{
    [CmdletProvider(PscxProviders.FeedStore, ProviderCapabilities.ShouldProcess)]
    public class FeedStoreProvider : NavigationCmdletProvider
    {
        private string _pathSeparator = Path.DirectorySeparatorChar.ToString();

        public static FeedsManager FeedsManager
        {
            get { return FeedsManagerSingleton.Instance; }
        }

        private class FeedsManagerSingleton
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static FeedsManagerSingleton()
            {
            }

            internal static readonly FeedsManager Instance = new FeedsManager();
        }

        #region Private methods
        
        /// <summary>
        /// Breaks up the path into individual elements.
        /// </summary>
        /// <param name="path">The path to split.</param>
        /// <returns>An array of path segments.</returns>
        private string[] ChunkPath(string path)
        {
            // Return the path with the drive name and first path 
            // separator character removed, split by the path separator.
            return path.Split(_pathSeparator.ToCharArray());
        }

        /// <summary>
        /// Adapts the path, making sure the correct path separator
        /// character is used.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string NormalizePath(string path)
        {
            if (String.IsNullOrEmpty(path)) return path;

            string result = path.Replace("/", _pathSeparator);

            if (result.StartsWith(_pathSeparator))
            {
                result = result.Substring(_pathSeparator.Length);
            }

            if (result.EndsWith(_pathSeparator))
            {
                result = result.Substring(0, result.Length - _pathSeparator.Length);
            }

            return result;
        }

        /// <summary>
        /// Checks if a given path is actually a drive name.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>
        /// True if the path given represents a drive, false otherwise.
        /// </returns>
        private bool PathIsDrive(string path)
        {
            return String.IsNullOrEmpty(path) || String.IsNullOrEmpty(path.Replace(_pathSeparator, ""));
        }

        #endregion

        #region Protected methods


        protected override Collection<PSDriveInfo>InitializeDefaultDrives()
        {
            Type t = Type.GetTypeFromProgID("Microsoft.FeedsManager", false);
            if (t == null) return null;

            // we only need to initialize a single default drive
            return new Collection<PSDriveInfo>(
                new PSDriveInfo[] {
                     new PSDriveInfo("Feed", this.ProviderInfo, "", "Microsoft Common Feed Store", this.Credential)
                 });
        }

        protected override bool IsValidPath(string path)
        {
            if (String.IsNullOrEmpty(path)) return true;

            // Converts all separators in the path to a uniform one.
            path = NormalizePath(path);

            // Splits the path into individual chunks.
            string[] pathChunks = path.Split(_pathSeparator.ToCharArray());

            foreach (string pathChunk in pathChunks)
            {
                if (pathChunk.Length == 0) return false;
            }
            return true;
        }

        protected override bool IsItemContainer(string path)
        {
            path = NormalizePath(path);

            if (PathIsDrive(path)) return true;

            return FeedsManager.ExistsFolder(path) || FeedsManager.ExistsFeed(path);
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            path = NormalizePath(path);

            if (PathIsDrive(path))
            {
                throw new ArgumentException("Path must be a a folder or feed.");
            }

            if (itemTypeName.ToLower().StartsWith("f"))
            {
                if (itemTypeName.Length == 1)
                {
                    WriteError(new ErrorRecord(new ArgumentException("Ambiguous type. Only \"folder\" or \"feed\" can be specified."),
                        "InvalidArgument", ErrorCategory.InvalidArgument, itemTypeName));
                }

                // The leaf of the path will contain the name of the item to create
                string[] pathChunks = ChunkPath(path);
                string parentPath = GetParentPath(path, "");

                // create a new folder. Let's check that we're in a folder and not a feed
                if (!FeedsManager.ExistsFolder(parentPath))
                {
                    WriteError(new ErrorRecord(new ArgumentException("Items can only be created within folders."),
                        "InvalidArgument", ErrorCategory.InvalidArgument, path));
                    return;
                }

                IFeedFolder folder = FeedsManager.GetFolder(parentPath) as IFeedFolder;
                string newItemName = pathChunks[pathChunks.Length - 1];

                //if (String.Compare("folder", 0, itemTypeName, 0, itemTypeName.Length, true) == 0)
                if (itemTypeName.Equals("folder", StringComparison.OrdinalIgnoreCase) ||
                    itemTypeName.Equals("directory", StringComparison.OrdinalIgnoreCase))
                {
                    IFeedFolder newFolder = folder.CreateSubfolder(newItemName) as IFeedFolder;
                    WriteItemObject(newFolder, newFolder.Path, true);
                    return;
                }

                if (String.Compare("feed", 0, itemTypeName, 0, itemTypeName.Length, true) == 0)
                {
                    if (newItemValue == null || !Uri.IsWellFormedUriString(newItemValue.ToString(), UriKind.Absolute))
                    {
                        WriteError(new ErrorRecord
                            (new ArgumentException("Value must be a valid feed URI."),
                               "InvalidArgument", ErrorCategory.InvalidArgument, newItemValue)
                              );
                        return;
                    }
                    IFeed newFeed = folder.CreateFeed(newItemName, newItemValue.ToString()) as IFeed;
                    WriteItemObject(newFeed, newFeed.Path, true);
                    return;
                }
            }

            WriteError(new ErrorRecord(new ArgumentException("The type is not a known type for the feed store. Only \"folder\" or \"feed\" can be specified."),
                   "InvalidArgument", ErrorCategory.InvalidArgument, itemTypeName));
            return;

        }

        protected override void MoveItem(string path, string destination)
        {
            path = NormalizePath(path);
            destination = NormalizePath(destination);

            if (!FeedsManager.ExistsFolder(destination))
            {
                WriteError(new ErrorRecord
                           (new ArgumentException("Target feed folder not found."),
                           "InvalidArgument", ErrorCategory.InvalidArgument, destination)
                          );
                return;
            }

            if (FeedsManager.ExistsFolder(path))
            {
                IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;
                if (ShouldProcess(folder.Path, "move"))
                {
                    folder.Move(destination);
                    WriteItemObject(folder, folder.Path, true);
                }
                return;
            }

            if (FeedsManager.ExistsFeed(path))
            {
                IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                if (ShouldProcess(feed.Path, "move"))
                {
                    feed.Move(destination);
                    WriteItemObject(feed, feed.Path, true);
                }
                return;
            }

            WriteError(new ErrorRecord
               (new ArgumentException("Item not found."),
               "InvalidArgument", ErrorCategory.InvalidArgument, path)
              );
            return;
        }

        protected override void RenameItem(string path, string newName)
        {
            if (newName.Contains(_pathSeparator))
            {
                WriteError(new ErrorRecord
                           (new ArgumentException("Cannot rename because the target specified is not a path."),
                           "InvalidArgument", ErrorCategory.InvalidArgument, newName)
                          );
                return;
            }

            path = NormalizePath(path);

            if (FeedsManager.ExistsFolder(path))
            {
                IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;

                if (ShouldProcess(folder.Path, "rename"))
                {
                    WriteDebug("Renaming folder " + folder.Path);
                    folder.Rename(newName);
                    WriteItemObject(folder, folder.Path, true);
                }
                return;
            }

            if (FeedsManager.ExistsFeed(path))
            {
                IFeed feed = FeedsManager.GetFeed(path) as IFeed;

                if (Force && ShouldProcess(feed.Path, "rename"))
                {
                    WriteDebug("Renaming feed " + feed.Path);
                    feed.Rename(newName);
                    WriteItemObject(feed, feed.Path, true);
                }
                return;
            }

            WriteError(new ErrorRecord
               (new NotImplementedException("Cannot rename because item specified."),
               "NotImplemented", ErrorCategory.NotImplemented, path)
              );
        } // RenameItem

        protected override void RemoveItem(string path, bool recurse)
        {
            path = NormalizePath(path);

            if (FeedsManager.ExistsFolder(path))
            {
                IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;
                if (ShouldProcess(path, "delete"))
                {
                    folder.Delete();
                }
                return;
            }

            if (FeedsManager.ExistsFeed(path))
            {
                IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                if (ShouldProcess(path, "delete"))
                {
                    feed.Delete();
                }
                return;
            }

            WriteError(new ErrorRecord
               (new ItemNotFoundException("Item not found."),
               "InvalidArgument", ErrorCategory.InvalidArgument, path)
              );
        }

        protected override bool ItemExists(string path)
        {
            path = NormalizePath(path);

            if (PathIsDrive(path)) return true;

            if (FeedsManager.ExistsFolder(path) || FeedsManager.ExistsFeed(path)) return true;

            string[] chunks = ChunkPath(path);
            if (chunks.Length > 0)
            {
                int id;
                if (int.TryParse(chunks[chunks.Length - 1], out id))
                {
                    path = GetParentPath(path, "");

                    if (FeedsManager.ExistsFeed(path))
                    {
                        IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                        IFeedItem feedItem = feed.GetItem(id) as IFeedItem;
                        return feedItem != null;
                    }
                }
            }
            return false;
        }

        protected override bool HasChildItems(string path)
        {
            path = NormalizePath(path);

            if (PathIsDrive(path))
            {
                IFeedFolder root = FeedsManager.RootFolder as IFeedFolder;
                IFeedsEnum subFolders = root.Subfolders as IFeedsEnum;
                if (subFolders.Count > 0) return true;

                IFeedsEnum feeds = root.Feeds as IFeedsEnum;
                if (feeds.Count > 0) return true;
            }
            else
            {
                if (FeedsManager.ExistsFolder(path))
                {
                    IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;
                    IFeedsEnum subFolders = folder.Subfolders as IFeedsEnum;
                    if (subFolders.Count > 0) return true;

                    IFeedsEnum feeds = folder.Feeds as IFeedsEnum;
                    if (feeds.Count > 0) return true;
                }

                if (FeedsManager.ExistsFeed(path))
                {
                    IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                    return feed.ItemCount > 0;
                }
            }
            return false;
        }

        protected override object GetChildItemsDynamicParameters(string path, bool recurse)
        {
            // We provide an "-unread" parameter so that the user can choose to only list 
            // unread items (and folders and feeds with unread items)
            WriteVerbose("GetChildItemsDynamicParameters:path = " + path);

            ParameterAttribute unreadAttrib = new ParameterAttribute();
            unreadAttrib.Mandatory = false;
            unreadAttrib.ValueFromPipeline = false;

            RuntimeDefinedParameter unreadParam = new RuntimeDefinedParameter();
            unreadParam.IsSet = false;
            unreadParam.Name = "Unread";
            unreadParam.ParameterType = typeof(SwitchParameter);
            unreadParam.Attributes.Add(unreadAttrib);

            RuntimeDefinedParameterDictionary dic = new RuntimeDefinedParameterDictionary();
            dic.Add("Unread", unreadParam);

            return dic;
        }

        protected override object GetChildNamesDynamicParameters(string path)
        {
            /* for now we'll just use the same parameters as gci */
            return GetChildItemsDynamicParameters(path, false);
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            path = NormalizePath(path);

            WriteDebug("Listing items in " + path);

            bool unreadOnly = false;
            RuntimeDefinedParameterDictionary dic = DynamicParameters as RuntimeDefinedParameterDictionary;
            if (dic != null && dic.ContainsKey("Unread"))
            {
                RuntimeDefinedParameter rdp = dic["Unread"];
                unreadOnly = rdp.IsSet;
            }

            WriteDebug(unreadOnly ? "Unread items only" : "All items");

            // Checks if the path represented is a drive. This means that the 
            // children of the path are folders and feeds
            if (PathIsDrive(path))
            {
                WriteDebug("Listing root folder folders");

                IFeedFolder folder = FeedsManager.RootFolder as IFeedFolder;
                IFeedsEnum subFolders = folder.Subfolders as IFeedsEnum;
                foreach (IFeedFolder subFolder in subFolders)
                {
                    if (!unreadOnly || subFolder.TotalUnreadItemCount > 0)
                    {
                        WriteItemObject(subFolder, subFolder.Path, true);
                        if (recurse) GetChildItems(subFolder.Path, true);
                    }
                }

                WriteDebug("Listing root folder feeds");

                IFeedsEnum feeds = folder.Feeds as IFeedsEnum;
                foreach (IFeed feed in feeds)
                {
                    if (!unreadOnly || feed.UnreadItemCount > 0)
                    {
                        WriteItemObject(feed, feed.Path, true);
                        if (recurse) GetChildItems(feed.Path, true);
                    }
                }

                return;
            }

            if (FeedsManager.ExistsFolder(path))
            {
                WriteDebug("Listing folders in " + path);

                IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;
                IFeedsEnum subFolders = folder.Subfolders as IFeedsEnum;
                foreach (IFeedFolder subFolder in subFolders)
                {
                    if (!unreadOnly || subFolder.TotalUnreadItemCount > 0)
                    {
                        WriteItemObject(subFolder, subFolder.Path, true);
                        if (recurse) GetChildItems(subFolder.Path, true);
                    }
                }

                WriteDebug("Listing feeds in " + path);

                IFeedsEnum feeds = folder.Feeds as IFeedsEnum;
                foreach (IFeed feed in feeds)
                {
                    if (!unreadOnly || feed.UnreadItemCount > 0)
                    {
                        WriteItemObject(feed, feed.Path, true);
                        if (recurse) GetChildItems(feed.Path, true);
                    }
                }
                return;
            }

            if (FeedsManager.ExistsFeed(path))
            {
                WriteDebug("Listing items in " + path);

                IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                IFeedsEnum feedItems = feed.Items as IFeedsEnum;
                foreach (IFeedItem feedItem in feedItems)
                {
                    if (!unreadOnly || !feedItem.IsRead)
                    {
                        WriteItemObject(feedItem, feed.Path + _pathSeparator + feedItem.LocalId, false);
                    }
                }
                return;
            }
        } // GetChildItems

        protected override string GetChildName(string path)
        {
            path = NormalizePath(path);

            WriteDebug("Getting name for " + path);

            // Checks if the path represented is a drive
            if (PathIsDrive(path))
            {
                return "";
            }// if (PathIsDrive...

            if (FeedsManager.ExistsFolder(path))
            {
                IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;
                return folder.Name;
            }

            if (FeedsManager.ExistsFeed(path))
            {
                IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                return feed.Name;
            }

            WriteDebug("Couldn't find drive, folder or feed - checking for item.");
            string[] chunks = ChunkPath(path);
            if (chunks.Length > 0)
            {
                WriteDebug("Chunks:");
                foreach (string chk in chunks) WriteDebug("chunk: " + chk);

                int id;
                if (int.TryParse(chunks[chunks.Length - 1], out id))
                {
                    path = GetParentPath(path, "");

                    WriteDebug("Looking for feed " + path);
                    if (FeedsManager.ExistsFeed(path))
                    {
                        WriteDebug("Found feed - looking for item");

                        IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                        IFeedItem feedItem = feed.GetItem(id) as IFeedItem;
                        if (feedItem != null)
                        {
                            WriteDebug("Found item - returning " + feedItem.LocalId);
                            return feedItem.LocalId.ToString();
                        }
                    }
                }
            }
            return base.GetChildName(path);
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            path = NormalizePath(path);

            WriteDebug("Listing names in " + path);

            bool unreadOnly = false;
            RuntimeDefinedParameterDictionary dic = DynamicParameters as RuntimeDefinedParameterDictionary;
            if (dic != null && dic.ContainsKey("Unread"))
            {
                RuntimeDefinedParameter rdp = dic["Unread"];
                unreadOnly = rdp.IsSet;
            }

            WriteDebug(unreadOnly ? "Unread items only" : "All items");
            // Checks if the path represented is a drive. This means that the 
            // children of the path are folders and feeds
            if (PathIsDrive(path))
            {
                WriteDebug("Listing root folder folder names");

                IFeedFolder folder = FeedsManager.RootFolder as IFeedFolder;
                IFeedsEnum subFolders = folder.Subfolders as IFeedsEnum;
                foreach (IFeedFolder subFolder in subFolders)
                {
                    if (!unreadOnly || subFolder.TotalUnreadItemCount > 0)
                    {
                        WriteItemObject(subFolder.Name, subFolder.Path, true);
                    }
                }

                WriteDebug("Listing root folder feed names");

                IFeedsEnum feeds = folder.Feeds as IFeedsEnum;
                foreach (IFeed feed in feeds)
                {
                    if (!unreadOnly || feed.UnreadItemCount > 0)
                    {
                        WriteItemObject(feed.Name, feed.Path, true);
                    }
                }

                return;
            }

            if (FeedsManager.ExistsFolder(path))
            {
                WriteDebug("Listing folder names in " + path);

                IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;
                IFeedsEnum subFolders = folder.Subfolders as IFeedsEnum;
                foreach (IFeedFolder subFolder in subFolders)
                {
                    if (!unreadOnly || subFolder.TotalUnreadItemCount > 0)
                    {
                        WriteItemObject(subFolder.Name, subFolder.Path, true);
                    }
                }

                WriteDebug("Listing feed names in " + path);

                IFeedsEnum feeds = folder.Feeds as IFeedsEnum;
                foreach (IFeed feed in feeds)
                {
                    if (!unreadOnly || feed.UnreadItemCount > 0)
                    {
                        WriteItemObject(feed.Name, feed.Path, true);
                    }
                }
                return;
            }

            if (FeedsManager.ExistsFeed(path))
            {
                WriteDebug("Listing names in " + path);

                IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                IFeedsEnum feedItems = feed.Items as IFeedsEnum;
                foreach (IFeedItem feedItem in feedItems)
                {
                    if (!unreadOnly || !feedItem.IsRead)
                    {
                        WriteItemObject(feedItem.LocalId.ToString(), feed.Path + _pathSeparator + feedItem.LocalId, false);
                    }
                }
                return;
            }
        } // GetChildNames

        protected override void GetItem(string path)
        {
            path = NormalizePath(path);

            // Checks if the path represented is a drive
            if (PathIsDrive(path))
            {
                WriteItemObject(FeedsManager.RootFolder, PSDriveInfo.Name + ':', true);
                return;
            }// if (PathIsDrive...

            if (FeedsManager.ExistsFolder(path))
            {
                IFeedFolder folder = FeedsManager.GetFolder(path) as IFeedFolder;
                WriteItemObject(folder, folder.Path, true);
                return;
            }

            if (FeedsManager.ExistsFeed(path))
            {
                IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                WriteItemObject(feed, feed.Path, true);
                return;
            }

            string[] chunks = ChunkPath(path);
            if (chunks.Length > 0)
            {
                int id;
                if (int.TryParse(chunks[chunks.Length - 1], out id))
                {
                    path = GetParentPath(path, "");

                    if (FeedsManager.ExistsFeed(path))
                    {
                        IFeed feed = FeedsManager.GetFeed(path) as IFeed;
                        IFeedItem feedItem = feed.GetItem(id) as IFeedItem;
                        if (feedItem != null)
                        {
                            WriteItemObject(feedItem, feed.Path + _pathSeparator + feedItem.LocalId, false);
                            return;
                        }
                    }
                }
            }

            base.GetItem(path);
        } // GetItem

        #endregion
    }
}
