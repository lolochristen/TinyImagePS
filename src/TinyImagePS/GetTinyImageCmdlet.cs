using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using TinyImagePS.Models;

namespace TinyImagePS
{
    [Cmdlet(VerbsCommon.Get, "TinyImage")]
    public class GetTinyImageCmdlet : PSCmdlet
    {
        private const string ResizeSetName = "Resize";

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
        public string Destination { get; set; }

        [Parameter] public SwitchParameter Force { get; set; }

        [Parameter] public SwitchParameter AsByteStream { get; set; }

        [Parameter(ParameterSetName = ResizeSetName,
            Position = 1)]
        public ResizeMode ResizeMode { get; set; }

        [Parameter(ParameterSetName = ResizeSetName)]
        public int? Width { get; set; }

        [Parameter(ParameterSetName = ResizeSetName)]
        public int? Height { get; set; }

        [Parameter(
            HelpMessage = "ApiKey obtained from https://tinypng.com/developers",
            ValueFromPipelineByPropertyName = true
        )]
        public string ApiKey { get; set; }

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

            if (string.IsNullOrEmpty(Destination))
            {
                // to pipeline
                byte[] content;

                if (ParameterSetName == ResizeSetName)
                {
                    WriteVerbose($"Resize: Url:{ProcessObject.Output.Url} Mode:{ResizeMode} Size:{Width}x{Height}");

                    using (var stream = tinify.Resize(ProcessObject.Output, ResizeMode, Width, Height))
                    {
                        content = new byte[stream.Length]; // read all in to memory ;-(
                        stream.Read(content, 0, (int)stream.Length);
                    }
                }
                else
                {
                    using (var stream = tinify.GetStream(ProcessObject.Output))
                    {
                        content = new byte[stream.Length]; // read all in to memory ;-(
                        stream.Read(content, 0, (int)stream.Length);
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
                bool isContainer = InvokeProvider.Item.IsContainer(Destination);
                string destinationPath;
                if (isContainer)
                {
                    var dirInfo = InvokeProvider.Item.Get(Destination).First().BaseObject as DirectoryInfo;
                    if (dirInfo == null)
                    {
                        WriteError(new TinifyException("Only FileSystemProvider is supported.").CreateErrorRecord("Exception"));
                        return;
                    }
                    destinationPath = System.IO.Path.Combine(dirInfo.FullName, ProcessObject.Source.Name);
                }
                else
                {
                    destinationPath = Destination;
                }

                if (File.Exists(destinationPath) && Force == false)
                {
                    WriteError(
                        new TinifyException($"File {destinationPath} already exists.").CreateErrorRecord(
                            "FileExists"));
                    return;
                }

                if (ParameterSetName == ResizeSetName)
                {
                    WriteVerbose($"Resize: Url:{ProcessObject.Output.Url} Mode:{ResizeMode} Size:{Width}x{Height} to {destinationPath}");
                    tinify.Resize(ProcessObject.Output, ResizeMode, Width, Height, destinationPath);
                }
                else
                {
                    WriteVerbose($"Download: Url:{ProcessObject.Output.Url} to {destinationPath}");
                    tinify.DownloadFile(ProcessObject.Output, destinationPath);
                }
            }
        }
    }
}