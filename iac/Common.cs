using Pulumi;

namespace AwsMeetupGroup.DataServices.Infrastructure {
    public static class Common {
        public static string appName = "AWSMeetupGroup_DataServicesDemo";
        public static InputMap<string> tags = new InputMap<string>{
            { "app-name", appName },
            { "owner", "tinnk" }
        };
    }
}