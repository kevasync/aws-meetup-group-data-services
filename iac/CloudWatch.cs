using Pulumi.Aws.CloudWatch;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class CloudWatch {
        public static LogStream CreateLogStream(string name) {
            return new LogStream(name, new LogStreamArgs{
                LogGroupName = new LogGroup($"{name}-group", new LogGroupArgs{
                    RetentionInDays = 1,
                    Tags = Common.tags
                }).Name
            });
        }
    }
}