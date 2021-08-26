using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using TinyImagePS.Models;

namespace TinyImagePS
{
    [Cmdlet(VerbsData.Compress, "TinyImage")]
    [OutputType(typeof(TinifyProcessInfo))]
    public class CompressTinyImageCmdlet : AsyncCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
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

        protected override async Task ProcessRecordAsync()
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
            {
                try
                {
                    WriteVerbose($"Tinyfy {path}");
                    var response = await tinify.Shrink(path);

                    if (Replace)
                    {
                        DestinationPath = path;
                        Force = true;
                    }

                    WriteVerbose($"Tinify successfull: Input:{response.Input} Output:{response.Output}");

                    if (string.IsNullOrEmpty(DestinationPath))
                    {
                        // to pipeline
                        WriteObject(new TinifyProcessInfo(response, path));
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

                        WriteVerbose($"Download to {DestinationPath}");

                        // to file
                        await tinify.DownloadFile(response.Output, DestinationPath);
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
    }
}