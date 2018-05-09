//---------------------------------------------------------------------
// Author: Keith Hill, jachymko, Oisin Grehan
//
// Description: Base class for all commands.
//
// Creation Date: Dec 9, 2006
//
// Modified: September 12, 2007; Oisin Grehan
//           * added comments (TODOs) about provider-internal path issues
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;

namespace Pscx.Commands
{
    public abstract partial class PscxCmdlet : PSCmdlet, IDisposable
    {
        private Dictionary<Type, Delegate> _actions;
        private bool _hasInterfaceActions;
        private bool _recurseIEnumerable;
        private string _currentPath;
        private string _cmdletName;

        // FIXME: ugly
        protected bool _initialized;

        protected PscxCmdlet()
        {            
            _recurseIEnumerable = true;
        }

        public string CmdletName
        {
            get
            {                
                if (_cmdletName == null)
                {
                    CmdletAttribute cmdletAttr = Utils.GetAttribute<CmdletAttribute>(GetType());
                    if (cmdletAttr != null)
                    {
                        _cmdletName = cmdletAttr.VerbName + "-" + cmdletAttr.NounName;
                    }                    
                }
                return _cmdletName;
            }
        }

        public IPscxErrorHandler ErrorHandler
        {
            get { return this; }
        }

        public IPscxFileHandler FileHandler
        {
            get { return this; }
        }

        public void EnsureDependency(PscxDependency dependency)
        {
            dependency.Ensure(this);
        }

        public void ValidateEncoding(StringEncodingParameter encoding)
        {
            if (encoding.IsPresent)
            {
                if (encoding.ToEncoding() == null)
                {
                    ErrorHandler.ThrowUnknownEncoding(encoding.ToString());
                }
            }
        }

        protected bool ProcessIEnumerableRecursively
        {
            get { return _recurseIEnumerable; }
            set { _recurseIEnumerable = value; }
        }

        protected string CurrentInputObjectPath
        {
            get { return _currentPath; }
        }

        protected static PscxContext Context
        {
            get { return PscxContext.Instance; }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _initialized = true;

            Debug.WriteLine(String.Format("ParameterSetName: {0}", this.ParameterSetName), this.GetType().Name);
            
            LoadPreferences();                        
            ValidatePscxPaths();            
        }

        /// <summary>
        /// You shouldnt need to override this function in most cases. Just
        /// call RegisterInputType&lt;T&gt; with your delegate, and this method
        /// will take care of determining the input type and calling your delegate
        /// if appropriate.
        /// 
        /// Called when the InputParameter is specified or a pipeline object that isn't
        /// a string (those bind to Path or LiteralPath) is bound to this parameter.
        /// Note: This method will be called even when inputObject is null.
        /// </summary>
        /// <param name="inputObject"></param>
        protected virtual void ProcessInputObject(PSObject inputObject)
        {
            if (inputObject == null)
            {
                ProcessNullInput();
            }
            else
            {
                ProcessInputObjectInternal(inputObject);
            }
        }

        protected virtual void ProcessNullInput()
        {
        }

        /// <summary>
        /// Registers a delegate called when specified type of object
        /// is read from the pipeline.
        /// </summary>
        /// <remarks>
        /// You may register an interface type too, but they are processed only when
        /// there is no handler available for the actual type.
        /// </remarks>
        /// <example>
        /// <para>For example, if you register the following types, the IDisposable handler 
        /// will be executed only on objects which do not inherit from Stream.</para>
        /// <code>
        ///     RegisterInputType&lt;IDisposable&gt;(/* ... */);
        ///     RegisterInputType&lt;FileStream&gt;(/* ... */);
        ///     RegisterInputType&lt;Stream&gt;(/* ... */);
        /// </code>
        /// </example>
        /// <typeparam name="T">The type which should be processed. </typeparam>
        /// <param name="action">
        /// The delegate that is called when the object is available.
        /// </param>
        protected void RegisterInputType<T>(Action<T> action)
        {
            PscxArgumentException.ThrowIfIsNull(action,
                "To ignore an input type, use IgnoreInputType<T>().");
            PscxArgumentException.ThrowIf(typeof(T) == typeof(Object),
                "You cannot register an action for Object. Override the ProcessInputObject method instead.");
            PscxArgumentException.ThrowIf(typeof(T) == typeof(PSObject),
                "You cannot register an action for PSObject. Override the ProcessInputObject method instead.");

            if (_recurseIEnumerable)
            {
                PscxArgumentException.ThrowIf(typeof(T) == typeof(IEnumerable),
                    "You cannot register an action for IEnumerable when ProcessIEnumerableRecursively is true.");
            }

            SetTypeAction(typeof(T), action);
        }

