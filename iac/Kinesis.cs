using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.Kinesis;
using Pulumi.Aws.Kinesis.Inputs;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Kinesis {
        public static Stream CreateStream(string topicName) {
            return new Stream(topicName, new StreamArgs {
                RetentionPeriod = 48,
                ShardCount = 1,
                Tags = Common.tags
            });
        }

        public static FirehoseDeliveryStream CreateS3Firehose(string name, Output<string> streamArn, Output<string> bucketArn) {
            var firehoseRole = Iam.CreateFirehoseRole();
            return new FirehoseDeliveryStream(name, new FirehoseDeliveryStreamArgs{
                KinesisSourceConfiguration = new FirehoseDeliveryStreamKinesisSourceConfigurationArgs {
                    KinesisStreamArn = streamArn, 
                    RoleArn = firehoseRole.Arn
                },
                Destination = "s3",
                S3Configuration = new FirehoseDeliveryStreamS3ConfigurationArgs {
                    BucketArn = bucketArn,
                    RoleArn = firehoseRole.Arn
                },
                Tags = Common.tags
            });
        }

        public static AnalyticsApplication CreateAnalyticsApplication(string name) {
            var analyticsAppRole = Iam.CreateKinesisAnalyticsAppRole();
            return new AnalyticsApplication(name, new AnalyticsApplicationArgs {
                Code = "SQL stuff here :)",
                CloudwatchLoggingOptions = new AnalyticsApplicationCloudwatchLoggingOptionsArgs {},
                Inputs = new AnalyticsApplicationInputsArgs{},
                Outputs = new List<AnalyticsApplicationOutputArgs>(){
                    new AnalyticsApplicationOutputArgs{}
                },
                ReferenceDataSources = new AnalyticsApplicationReferenceDataSourcesArgs{},
                Tags = Common.tags
            });
        }
    }
}