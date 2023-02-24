using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.RedshiftServerless;



namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Redshift{
        public static Workgroup CreateRedshiftServerless(string databaseName, Output<string> iamRole, List<Subnet> subnets, Output<string> vpcSecurityGroupId) {
            string namespaceName = $"{Common.appName}-redshift-namespace".ToLower().Replace("_", "-"); //valid pattern: ^[a-z0-9-]+$
            var serverlessNamespace = new Namespace(namespaceName, new NamespaceArgs
            {
                NamespaceName = namespaceName,
                AdminUsername = "mainuser",
                AdminUserPassword = "blahBlahBlahBlah1!",
                DbName = databaseName,
                IamRoles = iamRole,
                Tags = Common.tags
            });

            var subnetIds = new InputList<string>();
            subnets.ForEach(x => subnetIds.Add(x.Id));
            
            var workgroupName = $"{Common.appName}-redshift-workgroup".ToLower().Replace("_", "-");
            return new Workgroup(workgroupName, new WorkgroupArgs
            {
                NamespaceName = namespaceName,
                WorkgroupName = workgroupName,
                BaseCapacity = 32, // 32 RPUs to 512 RPUs in units of 8 (32,40,48...512)
                SecurityGroupIds =  new InputList<string> { vpcSecurityGroupId },
                SubnetIds = subnetIds,
                PubliclyAccessible = false,
                Tags = Common.tags
            }, new CustomResourceOptions { DependsOn = { serverlessNamespace } });
        }
    }
}