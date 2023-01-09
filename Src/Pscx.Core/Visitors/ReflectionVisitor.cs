//---------------------------------------------------------------------
// Author: jachymko
//
// Description: Base class for processing assembly/type/member hierarchy.
//
// Creation Date: Dec 23, 2006
//---------------------------------------------------------------------

using Pscx.Core;
using System;
using System.Reflection;

namespace Pscx.Visitors
{
    public abstract class ReflectionVisitor
    {
        Type currentType;
        MemberInfo currentMember;

        public virtual void VisitAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetExportedTypes())
            {
                try
                {
                    currentType = type;
                    VisitType(type);
                }
                finally
                {
                    currentType = null;
                }
            }
        }

        public virtual void VisitType(Type type)
        {
            foreach (MemberInfo member in type.GetMembers())
            {
                try
                {
                    currentMember = member;
                    VisitMember(member);
                }
                finally
                {
                    currentMember = null;
                }
            }

            foreach (object a in type.GetCustomAttributes(false))
            {
                VisitTypeAttribute(a);
            }
        }

        public virtual void VisitTypeAttribute(object attribute)
        {
        }

        public virtual void VisitMember(MemberInfo member)
        {
            foreach (object a in member.GetCustomAttributes(false))
            {
                VisitMemberAttribute(a);
            }

            PropertyInfo property = member as PropertyInfo;
            
            if (property != null)
            {
                VisitProperty(property);
            }
        }

        public virtual void VisitMemberAttribute(object attribute)
        {
        }

        public virtual void VisitProperty(PropertyInfo property)
        {
        }

        protected PropertyInfo CurrentProperty
        {
            get { return currentMember as PropertyInfo; }
        }

        protected Type CurrentType
        {
            get { return currentType; }
        }

        protected T GetAttribute<T>(ICustomAttributeProvider caholder, bool inherit) where T : Attribute
        {
            return Utils.GetAttribute<T>(caholder, inherit);
        }
    }
}
