using System.Threading.Tasks;
using Pulumi;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    class Program
    {
        static Task<int> Main() => Deployment.RunAsync<DataScienceStack>();
  
    }
}