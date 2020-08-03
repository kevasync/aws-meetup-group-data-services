from AWSIoTPythonSDK.MQTTLib import AWSIoTMQTTClient
import logging
import time
import argparse
import json
import calendar
import sys
import random
if (len(sys.argv) != 2):
    print('Provide shadow interaction endpoint url as only argument')
    quit()

host = sys.argv[1]
certPath = "/home/pi/"
clientId = 1
topic = "sensor-topic"

# Init AWSIoTMQTTClient
myAWSIoTMQTTClient = None
myAWSIoTMQTTClient = AWSIoTMQTTClient("sensor-site-%s" % clientId)
myAWSIoTMQTTClient.configureEndpoint(host, 8883)
myAWSIoTMQTTClient.configureCredentials("{}AmazonRootCA1.pem".format(certPath), "{}private.pem".format(certPath), "{}device.pem.crt".format(certPath))


# AWSIoTMQTTClient connection configuration
myAWSIoTMQTTClient.configureAutoReconnectBackoffTime(1, 32, 20)
myAWSIoTMQTTClient.configureOfflinePublishQueueing(-1)  # Infinite offline Publish queueing
myAWSIoTMQTTClient.configureDrainingFrequency(2)  # Draining: 2 Hz
myAWSIoTMQTTClient.configureConnectDisconnectTimeout(10)  # 10 sec
myAWSIoTMQTTClient.configureMQTTOperationTimeout(5)  # 5 sec
myAWSIoTMQTTClient.connect()

# Publish to the same topic in a loop forever
loopCount = 0
while loopCount < 10:
    message = {}
    message['SITE_ID'] = clientId
    message['SENSOR_TYPE'] = "TEMPERATURE"
    message['SENSOR_READING_VALUE'] = str(random.uniform(50.0,212.0))
    message['READING_TIMESTAMP'] = calendar.timegm(time.gmtime())
    messageJson = json.dumps(message)
    myAWSIoTMQTTClient.publish(topic, messageJson, 1)
    print('Published topic %s: %s\n' % (topic, messageJson))
    loopCount += 1
    time.sleep(1)

myAWSIoTMQTTClient.disconnect()