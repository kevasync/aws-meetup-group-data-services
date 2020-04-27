using Pulumi;
using Pulumi.Aws.Kinesis;
using Pulumi.Aws.Kinesis.Inputs;
using Pulumi.Aws.S3;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    class DataScienceStack : Stack
    {
        public DataScienceStack()
        {
            var sensorTopicName = "aws-meetup-group.iot.sensor-readings.incoming";
            var iotSensorIngestStream = Kinesis.CreateStream(sensorTopicName);
            var iotCert = IoT.createIoT(iotSensorIngestStream.Name);
            var sensorRawDataBucket = S3.CreateS3Bucket($"{Common.appName.Replace("_", "-")}-sensor-raw-data");
            var sensorS3Firehose = Kinesis.CreateS3Firehose($"{Common.appName}_sensor_producer_s3", iotSensorIngestStream.Arn, sensorRawDataBucket.Arn);
            // var analyticsApp = Kinesis.CreateAnalyticsApplication($"{Common.appName}_sensor_data_analytics");

            this.CertPublicKey = iotCert.PublicKey;
            this.CertPrivateKey = iotCert.PrivateKey;
            this.CertPem = iotCert.CertificatePem;
        }

        [Output]
        public Output<string> CertPublicKey { get; set; }
        
        [Output]
        public Output<string> CertPrivateKey { get; set; }
        
        [Output]
        public Output<string> CertPem { get; set; }

        // [Output]
        // public Output<string> SensorTopic{ get; set; }
    }
}