        protected void IgnoreInputType<T>()
        {
            SetTypeAction(typeof(T), null);
        }

        private void SetTypeAction(Type type, Delegate action)
        {
            if (_actions == null)
            {
                _actions = new Dictionary<Type, Delegate>();
            }

            if (type.IsInterface)
            {
                _hasInterfaceActions = true;
            }

            _actions[type] = action;
        }

        private void ProcessInputObjectInternal(object target)
        {
            PSObject psobj = target as PSObject;
            if (psobj != null)
            {
                PSPropertyInfo pspath = psobj.Properties["PSPath"];

                if (pspath != null)
                {
                    _currentPath = pspath.Value as string;
                }

                try
                {
                    ProcessInputObjectInternal(psobj.BaseObject);
                }
                finally
                {
                    _currentPath = null;
                }

                return;
            }

            if (target == null)
            {
                ProcessNullInput();
                return;
            }

            if (_actions == null)
            {
                return;
            }

            Type type = target.GetType();

            do
            {
                if (TryInvokeAction(type, target))
                {
                    return;
                }

                type = type.BaseType;
            }
            while (type != null);

            if (_hasInterfaceActions)
            {
                foreach (Type interfaceType in target.GetType().GetInterfaces())
                {
                    if (TryInvokeAction(interfaceType, target))
                    {
                        return;
                    }
                }
            }

            IEnumerable enumerable = (target as IEnumerable);

            if (ProcessIEnumerableRecursively && enumerable != null)
            {
                foreach (object obj in enumerable)
                {
                    ProcessInputObjectInternal(obj);
                }
            }
            else
            {
                ErrorHandler.WriteInvalidInputError(_actions.Keys, target);
            }
        }

        private bool TryInvokeAction(Type type, object target)
        {
            if (_actions.ContainsKey(type))
            {
                Delegate action = _actions[type];

                if (action != null)
                {
                    try
                    {
                        action.DynamicInvoke(target);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw e.GetBaseException();
                    }
                }

                return true;
            }

            return false;
        }

        private void LoadPreferences()
        {
            foreach (PropertyInfo pi in GetType().GetProperties())
            {
                PreferenceVariableAttribute pref = Utils.GetAttribute<PreferenceVariableAttribute>(pi);
                if (pref == null) continue;

                object value = pi.GetValue(this, null);

                if (value == null)
                {
                    value = PscxContext.Instance.Preferences[pref.VariableName] ?? pref.DefaultValue;
                    // FIXME: Check ValidateRange, ValidateSet, etc...
                }

                if ((pi.PropertyType == typeof(SwitchParameter)) && !((SwitchParameter)(value)).IsPresent)
                {
                    value = PscxContext.Instance.Preferences[pref.VariableName];
                    if (value == null)
                    {
                        continue;
                    }

                    value = new SwitchParameter(LanguagePrimitives.IsTrue(value));
                }
                
                if (value == null && pref.DefaultValue == null)
                {
                    ErrorHandler.ThrowNoPreferenceVariable(pi.Name, pref.VariableName);
                }
                else if ((value != null) && !pi.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    string actualType = value.GetType().Name;
                    string expectedType = pi.PropertyType.Name;
                    if (pi.PropertyType.IsGenericType)
                    {
                        Type[] genericArgs = pi.PropertyType.GetGenericArguments();
                        if (genericArgs.Length == 1)
                        {
                            expectedType = genericArgs[0].Name;
                        }
                    }

                    ErrorHandler.ThrowIncompatiblePreferenceVariableType(pi.Name, expectedType, actualType);
                }
                else
                {
                    pi.SetValue(this, value, null);
                }
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the
        /// PscxCmdlet and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources. 
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Releases all resources used by the PsxCmdlet.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }

            // NOTE: for Pscx developers (only assert if pipeline is terminating normally)
            if (!Stopping)
            {
                // TODO: localize
                //Trace.Assert(this._initialized, String.Format(
                //    "Pscx Developer Error: {0}\n\nFailure to call " +
                //    "base members in BeginProcessing, ProcessRecord and/or EndProcessing " + 
                //    " override. If you are seeing this issue as a Pscx end-user, please " +
                //    "report an issue on http://www.codeplex.com/powershellcx/", CmdletName));
            }
            GC.KeepAlive(this._initialized);
        }
    }
}
