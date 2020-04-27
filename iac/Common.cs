using Pulumi;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    public static class Common {
        public static string appName = "AWSMeetupGroup_DataServicesDemo";
        public static InputMap<object> tags = new InputMap<object>{
            { "app-name", appName },
            { "owner", "tinnk" }
        };
    }
}