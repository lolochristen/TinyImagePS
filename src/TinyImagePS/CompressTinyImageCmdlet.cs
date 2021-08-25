using System;
using System.IO;
using System.Management.Automation;
using TinyImagePS.Models;

namespace TinyImagePS
{
    [Cmdlet(VerbsData.Compress, "TinyImage")]
    [OutputType(typeof(TinifyResponse))]
    public class CompressTinyImageCmdlet : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true
        )]
        public string[] Path { get; set; }

        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true
        )]
        public string DestinationPath { get; set; }

        [Parameter(
            HelpMessage = "ApiKey obtained from https://tinypng.com/developers",
            ValueFromPipelineByPropertyName = true
        )]
        public string ApiKey { get; set; }

        [Parameter] public SwitchParameter Force { get; set; }

        [Parameter] public SwitchParameter Replace { get; set; }

        protected override void ProcessRecord()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                try
                {
                    ApiKey = SetTinyImageApiKeyCmdlet.GetApiKeyFromFile();
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(new ErrorRecord(new ApplicationException("Api Key is not present.", e),
                        "NOAPIKEY", ErrorCategory.InvalidArgument, null));
                    return;
                }

            var tinify = new TinifyApi(ApiKey);

            foreach (var path in Path)
                try
                {
                    var task = tinify.Shrink(path);
                    var response = task.Result;

                    if (Replace)
                    {
                        DestinationPath = path;
                        Force = true;
                    }

                    if (string.IsNullOrEmpty(DestinationPath))
                    {
                        // to pipeline
                        WriteObject(response);
                    }
                    else
                    {
                        // download file directly...
                        if (File.Exists(DestinationPath) && Force == false)
                        {
                            WriteError(
                                new TinifyException($"File {DestinationPath} already exists.").CreateErrorRecord(
                                    "FileExists"));
                            continue;
                        }

                        // to file
                        tinify.DownloadFile(task.Result, DestinationPath).Wait();
                    }
                }
                catch (TinifyException exception)
                {
                    WriteError(exception.CreateErrorRecord("Exception"));
                }
                catch (Exception exception)
                {
                    WriteError(
                        new TinifyException("Exception: " + exception.Message, exception)
                            .CreateErrorRecord("Exception"));
                }
        }
    }

    public class TinifyException : Exception
    {
        public TinifyException(string message) : base(message)
        {
        }

        public TinifyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal ErrorRecord CreateErrorRecord(string errorId = null,
            ErrorCategory errorCategory = ErrorCategory.NotSpecified, object targetObject = null)
        {
            var errorRecord = new ErrorRecord(this, errorId, errorCategory, targetObject);
            errorRecord.ErrorDetails = new ErrorDetails("TinyImage Error: " + Message);
            return errorRecord;
        }
    }
}