using System;
using System.IO;
using System.Management.Automation;
using TinyImagePS.Models;

namespace TinyImagePS
{
    [Cmdlet(VerbsCommon.Get, "TinyImage")]
    public class GetTinyImageCmdlet : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true
        )]
        public TinifyResponse ResponseObject { get; set; }

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

            if (string.IsNullOrEmpty(DestinationPath))
            {
                // to pipeline
                byte[] content;

                if (ParameterSetName == "Resize")
                    using (var stream = tinify.Resize(ResponseObject, ResizeMode, Width, Height).Result)
                    {
                        content = new byte[stream.Length]; // read all in to memory ;-(
                        stream.Write(content, 0, (int)stream.Length);
                    }

                //Task<Stream> task;
                //switch (ResizeMode)
                //{
                //    case ResizeMode.Scale:
                //        task = tinify.Scale(ResponseObject, Width, Height);
                //        break;
                //    case ResizeMode.Fit:
                //        task = tinify.Fit(ResponseObject, Width, Height);
                //        break;
                //    case ResizeMode.Cover:
                //        task = tinify.Cover(ResponseObject, Width, Height);
                //        break;
                //    case ResizeMode.Thumb:
                //        task = tinify.Thumb(ResponseObject, Width, Height);
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //}

                //task.Wait();
                //var stream = task.Result;
                //try
                //{
                //    content = new byte[stream.Length]; // read all in to memory ;-(
                //    task.Result.Write(content, 0, (int)stream.Length);
                //}
                //finally
                //{
                //    stream.Dispose();
                //}
                else
                    using (var stream = tinify.GetStream(ResponseObject).Result)
                    {
                        content = new byte[stream.Length]; // read all in to memory ;-(
                        stream.Write(content, 0, (int)stream.Length);
                    }
                //content = tinify.DownloadBytes(ResponseObject).Result;

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

                if (ParameterSetName == "Resize")
                    tinify.Resize(ResponseObject, ResizeMode, Width, Height, DestinationPath).Wait();

                //switch (ResizeMode)
                //{
                //    case ResizeMode.Scale:
                //        tinify.Scale(ResponseObject, Width, Height, DestinationPath).Wait();
                //        break;
                //    case ResizeMode.Fit:
                //        tinify.Fit(ResponseObject, Width, Height, DestinationPath).Wait();
                //        break;
                //    case ResizeMode.Cover:
                //        tinify.Cover(ResponseObject, Width, Height, DestinationPath).Wait();
                //        break;
                //    case ResizeMode.Thumb:
                //        tinify.Thumb(ResponseObject, Width, Height, DestinationPath).Wait();
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //}
                else
                    // to file
                    tinify.DownloadFile(ResponseObject, DestinationPath).Wait();
            }
        }
    }
}