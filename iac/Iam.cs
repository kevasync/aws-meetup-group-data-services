using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iot;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Iam {
        
        public static Role CreateRedshiftRole() {
            var role = new Role($"{Common.appName}-redshift-role", new RoleArgs
            {
                AssumeRolePolicy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Action"": ""sts:AssumeRole"",
                            ""Principal"": {
                                ""Service"": ""redshift.amazonaws.com""
                            },
                            ""Effect"": ""Allow"",
                            ""Sid"": """"
                        }
                    ]
                }",
                Tags = Common.tags
            });
            var policy = new RolePolicy($"{Common.appName}-redshift-role-policy", new RolePolicyArgs
            {
                Role = role.Id,  
                Policy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""s3:*""
                        ],
                        ""Resource"": ""*""
                    }]
                }"
            });

            return role;
        }

         

        public static Role CreateKinesisAnalyticsAppRole() {
            
            var role = new Role($"{Common.appName}-analytics-app-role", new RoleArgs
            {
                AssumeRolePolicy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Action"": ""sts:AssumeRole"",
                            ""Principal"": {
                                ""Service"": ""kinesisanalytics.amazonaws.com""
                            },
                            ""Effect"": ""Allow"",
                            ""Sid"": """"
                        }
                    ]
                }",
                Tags = Common.tags
            });
            var policy = new RolePolicy($"{Common.appName}-analytics-app-role-policy", new RolePolicyArgs
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
                        ""Resource"": ""*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""firehose:*""
                        ],
                        ""Resource"": ""*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""cloudwatch:*""
                        ],
                        ""Resource"": ""*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""s3:*""
                        ],
                        ""Resource"": ""arn:aws:s3:::*""
                    }]
                }"
            });

            return role;
        }
         public static Role CreateFirehoseRole() {
            
            var role = new Role($"{Common.appName}-firehose-role", new RoleArgs
            {
                AssumeRolePolicy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Action"": ""sts:AssumeRole"",
                            ""Principal"": {
                                ""Service"": ""firehose.amazonaws.com""
                            },
                            ""Effect"": ""Allow"",
                            ""Sid"": """"
                        }
                    ]
                }",
                Tags = Common.tags
            });
            var policy = new RolePolicy($"{Common.appName}-firehose-role-policy", new RolePolicyArgs
            {
                Role = role.Id,  
                Policy =
                    @"{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""s3:*"" 
                        ],
                        ""Resource"": ""arn:aws:s3:::*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""kinesis:*"" 
                        ],
                        ""Resource"": ""arn:aws:kinesis:*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""es:*"" 
                        ],
                        ""Resource"": ""arn:aws:es:*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""redshift:*"" 
                        ],
                        ""Resource"": ""arn:aws:redshift:*""
                    },{
                        ""Effect"": ""Allow"",
                        ""Action"": [
                            ""glue:*"" 
                        ],
                        ""Resource"": ""*""
                    }]
                }"
            });

            return role;
        }
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