//---------------------------------------------------------------------
// Author: Keith Hill
//
// Description: Class to implement the Send-SmtpMail cmdlet.
//
// Creation Date: Nov 1, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Mail;
using System.Text;
using Pscx.IO;

namespace Pscx.Commands.Net
{
    [OutputType(typeof(MailMessage))]
    [Cmdlet(VerbsCommunications.Send, PscxNouns.SmtpMail, 
            DefaultParameterSetName = "Authenticated", SupportsShouldProcess = true)]
    [Obsolete(@"The PSCX\Send-SmtpMail cmdlet is obsolete and will be removed in the next version of PSCX. Use the built-in Microsoft.PowerShell.Utility\Send-MailMessage cmdlet instead.")]
    public class SendSmtpMailCommand : PscxCmdlet
    {
        private const string SmtpHostPref = "SmtpHost";
        private const string SmtpPortPref = "SmtpPort";
        private const string SmtpFromPref = "SmtpFrom";

        private const int DefaultSmtpPort = 25;

        private PSObject _inputObject;
        List<PSObject> _objects;
        private PSCredential _credentials;
        private StringBuilder _bodyStrBld;
        private SmtpClient _smtpClient;
        private string _host;
        private string _subject;
        private string _body;
        private string _from;
        private string _replyTo;
        private string[] _to;
        private string[] _cc = new string[0];
        private string[] _bcc = new string[0];
        private PscxPathInfo[] _attachmentPaths = new PscxPathInfo[0];
        private List<string> _inputPath;
        private int _timeout = 100 * 1000;
        private int? _portNumber;
        private SwitchParameter _isBodyHtml;
        private SwitchParameter _anonymous;
        private SwitchParameter _passThru;
        private MailPriority _priority = MailPriority.Normal;
        private int? _width;

        #region Parameters

        [Parameter(ValueFromPipeline = true)]
        [AllowNull]
        [AllowEmptyString]
        public PSObject InputObject
        {
            get { return _inputObject; }
            set { _inputObject = value; }
        }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [PreferenceVariable(SmtpHostPref)]
        public string SmtpHost
        {
            get { return _host; }
            set { _host = value; }
        }

        [Parameter]
        [PreferenceVariable(SmtpPortPref, DefaultSmtpPort)]
        public int? PortNumber
        {
            get { return _portNumber; }
            set { _portNumber = value; }
        }

        [Parameter(ParameterSetName = "Authenticated")]
        [Credential]
        [ValidateNotNull]
        public PSCredential Credential
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        [Parameter(ParameterSetName = "Anonymous")]
        public SwitchParameter Anonymous
        {
            get { return _anonymous; }
            set { _anonymous = value; }
        }

        [Parameter]
        public SwitchParameter PassThru
        {
            get { return _passThru; }
            set { _passThru = value; }
        }

        [Parameter]
        [ValidateNotNullOrEmpty]
        [PreferenceVariable(SmtpFromPref)]
        public string From
        {
            get { return _from; }
            set { _from = value; }
        }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string ReplyTo
        {
            get { return _replyTo; }
            set { _replyTo = value; }
        }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] To
        {
            get { return _to; }
            set { _to = value; }
        }

        [Parameter]
        [ValidateNotNull]
        public string[] Cc
        {
            get { return _cc; }
            set { _cc = value; }
        }

        [Parameter]
        [ValidateNotNull]
        public string[] Bcc
        {
            get { return _bcc; }
            set { _bcc = value; }
        }

        [Parameter]
        public string Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        [Parameter]
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        [Parameter, PscxPath]
        [ValidateNotNull]
        public PscxPathInfo[] AttachmentPath
        {
            get { return _attachmentPaths; }
            set { _attachmentPaths = value; }
        }

        [Parameter, PscxPath(NoGlobbing = true)]
        [ValidateNotNull]
        public PscxPathInfo[] AttachmentLiteralPath
        {
            get { return _attachmentPaths; }
            set
            {
                _attachmentPaths = value;
            }
        }

        [Parameter()]
        [ValidateRange(0, Int32.MaxValue)]
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        [Parameter()]
        public SwitchParameter HtmlBody
        {
            get { return _isBodyHtml; }
            set { _isBodyHtml = value; }
        }

