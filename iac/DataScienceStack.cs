using Pulumi;
using Pulumi.Aws.Kinesis;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    class DataScienceStack : Stack
    {
        public DataScienceStack()
        {
            var iotSensorIngestStream = new Stream("aws-meetup-group.iot.sensor-readings.incoming", new StreamArgs {
                RetentionPeriod = 48,
                ShardCount = 1,
                Tags = Common.tags
            });

            var iotCert = IoT.createIoT(iotSensorIngestStream.Name);
            
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
    }
}