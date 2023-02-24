using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;

namespace AwsMeetupGroup.DataServices.Infrastructure {

    
    static class Networking {
        private const string vpcCidrBase =  "192.168.210";

        public static SecurityGroup CreateSecurityGroup(Input<string> vpcId) {
            return new SecurityGroup($"{Common.appName}-vpc-secgroup", new SecurityGroupArgs{
                Egress = new InputList<SecurityGroupEgressArgs> {
                    new SecurityGroupEgressArgs {
                        CidrBlocks = new InputList<string> { "0.0.0.0/0" },
                        FromPort = 0,
                        ToPort = 0,
                        Protocol = "-1"
                    }
                },
                Ingress = new InputList<SecurityGroupIngressArgs> {
                    new SecurityGroupIngressArgs {
                        CidrBlocks = new InputList<string> { "0.0.0.0/0" },
                        FromPort = 5439,
                        ToPort = 5439,
                        Protocol = "tcp"
                    }
                },
                Tags = Common.tags,
                VpcId = vpcId
            });
        }
        public static List<Subnet> CreateSubnets() {
            var vpc =  new Vpc($"{Common.appName}-vpc", new VpcArgs() {
                EnableDnsHostnames = true, 
                EnableDnsSupport = true,
                CidrBlock = $"{vpcCidrBase}.0/24",
                Tags = Common.tags
            }); 

            var ig = new InternetGateway($"{Common.appName}-igw", new InternetGatewayArgs() {
                VpcId = vpc.Id,
                Tags = Common.tags
            });

            var rt = new RouteTable($"{Common.appName}-rt", new RouteTableArgs() {
                VpcId = vpc.Id,
                Tags = Common.tags
            });
            
            var subnetCIDRRanges = new List<string>() {$"{vpcCidrBase}.0/26", $"{vpcCidrBase}.64/26", $"{vpcCidrBase}.128/26"};
            var availabilityZones = new List<string>() { "a", "b", "c" };
            
            var awsConfig = new Pulumi.Config("aws");
            var awsRegion = awsConfig.Require("region");

            var subnets = subnetCIDRRanges.Select((cidr, idx) => {
                var subnet = new Subnet ($"{Common.appName}-subnet-{idx}", new SubnetArgs{
                    VpcId = vpc.Id,
                    CidrBlock = cidr,
                    AvailabilityZone = $"{awsRegion}{availabilityZones[idx]}",
                    Tags = Common.tags
                });

                new RouteTableAssociation($"{Common.appName}-rta-{idx}", new RouteTableAssociationArgs() {
                    RouteTableId = rt.Id,
                    SubnetId = subnet.Id,
                });
                
                return subnet;
            }).ToList();

            var drt = new DefaultRouteTable($"{Common.appName}-drt", new DefaultRouteTableArgs() {
                DefaultRouteTableId = rt.Id,
                Routes = new InputList<DefaultRouteTableRouteArgs> {
                    new DefaultRouteTableRouteArgs() {CidrBlock = "0.0.0.0/0", GatewayId = ig.Id }
                },
                Tags = Common.tags
            });
            
            return subnets;
        }
    }
}