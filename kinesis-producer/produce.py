import boto3
import random
import os

client = boto3.client('kinesis', 'us-west-2')
streamName = os.getenv('STREAM_NAME')#'aws-meetup-group.iot.sensor-readings.incoming-a1aa62d'
print('Stream name:' + streamName)

my_shard_id = ""
for x in range(100):
	siteId = random.randint(1,11)
	sensorType = "TEMPERATURE" if x % 2 == 0 else "PRESSURE"
	message = '{"SITE_ID":"' + str(siteId) + '","SENSOR_TYPE":"' + sensorType + '","SENSOR_READING_VALUE":"' + str(random.uniform(50.0,212.0)) + '","READING_TIMESTAMP":"04/28/2020 17:52:16"}'
	response = client.put_record (
	    StreamName=streamName,
	    Data=message.encode(),
	    PartitionKey="1"
	)
	my_shard_id = response['ShardId']
	print("produced record " + str(x) + " to shard: " + my_shard_id)




my_shard_id = response['ShardId']

shard_iterator = client.get_shard_iterator(StreamName=streamName,
                                                      ShardId=my_shard_id,
                                                      ShardIteratorType='TRIM_HORIZON')

my_shard_iterator = shard_iterator['ShardIterator']

record_response = client.get_records(ShardIterator=my_shard_iterator,
                                              Limit=2)
print(record_response)

while record_response['Records'] != []:
    record_response = client.get_records(ShardIterator=record_response['NextShardIterator'],
                                                  Limit=2)
    print(record_response)
