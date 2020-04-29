using System.Collections.Generic;
using Pulumi;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    class DataScienceStack : Stack
    {
        public DataScienceStack()
        {
            //Create Buckets (Need to manually upload ref .CSV files)
            var sensorRawDataBucket = S3.CreateS3Bucket($"{Common.appName}-sensor-raw-data");
            var sensorRefDataBucket = S3.CreateS3Bucket($"{Common.appName}-sensor-ref-data");
            var temperatureEnrichedDataBucket = S3.CreateS3Bucket($"{Common.appName}-temp-enriched-data");
            var pressureEnrichedDataBucket = S3.CreateS3Bucket($"{Common.appName}-pressure-enriched-data");
            var ahtenaResultBucket = S3.CreateS3Bucket($"{Common.appName}-athena-result");

            //Create Incoming Kinesis Data Strean
            var sensorTopicName = "aws-meetup-group.iot.sensor-readings.incoming";
            var iotSensorIngestStream = Kinesis.CreateStream(sensorTopicName);
            
            
            //Create Firehose Delivery Streams
            var firehoseRole = Iam.CreateFirehoseRole();
            var sensorS3Firehose = Kinesis.CreateRawDataS3Firehose($"{Common.appName}_sensor_producer_s3", iotSensorIngestStream.Arn, sensorRawDataBucket.Arn, firehoseRole.Arn);
            var temperatureS3Firehose = Kinesis.CreateEnrichedDataS3Firehose($"{Common.appName}_temperature_producer_s3", temperatureEnrichedDataBucket.Arn, firehoseRole.Arn);
            var pressureS3Firehose = Kinesis.CreateEnrichedDataS3Firehose($"{Common.appName}_pressure_producer_s3", pressureEnrichedDataBucket.Arn, firehoseRole.Arn);

            //Create Kinesis Analytics Apps
            var inputStreamColumns = new List<string>(){ "SITE_ID", "SENSOR_TYPE", "SENSOR_READING_VALUE", "READING_TIMESTAMP" };
            var analyticsAppRole = Iam.CreateKinesisAnalyticsAppRole();
            var tempAnalyticsApp = Kinesis.CreateAnalyticsApplication($"{Common.appName}_temperature_data_analytics", new AnalyticsAppS3EnrichmentArgs (
                sensorRefDataBucket.Arn,
                AnalyticsSqlQueries.temperatureQuery,
                iotSensorIngestStream.Arn,
                temperatureS3Firehose.Arn,
                analyticsAppRole.Arn,
                "temperature",
                inputStreamColumns,
                "JSON",
                "weather.csv",
                new List<string>(){"SITE_ID", "OUTSIDE_TEMPERATURE"}
            ));
            var pressureAnalyticsApp = Kinesis.CreateAnalyticsApplication($"{Common.appName}_pressure_data_analytics", new AnalyticsAppS3EnrichmentArgs (
                sensorRefDataBucket.Arn,
                AnalyticsSqlQueries.pressureQuery,
                iotSensorIngestStream.Arn,
                pressureS3Firehose.Arn,
                analyticsAppRole.Arn,
                "pressure",
                inputStreamColumns,
                "JSON",
                "altitude.csv",
                new List<string>(){"SITE_ID", "ALTITUDE"}
            ));

            //Create Ahtena Database (Need to manually create tables)
            var s3AthenaDb = Athena.CreateDatabase($"{Common.appName}_sensor_athena_db", ahtenaResultBucket.BucketName);
            this.StreamName = iotSensorIngestStream.Name;
            this.EnrichedPressureBucketName = pressureEnrichedDataBucket.BucketName;
            this.EnrichedTemperatureBucketName = temperatureEnrichedDataBucket.BucketName;
            this.RawSensorDataBucketName = sensorRawDataBucket.BucketName;
            this.ReferenceDataBucket = sensorRefDataBucket.BucketName;
        }

        [Output]
        public Output<string> StreamName { get; set; }
        [Output]
        public Output<string> EnrichedPressureBucketName { get; set; }
        [Output]
        public Output<string> EnrichedTemperatureBucketName { get; set; }

        public Output<string> RawSensorDataBucketName { get; set; }
        [Output]
        public Output<string> ReferenceDataBucket { get; set; }
    }
}