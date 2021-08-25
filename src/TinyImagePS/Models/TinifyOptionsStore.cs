namespace TinyImagePS.Models
{
    public class TinifyOptionsStore
    {
        public string service = "s3";
        public string aws_access_key_id { get; set; }
        public string aws_secret_access_key { get; set; }
        public string region { get; set; }
        public string path { get; set; }
    }
}