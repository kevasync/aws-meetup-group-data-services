using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.Kinesis;
using Pulumi.Aws.Kinesis.Inputs;
using System.Linq;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Kinesis {
        public static Stream CreateStream(string topicName) {
            return new Stream(topicName, new StreamArgs {
                RetentionPeriod = 48,
                ShardCount = 1,
                Tags = Common.tags
            });
        }

        public static FirehoseDeliveryStream CreateRawDataS3Firehose(string name, Output<string> streamArn, Output<string> bucketArn, Output<string> roleArn) {
            return new FirehoseDeliveryStream(name, new FirehoseDeliveryStreamArgs{
                KinesisSourceConfiguration = new FirehoseDeliveryStreamKinesisSourceConfigurationArgs {
                    KinesisStreamArn = streamArn, 
                    RoleArn = roleArn
                },
                Destination = "s3",
                S3Configuration = new FirehoseDeliveryStreamS3ConfigurationArgs {
                    BucketArn = bucketArn,
                    RoleArn = roleArn
                },
                Tags = Common.tags
            });
        }

        public static FirehoseDeliveryStream CreateEnrichedDataS3Firehose(string name, Output<string> bucketArn, Output<string> roleArn) {
            return new FirehoseDeliveryStream(name, new FirehoseDeliveryStreamArgs {
                Destination = "s3",
                S3Configuration = new FirehoseDeliveryStreamS3ConfigurationArgs {
                    BucketArn = bucketArn,
                    RoleArn = roleArn
                },
                Tags = Common.tags
            });
        }

        public static AnalyticsApplication CreateAnalyticsApplication(string name, AnalyticsAppS3EnrichmentArgs args) {
            return new AnalyticsApplication(name, new AnalyticsApplicationArgs {
                Code = args.Code,
                CloudwatchLoggingOptions = new AnalyticsApplicationCloudwatchLoggingOptionsArgs {
                    LogStreamArn = CloudWatch.CreateLogStream($"{name}-log-stream").Arn,
                    RoleArn = args.RoleArn
                },
                Inputs = new AnalyticsApplicationInputsArgs{
                    KinesisStream = new AnalyticsApplicationInputsKinesisStreamArgs {
                        ResourceArn = args.StreamArn,
                        RoleArn = args.RoleArn
                    },
                    NamePrefix = args.NamePrefix,
                    Schema = new AnalyticsApplicationInputsSchemaArgs {
                        RecordFormat = new AnalyticsApplicationInputsSchemaRecordFormatArgs {
                            MappingParameters = new AnalyticsApplicationInputsSchemaRecordFormatMappingParametersArgs {
                                Json = new AnalyticsApplicationInputsSchemaRecordFormatMappingParametersJsonArgs{
                                    RecordRowPath = ".records"
                                }
                            }
                        },
                        RecordColumns = args.StreamFileColumns.Select(x => new AnalyticsApplicationInputsSchemaRecordColumnArgs {
                            Name = x,
                            SqlType = "VARCHAR(1024)",
                            Mapping = $"$.{x}"
                        }).ToList()
                    }
                },
                Outputs = new List<AnalyticsApplicationOutputArgs>(){
                    new AnalyticsApplicationOutputArgs {
                        Name = $"{args.NamePrefix.ToUpper()}_ANALYTICS_TOPIC",
                        KinesisFirehose = new AnalyticsApplicationOutputKinesisFirehoseArgs {
                            ResourceArn = args.OutputFirehoseArn,
                            RoleArn = args.RoleArn
                        },
                        Schema = new AnalyticsApplicationOutputSchemaArgs {
                            RecordFormatType = args.OutputFormat
                        }
                    }
                },
                ReferenceDataSources = new AnalyticsApplicationReferenceDataSourcesArgs {
                    TableName = $"{args.NamePrefix.ToUpper()}_REFERENCE_TABLE",
                    S3 = new AnalyticsApplicationReferenceDataSourcesS3Args {
                        BucketArn = args.ReferenceBucketArn,
                        FileKey = args.ReferenceFileKey,
                        RoleArn = args.RoleArn
                    },
                    Schema = new AnalyticsApplicationReferenceDataSourcesSchemaArgs {
                        RecordFormat = new AnalyticsApplicationReferenceDataSourcesSchemaRecordFormatArgs {
                            MappingParameters = new AnalyticsApplicationReferenceDataSourcesSchemaRecordFormatMappingParametersArgs {
                                Json = new AnalyticsApplicationReferenceDataSourcesSchemaRecordFormatMappingParametersJsonArgs {
                                    RecordRowPath = ".records"
                                }
                            }
                        },
                        RecordColumns = args.ReferenceFileColumns.Select(x => new AnalyticsApplicationReferenceDataSourcesSchemaRecordColumnArgs{
                            Name = x,
                            SqlType = "VARCHAR(1024)",
                            Mapping = $"$.{x}"
                        }).ToList()
                    }
                },
                Tags = Common.tags
            });
        }
    }
}