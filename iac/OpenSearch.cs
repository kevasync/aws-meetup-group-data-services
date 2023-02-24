using Pulumi.Aws.OpenSearch;
using Pulumi.Aws.OpenSearch.Inputs;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class OpenSearch{
        public static Domain CreateSearchDomain(string domainName) {
            return new Domain($"{Common.appName}-{domainName}", new DomainArgs() {
                DomainName = domainName,
                ClusterConfig = new DomainClusterConfigArgs() {
                    InstanceType = "t3.small.search"
                },
                EbsOptions = new DomainEbsOptionsArgs() {
                    EbsEnabled = true,
                    VolumeSize = 100

                },
                Tags = Common.tags
            }); 
        }
    }
}