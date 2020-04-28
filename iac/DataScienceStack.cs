using System.Collections.Generic;
using Pulumi;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    class DataScienceStack : Stack
    {
        public DataScienceStack()
        {
            var sensorTopicName = "aws-meetup-group.iot.sensor-readings.incoming";
            var iotSensorIngestStream = Kinesis.CreateStream(sensorTopicName);
            var iotCert = IoT.createIoT(iotSensorIngestStream.Name);
            var sensorRawDataBucket = S3.CreateS3Bucket($"{Common.appName}-sensor-raw-data");
            var tempRefDataBucket = S3.CreateS3Bucket($"{Common.appName}-temperature-ref-data");
            var pressureRefDataBucket = S3.CreateS3Bucket($"{Common.appName}-pressure-ref-data");
            var temperatureEnrichedDataBucket = S3.CreateS3Bucket($"{Common.appName}-temp-enriched-data");
            var pressureEnrichedDataBucket = S3.CreateS3Bucket($"{Common.appName}-pressure-enriched-data");

            var firehoseRole = Iam.CreateFirehoseRole();
            var sensorS3Firehose = Kinesis.CreateRawDataS3Firehose($"{Common.appName}_sensor_producer_s3", iotSensorIngestStream.Arn, sensorRawDataBucket.Arn, firehoseRole.Arn);
            var temperatureS3Firehose = Kinesis.CreateEnrichedDataS3Firehose($"{Common.appName}_temperature_producer_s3", temperatureEnrichedDataBucket.Arn, firehoseRole.Arn);
            var pressureS3Firehose = Kinesis.CreateEnrichedDataS3Firehose($"{Common.appName}_pressure_producer_s3", pressureEnrichedDataBucket.Arn, firehoseRole.Arn);

            var inputStreamColumns = new List<string>(){ "SITE_ID", "SENSOR_TYPE", "SENSOR_READING_VALUE", "READING_TIMESTAMP" };
            var analyticsAppRole = Iam.CreateKinesisAnalyticsAppRole();
            var tempAnalyticsApp = Kinesis.CreateAnalyticsApplication($"{Common.appName}_temperature_data_analytics", new AnalyticsAppS3EnrichmentArgs (
                tempRefDataBucket.Arn,
                AnalyticsSqlQueries.temperatureQuery,
                iotSensorIngestStream.Arn,
                temperatureS3Firehose.Arn,
                analyticsAppRole.Arn,
                "temperature",
                inputStreamColumns,
                "JSON",
                "weather.json",
                new List<string>(){"SITE_ID", "OUTSIDE_TEMPERATURE"}
            ));
            var pressureAnalyticsApp = Kinesis.CreateAnalyticsApplication($"{Common.appName}_pressure_data_analytics", new AnalyticsAppS3EnrichmentArgs (
                pressureRefDataBucket.Arn,
                AnalyticsSqlQueries.pressureQuery,
                iotSensorIngestStream.Arn,
                pressureS3Firehose.Arn,
                analyticsAppRole.Arn,
                "pressure",
                inputStreamColumns,
                "JSON",
                "altitude.json",
                new List<string>(){"SITE_ID", "ALTITUDE"}
            ));

            var temperatureAthenaDb = Athena.CreateDatabase($"{Common.appName}_temperature_athena_db", temperatureEnrichedDataBucket.BucketName);
            var pressureAthenaDb = Athena.CreateDatabase($"{Common.appName}_pressure_athena_db", pressureEnrichedDataBucket.BucketName);
        }
    }
}