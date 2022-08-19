using Antlr4.Runtime;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenEpl.TextECode.Internal
{
    internal class LoggerAntlrErrorListener<Symbol> : IAntlrErrorListener<Symbol>
    {
        public LoggerAntlrErrorListener(ILogger logger, string fileId)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FileId = fileId ?? throw new ArgumentNullException(nameof(fileId));
        }

        public ILogger Logger { get; }
        public string FileId { get; }

        public virtual void SyntaxError(TextWriter output, IRecognizer recognizer, Symbol offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Logger.LogError(e , "{FileId} [{Line}:{CharPositionInLine}]: (SyntaxError) {msg}", FileId, line, charPositionInLine, msg);
        }
    }
}
