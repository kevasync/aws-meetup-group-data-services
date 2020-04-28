
using System.Collections.Generic;
using Pulumi;

namespace AwsMeetupGroup.DataServices.Infrastructure { 

    class AnalyticsAppS3EnrichmentArgs {

        public AnalyticsAppS3EnrichmentArgs(Output<string> referenceBucketArn, string code, Output<string> streamArn,
                                            Output<string> outputFirehoseArn, Output<string> roleArn, string namePrefix,
                                            List<string> streamInputColumns, string outputFormat,
                                            string referenceFileKey, List<string> referenceFileColumns) {
            ReferenceBucketArn = referenceBucketArn;
            Code = code;
            StreamArn = streamArn;
            OutputFirehoseArn = outputFirehoseArn;
            RoleArn = roleArn;
            NamePrefix = namePrefix;
            StreamFileColumns = streamInputColumns;
            OutputFormat = outputFormat;
            ReferenceFileKey = referenceFileKey;
            ReferenceFileColumns = referenceFileColumns;
        }

        public Output<string> ReferenceBucketArn { get; set; }
        public string Code { get; set; }
        public Output<string> StreamArn { get; set; }
        public Output<string> OutputFirehoseArn { get; set; }
        public Output<string> RoleArn { get; set; }
        public string NamePrefix { get; set; }
        public List<string> StreamFileColumns { get; set; }
        public string OutputFormat { get; set; }
        public string ReferenceFileKey { get; set; }
        public List<string> ReferenceFileColumns { get; set; }
        
    }
}