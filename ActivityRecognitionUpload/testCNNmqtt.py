import tensorflow as tf
from tensorflow import keras
import pandas as pd
from keras.models import load_model
import cv2
from FormData import makeData
import numpy as np
import os
import time
import paho.mqtt.client as mqtt


def callPose():
    data1 = os.listdir('C:/users/stink/180da/ActivityRecognition/RealTimeOutput')
    csvFile = 'C:/users/stink/180da/activityrecognition/TestData.csv'
    dataset = [[] for i in range(len(data1))]
    datasetFinal = []

    model = load_model('ActRecognition.h5')
    cap = cv2.VideoCapture(0,cv2.CAP_DSHOW)
    iter=0
    while True:
        ret, img = cap.read()
        makeData(data1,dataset,datasetFinal,csvFile)
        testData = pd.read_csv(r'C:\Users\stink\180DA\ActivityRecognition\TestData.csv')
        testDataArray = testData.to_numpy()
        testDataFinal = np.reshape(testDataArray,(len(testDataArray),75,1))
        prediction = model.predict(testDataFinal)
        # Decide to transmit over MQTT 
        if prediction[0][1] > .01: 
            ###MQTT CODE HERE 
            client.publish('ece180da_team5', "poseOK", qos=1)
            print('Detected Pose')
            return
        iter+=1
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cv2.release()
    cv2.destroyAllWindows()
    
    
################### MQTT ###################
def on_connect(client, userdata, flags, rc):
    print("Connection returned result: "+str(rc))
    client.subscribe("ece180da_team5", qos=1)

def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

def on_message(client, userdata, message):
    print('Received message: "' + str(message.payload) + '" on topic "' + message.topic + '" with QoS ' + str(message.qos))
    print(message.payload.decode("UTF-8"))
    if(message.payload.decode("UTF-8")=="startPose"):
        callPose()
    

client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message

#client.connect_async('mqtt.eclipse.org')
client.connect_async('broker.hivemq.com')

client.loop_start()


while True:
    pass
    
client.loop_stop()
client.disconnect()  
################### MQTT ###################
