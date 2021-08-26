using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using TinyImagePS.Models;

namespace TinyImagePS
{
    [Cmdlet(VerbsCommon.Get, "TinyImage")]
    public class GetTinyImageCmdlet : AsyncCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
        )]
        public TinifyProcessInfo ProcessObject { get; set; }

        [Parameter(
            Position = 0,
            ValueFromPipelineByPropertyName = true
        )]
        public string DestinationPath { get; set; }

        [Parameter] public SwitchParameter Force { get; set; }

        [Parameter] public SwitchParameter AsByteStream { get; set; }

        [Parameter(ParameterSetName = "Resize",
            Position = 1)]
        public ResizeMode ResizeMode { get; set; }

        [Parameter(ParameterSetName = "Resize")]
        public int? Width { get; set; }

        [Parameter(ParameterSetName = "Resize")]
        public int? Height { get; set; }

        [Parameter(
            HelpMessage = "ApiKey obtained from https://tinypng.com/developers",
            ValueFromPipelineByPropertyName = true
        )]
        public string ApiKey { get; set; }

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

            if (string.IsNullOrEmpty(DestinationPath))
            {
                // to pipeline
                byte[] content;

                if (ParameterSetName == "Resize")
                {
                    WriteVerbose($"Resize operation: Url:{ProcessObject.Output.Url} Mode:{ResizeMode} Size:{Width}x{Height}");

                    using (var stream = await tinify.Resize(ProcessObject.Output, ResizeMode, Width, Height))
                    {
                        content = new byte[stream.Length]; // read all in to memory ;-(
                        await stream.WriteAsync(content, 0, (int)stream.Length);
                    }
                }
                else
                {
                    using (var stream = tinify.GetStream(ProcessObject.Output).Result)
                    {
                        content = new byte[stream.Length]; // read all in to memory ;-(
                        await stream.WriteAsync(content, 0, (int)stream.Length);
                    }
                }

                if (AsByteStream.IsPresent)
                    WriteObject(content);
                else
                    WriteObject(
                        Convert.ToBase64String(content)); // ??
            }
            else
            {
                if (File.Exists(DestinationPath) && Force == false)
                {
                    WriteError(
                        new TinifyException($"File {DestinationPath} already exists.").CreateErrorRecord(
                            "FileExists"));
                    return;
                }

                WriteVerbose($"Resize operation: Url:{ProcessObject.Output.Url} Mode:{ResizeMode} Size:{Width}x{Height} to {DestinationPath}");

                if (ParameterSetName == "Resize")
                    await tinify.Resize(ProcessObject.Output, ResizeMode, Width, Height, DestinationPath);
                else
                    // to file
                    await tinify.DownloadFile(ProcessObject.Output, DestinationPath);
            }
        }
    }
}