using System;
using System.IO;
using System.Management.Automation;

namespace TinyImagePS
{
    [Cmdlet(VerbsCommon.Set, "TinyImageApiKey")]
    public class SetTinyImageApiKeyCmdlet : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "ApiKey obtained from https://tinypng.com/developers",
            ValueFromPipelineByPropertyName = true
        )]
        public string ApiKey { get; set; }

        protected override void ProcessRecord()
        {
            SetApiKeyToFile(ApiKey);
        }

        internal static string GetApiKeyFromFile()
        {
            var path = GetApiKeyFile();
            using (var stream = File.OpenText(path))
                return stream.ReadLine();
        }

        internal static void SetApiKeyToFile(string apiKey)
        {
            var path = GetApiKeyFile();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var stream = File.CreateText(path))
                stream.Write(apiKey);
        }

        internal static string GetApiKeyFile()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                $".tinify{Path.DirectorySeparatorChar}tinify.apikey");
        }
    }
}