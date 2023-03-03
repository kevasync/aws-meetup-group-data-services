namespace AwsMeetupGroup.DataServices.Infrastructure { 
   public static class AnalyticsSqlQueries {
       public readonly static string temperatureQuery = @"
        CREATE OR REPLACE STREAM ""TEMPERATURE_ANALYTICS_TOPIC"" (SITE_ID VARCHAR(1024), SENSOR_READING_VALUE VARCHAR(1024), READING_TIMESTAMP VARCHAR(1024), OUTSIDE_TEMPERATURE VARCHAR(1024));
        CREATE OR REPLACE PUMP ""STREAM_PUMP_TEMPERATURE"" AS INSERT INTO ""TEMPERATURE_ANALYTICS_TOPIC""
        SELECT STREAM s.SITE_ID as ""SITE_ID"", s.SENSOR_READING_VALUE as ""SENSOR_READING_VALUE"", s.READING_TIMESTAMP as ""READING_TIMESTAMP"", t.OUTSIDE_TEMPERATURE as ""OUTSIDE_TEMPERATURE""
        FROM ""temperature_001"" s LEFT JOIN ""TEMPERATURE_REFERENCE_TABLE"" t
        ON s.SITE_ID=t.SITE_ID
        WHERE s.SENSOR_TYPE='TEMPERATURE';
       ";   
       public readonly static string pressureQuery = @"
        CREATE OR REPLACE STREAM ""PRESSURE_ANALYTICS_TOPIC"" (SITE_ID VARCHAR(1024), SENSOR_READING_VALUE VARCHAR(1024), READING_TIMESTAMP VARCHAR(1024), ALTITUDE VARCHAR(1024));
        CREATE OR REPLACE PUMP ""STREAM_PUMP_PRESSURE"" AS INSERT INTO ""PRESSURE_ANALYTICS_TOPIC""
        SELECT STREAM s.SITE_ID as ""SITE_ID"", s.SENSOR_READING_VALUE as ""SENSOR_READING_VALUE"", s.READING_TIMESTAMP as ""READING_TIMESTAMP"", t.ALTITUDE as ""ALTITUDE""
        FROM ""pressure_001"" s LEFT JOIN ""PRESSURE_REFERENCE_TABLE"" t
        ON s.SITE_ID=t.SITE_ID
        WHERE s.SENSOR_TYPE='PRESSURE';
       ";
   }
}

//Athena table creation queries:
// CREATE EXTERNAL TABLE IF NOT EXISTS pressure (
//   site_id string,
//   sensor_reading_value string,
//   reading_timestamp string,
//   altitude string 
// )
// ROW FORMAT SERDE 'org.apache.hadoop.hive.ql.io.parquet.serde.ParquetHiveSerDe'
// WITH SERDEPROPERTIES (
//   'serialization.format' = '1'
// ) LOCATION 's3://awsmeetupgroup-dataservicesdemo-pressure-enriched-data-6be57cf/'
// TBLPROPERTIES ('has_encrypted_data'='false');


// CREATE EXTERNAL TABLE IF NOT EXISTS temperature (
//   `site_id` string,
//   `sensor_reading_value` string,
//   `reading_timestamp` string,
//   `outside_temperature` string 
// )
// ROW FORMAT SERDE 'org.apache.hadoop.hive.ql.io.parquet.serde.ParquetHiveSerDe'
// WITH SERDEPROPERTIES (
//   'serialization.format' = '1'
// ) LOCATION 's3://awsmeetupgroup-dataservicesdemo-temp-enriched-data-01b83c3/'
// TBLPROPERTIES ('has_encrypted_data'='false');

// to curl an index:
//  * allow open access to the domain access in portal
//  * `curl -X GET https://search-meetup-search-domain-2n77ciuknd4elca3nwlbs2hoky.us-west-2.es.amazonaws.com/pressure-2020-05-27/_search\?q=SITE_ID:3 | jq .`



//spectrum:
//CREATE External schema and table

// drop external schema spectrum_schema;

// create external schema spectrum_schema from data catalog 
// database 'spectrum_db' 
// iam_role 'arn:aws:iam::<account>:role/AWSMeetupGroup_DataServicesDemo-redshift-role-3f9603a'
// region 'us-west-2'
// create external database if not exists;



// CREATE  external table spectrum_schema.pressure ( 
//   `SITE_ID` VARCHAR(128),
//   `SENSOR_READING_VALUE` VARCHAR(128),
//   `READING_TIMESTAMP` VARCHAR(128),
//   `ALTITUDE` VARCHAR(128) 
//   )
// stored as PARQUET
// location 's3://awsmeetupgroup-dataservicesdemo-pressure-enriched-data-cc612db/';

// https://aws.amazon.com/blogs/big-data/10-best-practices-for-amazon-redshift-spectrum/



// create table public.siteinfo (
// 	siteId varchar(128),
// 	siteName varchar(128)
// );

// insert into public.siteinfo values ('5', 'Charleston');

// select * from  spectrum_schema.pressure p left join public.siteinfo i on p.site_id = i.siteId;