import time
import importTest
import classifyMqtt2

import paho.mqtt.client as mqtt

message1="haha"

def on_connect(client, userdata, flags, rc):
    print("Connection returned result: "+str(rc))
    client.subscribe("ece180d/test", qos=1)

def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

def on_message(client, userdata, message):
    print('Received message: "' + str(message.payload) + '" on topic "' + message.topic + '" with QoS ' + str(message.qos))
    classifyMqtt2.callClassifier()
    #importTest.callFile()
    #message1 = str(message.payload)

client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message

client.connect_async('mqtt.eclipse.org')

client.loop_start()


while True:
    pass
    #print('jonjon')
    #print(message1)
    #importTest.callFile()
    
client.loop_stop()
client.disconnect()  
