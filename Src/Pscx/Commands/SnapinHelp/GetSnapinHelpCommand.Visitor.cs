//---------------------------------------------------------------------
// Author: jachymko, Keith Hill
//
// Description: Class which navigates over a cmdlet a reads all .NET 
//              metadata for help.
//
// Creation Date: Dec 23, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml;

using Pscx.Visitors;

namespace Pscx.Commands.SnapinHelp
{
    partial class GetSnapinHelpCommand 
    {
        class Visitor : CmdletReflectionVisitor
        {
            private readonly GetSnapinHelpCommand _command;

            private CmdletInfo _cmdletInfo;
            private ParameterInfo _parameterInfo;

            public Visitor(GetSnapinHelpCommand command)
            {
                _command = command;
            }

            public override void VisitCmdlet(CmdletAttribute cmdlet)
            {
                _cmdletInfo = new CmdletInfo(CurrentType, cmdlet);
                _command._cmdlets.Add(_cmdletInfo);

                base.VisitCmdlet(cmdlet);
            }

            public override void VisitTypeAttribute(object attribute)
            {
                RelatedLinkAttribute related = attribute as RelatedLinkAttribute;

                if (related != null)
                {
                    _cmdletInfo.RelatedLinks.Add(related.Text);
                }

                base.VisitTypeAttribute(attribute);
            }

            public override void VisitParameter(ParameterAttribute parameter)
            {
                _parameterInfo = new ParameterInfo(CurrentProperty, parameter);
                _cmdletInfo.AddParameter(_parameterInfo);

                base.VisitParameter(parameter);
            }

            public override void VisitMemberAttribute(object attribute)
            {
                AcceptsWildcardsAttribute aw = attribute as AcceptsWildcardsAttribute;
                if (aw != null)
                {
                    _parameterInfo.AcceptsWildcards = aw.Value;
                }

                base.VisitMemberAttribute(attribute);
            }
        }
    }
}
