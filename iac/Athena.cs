using Pulumi;
using Pulumi.Aws.Athena;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Athena {
        public static Database CreateDatabase(string name, Output<string> bucketArn) {
            return new Database(name.ToLower(), new DatabaseArgs{
                Bucket = bucketArn
            });
        }
    }
}