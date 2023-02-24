using System.Collections.Generic;
using System.Collections.Immutable;
using Pulumi;
using Pulumi.Aws.RedshiftServerless.Outputs;

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
            var athenaResultBucket = S3.CreateS3Bucket($"{Common.appName}-athena-result");
            var firehoseTempBucket = S3.CreateS3Bucket($"{Common.appName}-firehose-temp");
            
            //Create Glue Schemas
            var glueDbName = $"{Common.appName}_glue_schema_database";
            var glueDb = Glue.CreateGlueDatabase(glueDbName);
            var temperatureGlueSchemaTable = Glue.CreateGlueTable(glueDb.Name, $"{Common.appName}_glue_temperature_table",
                new List<string> {"SITE_ID", "SENSOR_READING_VALUE", "READING_TIMESTAMP", "OUTSIDE_TEMPERATURE"});
            var pressureGlueSchemaTable = Glue.CreateGlueTable(glueDb.Name, $"{Common.appName}_glue_pressure_table",
                new List<string> {"SITE_ID", "SENSOR_READING_VALUE", "READING_TIMESTAMP", "ALTITUDE"});
            
            //Create Redshift 
            var subnets = Networking.CreateSubnets();
            var securityGroup = Networking.CreateSecurityGroup(subnets[0].VpcId);
            var redshiftRole = Iam.CreateRedshiftRole();
            var redshiftServerlessWorkgroup = Redshift.CreateRedshiftServerless("default_db", redshiftRole.Arn, subnets, securityGroup.Id);

            //Create OpenSearch
            var openSearchDomain = OpenSearch.CreateSearchDomain("meetup-search-domain");

            //Create Kinesis Data Streams
            var sensorTopicName = "aws-meetup-group.iot.sensor-readings.incoming";
            var iotSensorIngestStream = Kinesis.CreateStream(sensorTopicName);
            var enrichedTemperatureTopicName = "aws-meetup-group.temperature.enriched";
            var enrichedTemperatureStream = Kinesis.CreateStream(enrichedTemperatureTopicName);
            var enrichedPressureTopicName = "aws-meetup-group.pressure.enriched";
            var enrichedPressureStream = Kinesis.CreateStream(enrichedPressureTopicName);
                        
            //Create Firehose Delivery Streams
            var firehoseRole = Iam.CreateFirehoseRole();
            
            var sensorS3Firehose = Kinesis.CreateRawDataS3Firehose($"{Common.appName}_sensor_producer_s3", iotSensorIngestStream.Arn, sensorRawDataBucket.Arn, firehoseRole.Arn);
            
            var temperatureS3Firehose = Kinesis.CreateEnrichedDataS3Firehose($"{Common.appName}_temperature_producer_s3", enrichedTemperatureStream.Arn, temperatureEnrichedDataBucket.Arn, firehoseRole.Arn, glueDb.Name, temperatureGlueSchemaTable.Name);
            var temperatureOpenSearchFirehose = Kinesis.CreateEnrichedDataOpenSearchFirehose($"{Common.appName}_temperature_producer_os", openSearchDomain.Arn, firehoseRole.Arn, "temperature", enrichedTemperatureStream.Arn, firehoseTempBucket.Arn);
            var pressureS3Firehose = Kinesis.CreateEnrichedDataS3Firehose($"{Common.appName}_pressure_producer_s3", enrichedPressureStream.Arn, pressureEnrichedDataBucket.Arn, firehoseRole.Arn, glueDb.Name, pressureGlueSchemaTable.Name);
            var pressureOpenSearchFirehose = Kinesis.CreateEnrichedDataOpenSearchFirehose($"{Common.appName}_pressure_producer_os", openSearchDomain.Arn, firehoseRole.Arn, "pressure", enrichedPressureStream.Arn, firehoseTempBucket.Arn);

            //Create Kinesis Analytics Apps
            var inputStreamColumns = new List<string>(){ "SITE_ID", "SENSOR_TYPE", "SENSOR_READING_VALUE", "READING_TIMESTAMP" };
            var analyticsAppRole = Iam.CreateKinesisAnalyticsAppRole();
            var tempAnalyticsApp = Kinesis.CreateAnalyticsApplication($"{Common.appName}_temperature_data_analytics", new AnalyticsAppS3EnrichmentArgs (
                sensorRefDataBucket.Arn,
                AnalyticsSqlQueries.temperatureQuery,
                iotSensorIngestStream.Arn,
                enrichedTemperatureStream.Arn,
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
                enrichedPressureStream.Arn,
                analyticsAppRole.Arn,
                "pressure",
                inputStreamColumns,
                "JSON",
                "altitude.csv",
                new List<string>(){"SITE_ID", "ALTITUDE"}
            ));

            //Create Ahtena Database (Need to manually create tables)
            var s3AthenaDb = Athena.CreateDatabase($"{Common.appName}_sensor_athena_db", athenaResultBucket.BucketName);

            //Create IoT Core things
            var iotCert = IoT.createIoTCore(iotSensorIngestStream.Name);
            
            //Set outputs
            this.IncomingStreamName = iotSensorIngestStream.Name;
            this.EnrichedPressureBucketName = pressureEnrichedDataBucket.BucketName;
            this.EnrichedTemperatureBucketName = temperatureEnrichedDataBucket.BucketName;
            this.RawSensorDataBucketName = sensorRawDataBucket.BucketName;
            this.ReferenceDataBucket = sensorRefDataBucket.BucketName;
            this.RedshiftRoleArn = redshiftRole.Arn;
            this.IoTDevicePem = iotCert.CertificatePem;
            this.IoTPrivateKey = iotCert.PrivateKey;
            this.IoTPublicKey = iotCert.PublicKey;
            this.EnrichedPressureStreamName = enrichedPressureStream.Name;
            this.EnrichedTemperatureStreamName = enrichedTemperatureStream.Name;
        }

        [Output]
        public Output<string> IncomingStreamName { get; set; }
        [Output]
        public Output<string> EnrichedPressureBucketName { get; set; }
        [Output]
        public Output<string> EnrichedTemperatureBucketName { get; set; }
        [Output]
        public Output<string> RawSensorDataBucketName { get; set; }
        [Output]
        public Output<string> ReferenceDataBucket { get; set; }
        [Output]
        public Output<string> RedshiftRoleArn { get; set; }
        [Output]
        public Output<string> IoTDevicePem { get; set; }
        [Output]
        public Output<string> IoTPrivateKey { get; set; }
        [Output]
        public Output<string> IoTPublicKey { get; set; }
        [Output]
        public Output<string> EnrichedPressureStreamName { get; set; }
        [Output]
        public Output<string> EnrichedTemperatureStreamName { get; set; }
    }
}