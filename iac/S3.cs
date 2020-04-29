using System.Reflection;
using Pulumi.Aws.S3;
using System.Linq;
using System.IO;
using Pulumi;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class S3 {
        public static Bucket CreateS3Bucket(string name) {
            return new Bucket(name.Replace("_", "-"), new BucketArgs {
                Acl = "private",
                Tags = Common.tags
            });
        }

        public static BucketObject CreateS3Object(string name, Output<string> bucketArn, string key, string filePath){
            return new BucketObject(name, new BucketObjectArgs {
                Key = key, 
                Bucket = bucketArn,
                Content = ReadResource(filePath)
            });
        }

        private static string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            
            resourcePath = assembly.GetManifestResourceNames().Single(x => x == name);
            
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}