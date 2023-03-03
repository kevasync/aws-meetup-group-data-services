import boto3, random, os
from datetime import datetime

client = boto3.client('kinesis', os.getenv('REGION'))
streamName = os.getenv('INCOMING_STREAM_NAME')
print(f'Stream name: {streamName}')

my_shard_id = ""
for x in range(100):
	siteId = random.randint(1,11)
	sensorType = 'TEMPERATURE' if x % 2 == 0 else 'PRESSURE'
	message = f'''{{
		"SITE_ID":"{str(siteId)}",
		"SENSOR_TYPE":"{sensorType}",
		"SENSOR_READING_VALUE":"{str(random.uniform(50.0,212.0))}",
		"READING_TIMESTAMP":"{datetime.now().strftime("%d/%m/%Y %H:%M:%S")}"
	}}'''.replace('\n','').replace('\t', '')
	response = client.put_record (
	    StreamName=streamName,
	    Data=message.encode(),
	    PartitionKey='1'
	)
	my_shard_id = response['ShardId']
	print(f'Produced record {str(x)} on shard {my_shard_id}: {message}')

# # Read records from the steam
# my_shard_id = response['ShardId']
# shard_iterator = client.get_shard_iterator(StreamName=streamName, ShardId=my_shard_id, ShardIteratorType='TRIM_HORIZON')
# my_shard_iterator = shard_iterator['ShardIterator']
# record_response = client.get_records(ShardIterator=my_shard_iterator,Limit=2)
# print(record_response)
# while record_response['Records'] != []:
#     record_response = client.get_records(ShardIterator=record_response['NextShardIterator'], Limit=2)
#     print(f"\n**TEST** Read record from stream: {record_response}\n")
