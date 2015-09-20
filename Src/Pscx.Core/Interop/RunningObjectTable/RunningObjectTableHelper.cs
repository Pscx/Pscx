using System;
using System.Collections;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using JetBrains.Annotations;

namespace Pscx.Interop.RunningObjectTable
{
    public static class RunningObjectTableHelper
    {
        #region Public Methods

        /// <summary>
        /// Converts a COM class ID into a prog id.
        /// </summary>
        /// <param name="progID">The prog id to convert to a class id.</param>
        /// <returns>Returns the matching class id or the prog id if it wasn't found.</returns>
        public static string ConvertProgIdToClassId(string progID)
        {
            Guid testGuid;
            try
            {
                 NativeMethods.CLSIDFromProgIDEx(progID, out testGuid);
            }
            catch
            {
                try
                {
                    NativeMethods.CLSIDFromProgID(progID, out testGuid);
                }
                catch
                {
                    return progID;
                }
            }
            return testGuid.ToString().ToUpper();
        }

        /// <summary>
        /// Converts a COM class ID into a prog id.
        /// </summary>
        /// <param name="classId">The class id to convert to a prog id.</param>
        /// <returns>Returns the matching class id or null if it wasn't found.</returns>
        public static string ConvertClassIdToProgId(string classId)
        {
            Guid testGuid = new Guid(classId.Replace("!", ""));
            string progId = null;
            int result = 0;
            try
            {
                result = NativeMethods.ProgIDFromCLSID(ref testGuid, out progId);

                // class factory not found
                if ((result == -2147221164 /*0x80040154*/) && PscxContext.Instance.Is64BitProcess)
                {
                    throw new ArgumentException("Could not retrieve ProgID for Class Id: " + classId + 
                        ". Try running under the 32 bit PowerShell console.");
                }
            }
            catch (Exception)
            {
                if (result == -2147221164 /*0x80040154*/)
                {
                    // rethrow
                    throw;
                }
                return null;
            }
            return progId;
        }

        /// <summary>
        /// Get a snapshot of the running object table (ROT).
        /// </summary>
        /// <param name="filter">The filter to apply to the list (nullable).</param>
        /// <returns>A hashtable of the matching entries in the ROT</returns>
        public static Hashtable GetActiveObjectList(string filter)
        {
            var result = new Hashtable();

            IntPtr numFetched = IntPtr.Zero;
            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            var monikers = new IMoniker[1];

            NativeMethods.GetRunningObjectTable(0, out runningObjectTable);
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                NativeMethods.CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                object runningObjectVal;
                runningObjectTable.GetObject(monikers[0], out runningObjectVal);
                if (string.IsNullOrEmpty(filter) || runningObjectName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    result[runningObjectName] = runningObjectVal;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a COM object from the ROT, given a prog or class Id.
        /// </summary>
        /// <param name="progOrClassId">The prog or class id of the object to return.</param>
        /// <returns>The requested object, or null if the object is not found.</returns>
        public static object GetActiveObject(string progOrClassId)
        {
            if (String.IsNullOrEmpty(progOrClassId)) {
                throw new ArgumentException("progOrClassId is null or empty.");
            }

            string classId = progOrClassId;

            if ((progOrClassId.IndexOf("{") == -1) && (!progOrClassId.StartsWith("clsid:"))) {
                // Convert the prog id into a class id
                classId = ConvertProgIdToClassId(progOrClassId);

                if (classId == null) {
                    throw new ArgumentException(
                        String.Format("Unable to translate '{0}' to a class Id.", progOrClassId), "progOrClassId");
                }
            }
            else {
            }

            IRunningObjectTable prot = null;
            IEnumMoniker pMonkEnum = null;

            try
            {
                // Open the running objects table.
                NativeMethods.GetRunningObjectTable(0, out prot);
                prot.EnumRunning(out pMonkEnum);
                pMonkEnum.Reset();

                IMoniker[] pmon = new IMoniker[1];

                // Iterate through the results
                IntPtr fetched = IntPtr.Zero;

                while (pMonkEnum.Next(1, pmon, fetched) == 0)
                {
                    IBindCtx pCtx = null;
                    string displayName;
                    try
                    {
                        NativeMethods.CreateBindCtx(0, out pCtx);
                        pmon[0].GetDisplayName(pCtx, null, out displayName);
                    }
                    finally
                    {
                        if (pCtx != null)
                        {
                            Marshal.ReleaseComObject(pCtx);
                        }
                    }

                    if (displayName != null)
                    {
                        if (displayName.IndexOf(classId) != -1)
                        {
                            // Return the matching object
                            object objReturnObject;
                            prot.GetObject(pmon[0], out objReturnObject);
                            return objReturnObject;
                        }
                    }
                }
                return null;
            }
            finally
            {
                if (prot != null)
                {
                    Marshal.ReleaseComObject(prot);
                }
                if (pMonkEnum != null)
                {
                    Marshal.ReleaseComObject(pMonkEnum);
                }
            }
        }

        #endregion
    }
}
