//---------------------------------------------------------------------
// Author: Oisin Grehan
//
// Description: Static helper class for working with nested pipelines.
//              Uses c# 3.0 compiler features.
//
// Example:
//
//          * lambda style
//
//          int answer = PipelineHelper.ExecuteScalar<int>(
//              pipeline => pipeline.Commands.AddScript("2 * 4"));
//
//          * command style
//
//          Collection<PSObject> = PipelineHelper.Execute(
//              new Command("get-childitem"),
//              new CommandArgument() { Name="Path", Value="." }, 
//              new CommandArgument() { Name="Force", Value=true }
//          );
//
// Creation Date: December 1, 2007
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation.Runspaces;

using Pscx.Commands;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections;

namespace Pscx
{
    /// <summary>
    /// 
    /// </summary>
    public static class PipelineHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipe"></param>
        public delegate void PipelineAction(Pipeline pipe);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static Collection<PSObject> Execute(Command command, params CommandArgument[] arguments)
        {
            return Execute<Collection<PSObject>>(command, arguments);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptContents"></param>
        /// <returns></returns>
        public static T Execute<T>(string scriptContents)
        {
            return Execute<T>(pipeline => pipeline.Commands.AddScript(scriptContents));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static T Execute<T>(Command command, params CommandArgument[] arguments)
        {
            return ExecuteInternal<T>(command, false, arguments);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptContents"></param>
        /// <returns></returns>
        public static T ExecuteScalar<T>(string scriptContents)
        {
            return ExecuteScalar<T>(pipeline => pipeline.Commands.AddScript(scriptContents));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static T ExecuteScalar<T>(Command command, params CommandArgument[] arguments)
        {
            return ExecuteInternal<T>(command, true, arguments);
        }

        private static T ExecuteInternal<T>(Command command, bool scalar, params CommandArgument[] arguments)
        {
            foreach (var argument in arguments)
            {
                // parametername, value
                command.Parameters.Add(argument.Name, argument.Value);
            }

            using (Pipeline pipe = Runspace.DefaultRunspace.CreateNestedPipeline())
            {
                pipe.Commands.Add(command);
                Collection<PSObject> results = pipe.Invoke();

                object returnValue = results;
                if (scalar)
                {
                    returnValue = results[0];
                }

                return (T)LanguagePrimitives.ConvertTo(returnValue, typeof(T));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public static T Execute<T>(PipelineAction code)
        {
            return ExecuteInternal<T>(code, false);
        }

        public static T ExecuteScalar<T>(PipelineAction code)
        {
            return ExecuteInternal<T>(code, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        private static T ExecuteInternal<T>(PipelineAction code, bool scalar)
        {
            using (Pipeline pipe = Runspace.DefaultRunspace.CreateNestedPipeline())
            {
                code.Invoke(pipe);
                Collection<PSObject> results = pipe.Invoke();

                object returnValue = results;

                if (scalar)
                {
                    // even a null result ends up in a collection of length 1.
                    returnValue = results[0];
                }

                return (T)LanguagePrimitives.ConvertTo(returnValue, typeof(T));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct CommandArgument
    {
        public string Name;
        public object Value;
    }
}
