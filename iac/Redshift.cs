using Pulumi;
using Pulumi.Aws.RedShift;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class Redshift{
        public static Cluster CreateRedshiftCluster(string databaseName, Output<string> iamRole, InputList<string> subnets, Output<string> vpcSecurityGroupId) {
            var subnetGroup = new SubnetGroup($"{Common.appName.Replace("_", "-").ToLower()}-subnet-group", new  SubnetGroupArgs(){
                SubnetIds = subnets,
                Tags = Common.tags
            });

            return new Cluster($"{Common.appName}-redshift", new ClusterArgs() {
                ClusterIdentifier = $"{Common.appName.Replace("_", "-")}-redshift-1".ToLower(),
                ClusterType = "single-node",
                DatabaseName = databaseName,
                MasterPassword = "blahBlahBlahBlah1!",
                MasterUsername = "masteruser",
                NodeType = "dc2.large",
                IamRoles = iamRole,
                Tags = Common.tags,
                AutomatedSnapshotRetentionPeriod = 0,
                SkipFinalSnapshot = true,
                ClusterSubnetGroupName = subnetGroup.Name,
                VpcSecurityGroupIds = new InputList<string> { vpcSecurityGroupId }
            });            
        }
    }
}