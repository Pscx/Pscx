using System;

using Pscx.IO;

namespace Pscx.Commands.IO.Compression.ArchiveWriter
{
	public interface IArchiveWriter : IDisposable
	{
		void BeginProcessing();
		void ProcessPath(PscxPathInfo pscxPath);
		void EndProcessing();
	    void StopProcessing();

		string DefaultExtension { get; }
	}
}
