# AWS Meetup Group

## AWS Data Pipeline - Part 2
Usage example of some more commonly used AWS data services:
 * Glue
 * Redshift
 * Elasticsearch
 * Athena, again!

### Part 2 Links n Such:
 * Live stream: https://youtu.be/r9xcVWTNV6c
 * Slides: https://speakerdeck.com/kevasync/aws-data-pipeline-part-2

![Pipeline Architecture Diagram](https://github.com/kevasync/aws-meetup-group-data-services/blob/master/imgs/arch-diagram-part2.png "Part 2 Pipeline Architecture Diagram")


## AWS Data Pipeline - Part 1
Usage example of some commonly used AWS data services:
 * Kinesis Data Streams
 * Kinesis Firehose Delivery Stream
 * Kinesis Analytics Applications
 * S3
 * Athena

### Part 1 Links n Such:
 * Live stream: https://youtu.be/Nm_GWcHgsno
 * Slides: https://speakerdeck.com/kevasync/aws-data-pipelines-part-1

![Part 1 Pipeline Architecture Diagram](https://github.com/kevasync/aws-meetup-group-data-services/blob/master/imgs/arch-diagram.png "Pipeline Architecture Diagram")


## Getting Started
Prerequisites:
 * Active AWS account w/ [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) and [CLI access configured](https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-configure.html)
 * [jq](https://stedolan.github.io/jq/download/)
 * [python3](https://realpython.com/installing-python/) 
 * Active Pulumi account (Can be created using GitHub as login provider)
 * [Pulumi CLI](https://www.pulumi.com/docs/get-started/install/)
 * [.NET Core](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
    * _Note:_ installation via installation binaries may conflict with package managers (e.g. Homebrew), if having issue try uninstalling from package manager and re-installing via installation binary
    * If having issues on Mac, try `dotnet-install.sh` script outlined [here](https://danielhilton.medium.com/how-to-install-multiple-asp-net-core-runtimes-in-macos-717e8d0176ea)

Install Pulumi and deploy:
 * `cd iac`
 * `pulumi login`
 * Set region: `pulumi config set aws:region <value>`
    * Replacing `<value>` with your preferred region
 * `pulumi up`
    * Follow prompts to create a new stack (Or select existing stack if already created)
    *  Name stack `<org>/data-pipeline`
        * Replacing `<org>` with your Pulumi organization name
        * This will be your GitHub user ID if you used it as auth provider
   * Accept prompt to deploy resources. The first deployment will take several minutes:
      ```bash 
      pulumi up
      Previewing update (data-pipeline)

      View Live: https://app.pulumi.com/kevasync/aws-meetup-group-data-services-infra/data-pipeline/previews/d494efb2-6d02-439f-86ef-e5467ced8ef1

         Type                                   Name                                                                         Plan       
      +   pulumi:pulumi:Stack                    aws-meetup-group-data-services-infra-data-pipeline                           create     
      ...   
      +   └─ aws:kinesis:FirehoseDeliveryStream  AWSMeetupGroup_DataServicesDemo_temperature_producer_es                      create     


      Outputs:
         EnrichedPressureBucketName   : "awsmeetupgroup-dataservicesdemo-pressure-enriched-data-1f16ea5"
         ...
         StreamName                   : "aws-meetup-group.iot.sensor-readings.incoming-b58a357"

      Resources:
         + 50 to create

      Do you want to perform this update? yes
      Updating (data-pipeline)

      View Live: https://app.pulumi.com/kevasync/aws-meetup-group-data-services-infra/data-pipeline/updates/1

         Type                                   Name                                                                         Status              Info
      +   pulumi:pulumi:Stack                    aws-meetup-group-data-services-infra-data-pipeline                           creating (135s)     'dotnet build -nologo .' completed successfully
      +   ├─ aws:glue:CatalogDatabase            AWSMeetupGroup_DataServicesDemo_glue_schema_database                         created (1s)        
      ...
      +   ├─ aws:kinesis:Stream                  aws-meetup-group.pressure.enirched                                           created (23s)       
      ```
   * _Note:_ you may see error waiting for Redshift Serverless Workgroup to become available:
      ```bash
      aws:redshiftserverless:Workgroup (awsmeetupgroup-dataservicesdemo-redshift-workgroup):
      error: 1 error occurred:
         * creating urn:pulumi:data-pipeline::aws-meetup-group-data-services-infra::aws:redshiftserverless/workgroup:Workgroup::awsmeetupgroup-dataservicesdemo-redshift-workgroup: 1 error occurred:
         * waiting for Redshift Serverless Workgroup (awsmeetupgroup-dataservicesdemo-redshift-workgroup) to be created: timeout while waiting for state to become 'AVAILABLE' (last state: 'CREATING', timeout: 10m0s)
      ```
      * Run `pulumi up -y` again if so
   

Setup Redshift Stream Ingestion

* Using RedshiftRoleArn from Pulumi output, navigate to Redshift Query Editor for newly created workgroup and execute query to create external schema for Kinesis:
   ```sql
   CREATE EXTERNAL SCHEMA kds
      FROM KINESIS
      IAM_ROLE 'yourRoleArn';
   ```
* Create Materialized views for raw and enriched Kinesis Streams (Find stream names in `IncomingStreamName`, `EnrichedPressureStreamName`, and `EnrichedTemperatureStreamName` Pulumi stack outputs)
   ```sql
   CREATE MATERIALIZED VIEW raw_view AUTO REFRESH YES AS
      SELECT 
         approximate_arrival_timestamp,
         JSON_PARSE(kinesis_data) as Data
      FROM kds."yourIncomingStreamName"
      WHERE CAN_JSON_PARSE(kinesis_data);

   CREATE MATERIALIZED VIEW pressure_view AUTO REFRESH YES AS
      SELECT 
         approximate_arrival_timestamp,
         JSON_PARSE(kinesis_data) as Data
      FROM kds."yourEnrichedPressureStreamName"
      WHERE CAN_JSON_PARSE(kinesis_data);

   CREATE MATERIALIZED VIEW temperature_view AUTO REFRESH YES AS
      SELECT 
         approximate_arrival_timestamp,
         JSON_PARSE(kinesis_data) as Data
      FROM kds."yourEnrichedTemperatureStreamName"
      WHERE CAN_JSON_PARSE(kinesis_data);
   ```
   * _Note:_ stream names are enclosed in _double-quotes_ as hyphens in names are not valid otherwise
   * To test, set `STREAM_NAME` environment variable using `IncomingStreamName` output and run [sample data producer](./kinesis-producer/produce.py)
      * Let run for a few seconds and hit `ctrl-c` to exit
   * Back in the Redshift Query Editor, execute the following to view data from topic:
         ```sql
         REFRESH MATERIALIZED VIEW raw_view;
         select * from raw_view;
         ```
      ![Raw stream Redshift results](./imgs/raw_stream_redshift_results.png)
   
Setup Analytics Applications Reference Data
* _(Optional)_ Ensure you have changed directory to `iac`: `cd iac`
* Upload reference data
   * Get name of reference data bucket and upload the [altitude.csv](./iac/reference-data/altitude.csv) and [weather.csv](./iac/reference-data/weather.csv) reference data files:
      ```bash
      export REFERENCE_BUCKET="$(pulumi stack output --json | jq -r .ReferenceDataBucket)"
      echo "Reference data bucket name: ${REFERENCE_BUCKET}"
      aws s3 cp reference-data/altitude.csv "s3://${REFERENCE_BUCKET}"
      aws s3 cp reference-data/weather.csv "s3://${REFERENCE_BUCKET}"
      ```

Setup Athena
* Set environment variables and create Athena tables:
   ```bash
   export ATHENA_DB_NAME="$(pulumi stack output --json | jq -r .AthenaTableName)"
   export WORKGROUP_NAME=$(aws athena list-work-groups --output json | jq -r '.WorkGroups[0].Name') 
   export DATA_CATALOG_NAME=$(aws athena list-data-catalogs --output json | jq -r '.DataCatalogsSummary[0].CatalogName') 
   export ENRICHED_TEMPERATURE_BUCKET_NAME=$(pulumi stack output --json | jq -r .EnrichedTemperatureBucketName)
   export ENRICHED_PRESSURE_BUCKET_NAME=$(pulumi stack output --json | jq -r .EnrichedPressureBucketName)
   export ATHENA_RESULTS_BUCKET=$(pulumi stack output --json | jq -r .AthenaResultBucketName)

   echo "Athena database name: ${ATHENA_DB_NAME}"
   echo "Athena workgroup name: ${WORKGROUP_NAME}"
   echo "Athena data catalog name: ${DATA_CATALOG_NAME}"
   echo "Enriched temperature s3 bucket: ${ENRICHED_TEMPERATURE_BUCKET_NAME}" 
   echo "Enriched pressure s3 bucket: ${ENRICHED_PRESSURE_BUCKET_NAME}" 
   echo "Athena results s3 bucket: ${ATHENA_RESULTS_BUCKET}" 
   
   # One-time step to set Athena output S3 location
   aws athena update-work-group --work-group "${WORKGROUP_NAME}" --configuration-updates "ResultConfigurationUpdates={OutputLocation=s3://${ATHENA_RESULTS_BUCKET}/}"

   # Create Athena temperature table
   aws athena start-query-execution \
    --query-string "CREATE EXTERNAL TABLE IF NOT EXISTS temperature (site_id string,sensor_reading_value string,reading_timestamp string,outside_temperature string) ROW FORMAT SERDE 'org.apache.hadoop.hive.ql.io.parquet.serde.ParquetHiveSerDe' WITH SERDEPROPERTIES ('serialization.format' = '1') LOCATION 's3://${ENRICHED_TEMPERATURE_BUCKET_NAME}/' TBLPROPERTIES ('has_encrypted_data'='false');" \
    --work-group "${WORKGROUP_NAME}" \
    --query-execution-context "Database=${ATHENA_DB_NAME},Catalog=${DATA_CATALOG_NAME}"

   # Create Athena pressure table
   aws athena start-query-execution \
    --query-string "CREATE EXTERNAL TABLE IF NOT EXISTS pressure (site_id string,sensor_reading_value string,reading_timestamp string,altitude string) ROW FORMAT SERDE 'org.apache.hadoop.hive.ql.io.parquet.serde.ParquetHiveSerDe' WITH SERDEPROPERTIES ('serialization.format' = '1') LOCATION 's3://${ENRICHED_PRESSURE_BUCKET_NAME}/' TBLPROPERTIES ('has_encrypted_data'='false');" \
    --work-group "${WORKGROUP_NAME}" \
    --query-execution-context "Database=${ATHENA_DB_NAME},Catalog=${DATA_CATALOG_NAME}"
    
   # ensure tables were created
   aws athena list-table-metadata --database-name "${ATHENA_DB_NAME}" --catalog-name "${DATA_CATALOG_NAME}" --output json | jq

   ## If you are curious and want to view results from the individual queries, use QueryExecutionId attribute start-execution-query output:
   # aws athena get-query-results --query-execution-id <your_QueryExecutionId>
   ```

Produce Data to Raw Sensor Data Topic:

Let's send some data through the data pipeline and take a look at it action:
* While still in the [iac](./iac/) directory, set the incoming data topic name:
   ```bash
   export INCOMING_STREAM_NAME=$(pulumi stack output --json | jq -r .IncomingStreamName)
   echo "Incoming topic name: ${INCOMING_STREAM_NAME}"
   ```
* Change to the producer directory, set the region, install dependencies, run the [producer](./kinesis-producer/produce.py) for a few seconds:
   ```bash
   cd ../kinesis-producer
   export REGION="us-west-2"
   pip3 install -r requirements.txt
   python3 produce.py
   ```
* 100 messages will be produced to the incoming steam"
   ```bash
   $ python3 produce.py
   Produced record 0 on shard shardId-000000000000: {"SITE_ID":"8","SENSOR_TYPE":"TEMPERATURE","SENSOR_READING_VALUE":"133.85614595369273","READING_TIMESTAMP":"03/03/2023 16:00:01"}
   Produced record 1 on shard shardId-000000000000: {"SITE_ID":"5","SENSOR_TYPE":"PRESSURE","SENSOR_READING_VALUE":"131.84987987949546","READING_TIMESTAMP":"03/03/2023 16:00:01"}
   Produced record 2 on shard shardId-000000000000: {"SITE_ID":"9","SENSOR_TYPE":"TEMPERATURE","SENSOR_READING_VALUE":"191.12123844044754","READING_TIMESTAMP":"03/03/2023 16:00:01"}
   Produced record 3 on shard shardId-000000000000: {"SITE_ID":"11","SENSOR_TYPE":"PRESSURE","SENSOR_READING_VALUE":"203.0377145771329","READING_TIMESTAMP":"03/03/2023 16:00:01"}
   Produced record 4 on shard shardId-000000000000: {"SITE_ID":"7","SENSOR_TYPE":"TEMPERATURE","SENSOR_READING_VALUE":"155.52617270808798","READING_TIMESTAMP":"03/03/2023 16:00:01"}
   Produced record 5 on shard shardId-000000000000: {"SITE_ID":"5","SENSOR_TYPE":"PRESSURE","SENSOR_READING_VALUE":"121.79389050717671","READING_TIMESTAMP":"03/03/2023 16:00:01"}
   Produced record 6 on shard shardId-000000000000: {"SITE_ID":"3","SENSOR_TYPE":"TEMPERATURE","SENSOR_READING_VALUE":"99.20502645042536","READING_TIMESTAMP":"03/03/2023 16:00:02"}
   Produced record 7 on shard shardId-000000000000: {"SITE_ID":"10","SENSOR_TYPE":"PRESSURE","SENSOR_READING_VALUE":"136.0427773999524","READING_TIMESTAMP":"03/03/2023 16:00:02"}
   Produced record 8 on shard shardId-000000000000: {"SITE_ID":"10","SENSOR_TYPE":"TEMPERATURE","SENSOR_READING_VALUE":"56.877663742741014","READING_TIMESTAMP":"03/03/2023 16:00:02"}
   Produced record 9 on shard shardId-000000000000: {"SITE_ID":"6","SENSOR_TYPE":"PRESSURE","SENSOR_READING_VALUE":"126.38795749986275","READING_TIMESTAMP":"03/03/2023 16:00:02"}
   ...
   ```


