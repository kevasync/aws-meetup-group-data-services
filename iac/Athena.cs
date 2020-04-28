using Pulumi;
using Pulumi.Aws.Athena;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Athena {
        public static Database CreateDatabase(string name, Output<string> bucketName) {
            return new Database(name.ToLower(), new DatabaseArgs{
                Bucket = bucketName
            });
        }
    }
}