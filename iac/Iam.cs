using Pulumi.Aws.Iam;
using Pulumi.Aws.Iot;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Iam {
        
        public static Role CreateIotTopicRuleRole() {
            
            var role = new Role($"{Common.appName}-iot-topic-rule-role", new RoleArgs
            {
                AssumeRolePolicy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Action"": ""sts:AssumeRole"",
                            ""Principal"": {
                                ""Service"": ""iot.amazonaws.com""
                            },
                            ""Effect"": ""Allow"",
                            ""Sid"": """"
                        }
                    ]
                }",
                Tags = Common.tags
            });

            var policy = new RolePolicy($"{Common.appName}-iot-topic-rule-role-policy", new RolePolicyArgs
            {
                Role = role.Id,  
                Policy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""kinesis:*""
                        ],
                        ""Resource"": ""arn:aws:kinesis:*""
                    }]
                }"
            });


            return role;
        }

        public static Role CreateIotThingRole() {
            
            var role = new Role($"{Common.appName}-iot-thing-role", new RoleArgs
            {
                AssumeRolePolicy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Action"": ""sts:AssumeRole"",
                            ""Principal"": {
                                ""Service"": ""iot.amazonaws.com""
                            },
                            ""Effect"": ""Allow"",
                            ""Sid"": """"
                        }
                    ]
                }",
                Tags = Common.tags
            });

            var policy = new RolePolicy($"{Common.appName}-iot-thing-role-policy", new RolePolicyArgs
            {
                Role = role.Id,  
                Policy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""iot:Connect""
                        ],
                        ""Resource"": ""arn:aws:iot:*:client/*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""iot:Public""
                        ],
                        ""Resource"": ""arn:aws:iot:*:topic/*""
                    }]
                }"
            });


            return role;
        }
    }


    
}