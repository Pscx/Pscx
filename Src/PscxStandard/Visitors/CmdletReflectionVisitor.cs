//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class for processing cmdlet/parameter hierarchy.
//
// Creation Date: Dec 23, 2006
//---------------------------------------------------------------------
using System;
using System.Management.Automation;
using System.Reflection;

namespace Pscx.Visitors
{
    public abstract class CmdletReflectionVisitor : ReflectionVisitor
    {
        CmdletAttribute currentCmdlet;
        ParameterAttribute currentParameter;

        public sealed override void VisitType(Type type)
        {
            currentCmdlet = GetAttribute<CmdletAttribute>(type, false);
            
            if (currentCmdlet != null)
            {
                try
                {
                    VisitCmdlet(currentCmdlet);
                    base.VisitType(type);
                }
                finally
                {
                    currentCmdlet = null;
                }
            }
        }

        public sealed override void VisitMember(MemberInfo member)
        {
            currentParameter = GetAttribute<ParameterAttribute>(member, false);

            if (currentParameter != null)
            {
                try
                {
                    VisitParameter(currentParameter);
                    base.VisitMember(member);
                }
                finally
                {
                    currentParameter = null;
                }
            }
        }

        public virtual void VisitCmdlet(CmdletAttribute cmdlet)
        {
        }

        public virtual void VisitParameter(ParameterAttribute parameter)
        {
        }

        protected CmdletAttribute CurrentCmdlet
        {
            get { return currentCmdlet; }
        }

        protected ParameterAttribute CurrentParameter
        {
            get { return currentParameter; }
        }
    }
}
