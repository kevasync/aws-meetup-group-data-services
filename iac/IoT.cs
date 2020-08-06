using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iot;

using Policy = Pulumi.Aws.Iot.Policy;
using PolicyArgs = Pulumi.Aws.Iot.PolicyArgs;
using PolicyAttachment = Pulumi.Aws.Iot.PolicyAttachment;
using PolicyAttachmentArgs = Pulumi.Aws.Iot.PolicyAttachmentArgs;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class IoT {
        public static Certificate createIoTCore(Output<string> streamName) {
            var topicRuleRole = Iam.CreateIotTopicRuleRole();
            var iotThingPolicy = new Policy($"{Common.appName}_iot_sensor_producer_thing_policy", new PolicyArgs { 
                PolicyDocument = @"{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {
                    ""Action"": [
                        ""iot:*"", 
                        ""  :*""
                    ],
                    ""Effect"": ""Allow"",
                    ""Resource"": ""*""
                    }
                ]
                }"
            });
            
            var iotThingType = new ThingType($"{Common.appName}_raspberryPi");
            var iotThing = new Thing($"{Common.appName}_iot_sensor_producer", new ThingArgs { ThingTypeName = iotThingType.Name });
            var iotCert = new Certificate($"{Common.appName}_iot_sensor_producer_cert", new CertificateArgs { Active = true });

            var iotThingPrincipalAttachement = new ThingPrincipalAttachment($"{Common.appName}_iot_sensor_producer_thing_cert_attachment", new ThingPrincipalAttachmentArgs{
                Principal = iotCert.Arn,
                Thing = iotThing.Name,
            });
            
            var iotPolicyAttachment = new PolicyAttachment($"{Common.appName}_iot_sensor_producer_policy_attachment", new PolicyAttachmentArgs{
                Policy = iotThingPolicy.Name,
                Target = iotCert.Arn
            });
            
            var iotRule = new TopicRule($"{Common.appName}_sensor_ingest_iot_rule", new TopicRuleArgs {
                Enabled = true,
                Kinesis = new Pulumi.Aws.Iot.Inputs.TopicRuleKinesisArgs {
                    PartitionKey = "'SITE_ID'",
                    RoleArn = topicRuleRole.Arn,
                    StreamName = streamName
                },
                Sql = $"SELECT * FROM 'sensor-topic'",
                SqlVersion = "2015-10-08"
            });

            return iotCert;
        }
    }
}