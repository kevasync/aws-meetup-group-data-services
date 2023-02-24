using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.Kinesis;
using Pulumi.Aws.Kinesis.Inputs;
using System.Linq;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Kinesis {
        public static Stream CreateStream(string streamName) {
            return new Stream(streamName, new StreamArgs {
                RetentionPeriod = 48,
                ShardCount = 1,
                Tags = Common.tags
            });
        }

        public static FirehoseDeliveryStream CreateRawDataS3Firehose(string name, Output<string> inputStreamArn, Output<string> outputBucketArn, Output<string> roleArn) {
            
            
            return new FirehoseDeliveryStream(name, new FirehoseDeliveryStreamArgs{
                KinesisSourceConfiguration = new FirehoseDeliveryStreamKinesisSourceConfigurationArgs {
                    KinesisStreamArn = inputStreamArn, 
                    RoleArn = roleArn
                },
                Destination = "s3",
                S3Configuration = new FirehoseDeliveryStreamS3ConfigurationArgs {
                    BucketArn = outputBucketArn,
                    RoleArn = roleArn
                },
                Tags = Common.tags
            });
        }

        public static FirehoseDeliveryStream CreateEnrichedDataS3Firehose(string name, Output<string> inputStreamArn, Output<string> bucketArn, Output<string> roleArn, Output<string> glueDbName, Output<string> glueSchemaTableName) {
            return new FirehoseDeliveryStream(name, new FirehoseDeliveryStreamArgs {
                KinesisSourceConfiguration = new FirehoseDeliveryStreamKinesisSourceConfigurationArgs {
                    KinesisStreamArn = inputStreamArn, 
                    RoleArn = roleArn
                },
                Destination = "extended_s3",
                ExtendedS3Configuration = new FirehoseDeliveryStreamExtendedS3ConfigurationArgs () {
                    BucketArn = bucketArn,
                    RoleArn = roleArn,
                    BufferSize = 128,
                    DataFormatConversionConfiguration = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationArgs {
                        InputFormatConfiguration = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationInputFormatConfigurationArgs {
                            Deserializer = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationInputFormatConfigurationDeserializerArgs {
                                HiveJsonSerDe = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationInputFormatConfigurationDeserializerHiveJsonSerDeArgs {}
                            }
                        },
                        OutputFormatConfiguration = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationOutputFormatConfigurationArgs {
                            Serializer = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationOutputFormatConfigurationSerializerArgs {
                                ParquetSerDe = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationOutputFormatConfigurationSerializerParquetSerDeArgs { }
                            }
                        },
                        SchemaConfiguration = new FirehoseDeliveryStreamExtendedS3ConfigurationDataFormatConversionConfigurationSchemaConfigurationArgs {
                            DatabaseName = glueDbName,
                            TableName = glueSchemaTableName,
                            RoleArn = roleArn
                        }
                    },
                },
                Tags = Common.tags
            });
        }

        public static FirehoseDeliveryStream CreateEnrichedDataRedshiftFirehose(string name, Output<string> endpoint, Output<int?> port, Output<string> database, Output<string?> username, Output<string?> password, Output<string> roleArn, string tableName, string columns, Output<string> inputStreamArn, Output<string> tempBucketArn) {
            
            return new FirehoseDeliveryStream(name, new FirehoseDeliveryStreamArgs {
                KinesisSourceConfiguration = new FirehoseDeliveryStreamKinesisSourceConfigurationArgs {
                    KinesisStreamArn = inputStreamArn, 
                    RoleArn = roleArn
                },
                Destination = "redshift",
                RedshiftConfiguration = new FirehoseDeliveryStreamRedshiftConfigurationArgs() {
                    //https://github.com/pulumi/pulumi/issues/1631
                    ClusterJdbcurl = "jdbc:redshift://awsmeetupgroup-dataservicesdemo-redshift-1.cilhes4fcm24.us-west-2.redshift.amazonaws.com:5439/default_db",
                    RoleArn = roleArn,
                    DataTableName = tableName,
                    DataTableColumns = columns,
                    Username = username,
                    Password = password
                },
                S3Configuration = new FirehoseDeliveryStreamS3ConfigurationArgs() { 
                    BucketArn = tempBucketArn, RoleArn = roleArn 
                },
                Tags = Common.tags
            });
        }

          public static FirehoseDeliveryStream CreateEnrichedDataOpenSearchFirehose(string name, Output<string> domainArn, Output<string> roleArn, string indexName, Output<string> inputStreamArn, Output<string> tempBucketArn) {
            return new FirehoseDeliveryStream(name, new FirehoseDeliveryStreamArgs {
                KinesisSourceConfiguration = new FirehoseDeliveryStreamKinesisSourceConfigurationArgs {
                    KinesisStreamArn = inputStreamArn, 
                    RoleArn = roleArn
                },
                Destination = "elasticsearch",
                ElasticsearchConfiguration = new FirehoseDeliveryStreamElasticsearchConfigurationArgs() {
                    RoleArn = roleArn,
                    DomainArn = domainArn,
                    IndexName = indexName
                },
                S3Configuration = new FirehoseDeliveryStreamS3ConfigurationArgs() {
                    BucketArn = tempBucketArn, RoleArn = roleArn
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
                                    RecordRowPath = "."
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
                        KinesisStream = new AnalyticsApplicationOutputKinesisStreamArgs() {
                            ResourceArn = args.OutputArn,
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
                                Csv = new AnalyticsApplicationReferenceDataSourcesSchemaRecordFormatMappingParametersCsvArgs {
                                    RecordColumnDelimiter = ",",
                                    RecordRowDelimiter = "\n"
                                }
                            }
                        },
                        RecordColumns = args.ReferenceFileColumns.Select(x => new AnalyticsApplicationReferenceDataSourcesSchemaRecordColumnArgs{
                            Name = x,
                            SqlType = "VARCHAR(1024)",
                            Mapping = x
                        }).ToList()
                    }
                },
                Tags = Common.tags
            });
        }
    }
}