using Pulumi.Aws.S3;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class S3 {
        public static Bucket CreateS3Bucket(string name) {
            return new Bucket(name.Replace("_", "-"), new BucketArgs {
                Acl = "private",
                Tags = Common.tags
            });
        }
    }
}