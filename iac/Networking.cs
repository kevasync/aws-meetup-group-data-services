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
        public static Subnet CreateSubnet() {
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
            
            var subnet =  new Subnet ($"{Common.appName}-subnet", new SubnetArgs{
                VpcId = vpc.Id,
                CidrBlock = $"{vpcCidrBase}.0/25",
                Tags = Common.tags
            });

            var rta = new RouteTableAssociation($"{Common.appName}-rta", new RouteTableAssociationArgs() {
                RouteTableId = rt.Id,
                SubnetId = subnet.Id,
            });

            var drt = new DefaultRouteTable($"{Common.appName}-drt", new DefaultRouteTableArgs() {
                DefaultRouteTableId = rt.Id,
                Routes = new InputList<DefaultRouteTableRouteArgs> {
                    new DefaultRouteTableRouteArgs() {CidrBlock = "0.0.0.0/0", GatewayId = ig.Id }
                },
                Tags = Common.tags
            });
            
            return subnet;
        }
    }
}