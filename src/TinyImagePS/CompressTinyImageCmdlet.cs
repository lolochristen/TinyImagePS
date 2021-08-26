using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Threading.Tasks;
using Microsoft.PowerShell.Commands;
using TinyImagePS.Models;

namespace TinyImagePS
{
    [Cmdlet(VerbsData.Compress, "TinyImage")]
    [OutputType(typeof(TinifyProcessInfo))]
    public class CompressTinyImageCmdlet : PSCmdlet
    {
        private string[] paths;
        private bool isLiteral;

        /// <summary>
        /// Gets or sets the path parameter to the command.
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "Path",
            Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string[] Path
        {
            get { return paths; }

            set { paths = value; }
        }

        /// <summary>
        /// Gets or sets the literal path parameter to the command.
        /// </summary>
        [Parameter(ParameterSetName = "LiteralPath",
            Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Alias("PSPath", "LP")]
        public string[] LiteralPath
        {
            get
            {
                return paths;
            }

            set
            {
                isLiteral = true;
                paths = value;
            }
        }

        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true
        )]
        public string Destination { get; set; }

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

            var items = InvokeProvider.Item.Get(Path, true, isLiteral);

            foreach (var item in items)
            {
                try
                {
                    var fileInfo = item.BaseObject as FileSystemInfo;

                    if (fileInfo == null)
                    {
                        WriteError(new TinifyException("Only FileSystemProvider is supported.").CreateErrorRecord());
                        continue;
                    }

                    var path = fileInfo.FullName;

                    WriteVerbose($"Tinyfy {path}");

                    var response = tinify.Shrink(path);

                    if (Replace)
                    {
                        Destination = path;
                        Force = true;
                    }

                    WriteVerbose($"Tinify was successful: Input:{response.Input} Output:{response.Output}");

                    if (string.IsNullOrEmpty(Destination))
                    {
                        // to pipeline
                        WriteObject(new TinifyProcessInfo(response, fileInfo));
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
                                continue;
                            }
                            destinationPath = System.IO.Path.Combine(dirInfo.FullName, fileInfo.Name);
                        }
                        else
                        {
                            destinationPath = Destination; 
                        }

                        // download file directly...
                        if (File.Exists(destinationPath) && Force == false)
                        {
                            WriteError(
                                new TinifyException($"File {destinationPath} already exists.").CreateErrorRecord(
                                    "FileExists"));
                            continue;
                        }

                        WriteVerbose($"Download to {destinationPath}");
                        tinify.DownloadFile(response.Output, destinationPath);
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