using Pulumi.Aws.ElasticSearch;
using Pulumi.Aws.ElasticSearch.Inputs;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    static class ElasticSearch{
        public static Domain CreateSearchDomain(string domainName) {
            return new Domain($"{Common.appName}-{domainName}", new DomainArgs() {
                DomainName = domainName,
                ClusterConfig = new DomainClusterConfigArgs() {
                    InstanceType = "r4.large.elasticsearch"
                },
                ElasticsearchVersion = "1.5",
                EbsOptions = new DomainEbsOptionsArgs() {
                    EbsEnabled = true,
                    VolumeSize = 100

                },
                Tags = Common.tags
            }); 
        }
    }
}