        [Parameter()]
        public MailPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        [ValidateRange(2, Int32.MaxValue)]
        [Parameter()]
        public int? Width
        {
            get { return _width; }
            set { _width = value; }
        }

        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            WriteWarning(Properties.Resources.SmtpMailDeprecationWarning);

            _bodyStrBld = new StringBuilder();
            _inputPath = new List<string>();
            _objects = new List<PSObject>();

            //_resolvedPaths = GetResolvedPaths(_attachmentPaths, _literalPathUsed);

            _smtpClient = new SmtpClient(_host);
            _smtpClient.Timeout = _timeout;
            _smtpClient.Port = _portNumber.Value;

            if (_anonymous)
            {
                _smtpClient.Credentials = null;
                _smtpClient.UseDefaultCredentials = false;
            }
            else if (_credentials != null)
            {
                _smtpClient.Credentials = _credentials.GetNetworkCredential();
            }
            else
            {
                _smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
        }

        protected override void ProcessRecord()
        {
            if ((_inputObject == null) || (_inputObject.BaseObject == null)) return;

            object input = _inputObject.BaseObject;
            if (input is FileInfo)
            {
                FileInfo fileInfo = (FileInfo)input;
                _inputPath.Add(fileInfo.FullName);
            }
            else if (input is string)
            {
                string line = ((string)input).TrimEnd(null);
                _bodyStrBld.AppendLine(line);
            }
            else
            {
                // Save any other objects for formatting into text during EndProcessing
                _objects.Add(_inputObject);
            }
        }

        protected override void EndProcessing()
        {
            MailMessage message = null;

            try
            {
                message = new MailMessage();
                message.From = new MailAddress(_from);
                message.IsBodyHtml = _isBodyHtml;
                message.Priority = _priority;

                if (!String.IsNullOrEmpty(_replyTo))
                {
                    message.ReplyToList.Add(new MailAddress(_replyTo));
                }

                if (!String.IsNullOrEmpty(_subject))
                {
                    message.Subject = _subject;
                }

                if (!String.IsNullOrEmpty(_body))
                {
                    message.Body = _body;
                }

                if (_bodyStrBld.Length > 0)
                {
                    message.Body += _bodyStrBld.ToString().TrimEnd(null);
                }

                if (_objects.Count > 0)
                {
                    message.Body += RenderInputObjectsToText();
                }

                foreach (string recipient in _to)
                {
                    message.To.Add(recipient);
                }

                foreach (string recipient in _cc)
                {
                    message.CC.Add(recipient);
                }

                foreach (string recipient in _bcc)
                {
                    message.Bcc.Add(recipient);
                }

                foreach (PscxPathInfo attachmentPath in _attachmentPaths)
                {
                    message.Attachments.Add(new Attachment(attachmentPath.ProviderPath));
                }

                foreach (string attachment in _inputPath)
                {
                    message.Attachments.Add(new Attachment(attachment));
                }

                SendMessage(message);
            }
            finally
            {
                if (message != null) 
                {
                    message.Dispose();
                }
                _bodyStrBld = null;
                _objects = null;
                _inputPath = null;
            }
        }

        private void SendMessage(MailMessage message)
        {
            try
            {
                if (ShouldProcess(message.ToString()))
                {
                    _smtpClient.Send(message);
                    WriteVerbose(Properties.Resources.SmtpMailSent);

                    if (_passThru)
                    {
                        WriteObject(message);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                this.ErrorHandler.WriteInvalidOperationError(_smtpClient, ex);
            }
            catch (SmtpException ex)
            {
                this.ErrorHandler.WriteSmtpSendMessageError(_smtpClient, ex);
            }
        }

        private string RenderInputObjectsToText()
        {
            StringBuilder formattedOutput = new StringBuilder();

            // Format any other object type using out-string
            string cmd = String.Empty;
            if (_width.HasValue)
            {
                cmd = String.Format(CultureInfo.InvariantCulture, "$input | out-string -width {0}", _width.Value);
            }
            else
            {
                cmd = "$input | out-string";
            }

            Collection<PSObject> results = InvokeCommand.InvokeScript(cmd, false, PipelineResultTypes.None, _objects);
            foreach (PSObject obj in results)
            {
                string text = obj.ToString();
                string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    formattedOutput.AppendLine(line.TrimEnd(null));
                }
            }

            return formattedOutput.ToString().TrimEnd(null);
        }
    }
}
