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
import glob
import csv

print('Enter The Lobby Code:')
topicName = input()
print('Enter Your Nickname:')
playerName = input()


def clearFiles():
    files = glob.glob('C:/Users/jonry/Desktop/testpystuff/**/*.json', recursive=True) # insert path to the json directories, keep the /**/*.json 

    for f in files:
        try:
            os.remove(f)
        except OSError as e:
            print("Error: %s : %s" % (f, e.strerror))
            
def clearCSV():
    os.remove('TestData.csv') #rename to correct csv file name
    with open('TestData.csv', 'w') as my_empty_csv: #rename to correct csv file name
        pass


def callPose():
    model = load_model('ActRecognition.h5')
    cap = cv2.VideoCapture(0,cv2.CAP_DSHOW)
    iter=0
    while True:
        ret, img = cap.read()
        data1 = os.listdir('C:/users/stink/180da/ActivityRecognition/RealTimeOutput')
        csvFile = 'C:/users/stink/180da/activityrecognition/TestData.csv'
        dataset = [[] for i in range(len(data1))]
        datasetFinal = []
        
        makeData(data1,dataset,datasetFinal,csvFile)
        testData = pd.read_csv(r'C:\Users\stink\180DA\ActivityRecognition\TestData.csv')
        testDataArray = testData.to_numpy()
        testDataFinal = np.reshape(testDataArray,(len(testDataArray),75,1))
        prediction = model.predict(testDataFinal)
        # Decide whether to transmit over MQTT 
        for i in range(iter+10,len(prediction)):
            spredict = prediction[i][1]
            if spredict > 1e-11 and spredict != 0: 
                ###MQTT CODE HERE 
                client.publish(topicName, playerName + ",poseOK", qos=1)
                print('Detected Pose')
                return
        iter+=1
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()
    
    
################### MQTT ###################
def on_connect(client, userdata, flags, rc):
    print("Connection returned result: "+str(rc))
    client.subscribe(topicName, qos=1)
    #client.publish(topicName, "playerName," + playerName, qos=1)

def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

def on_message(client, userdata, message):
    print('Received message: "' + str(message.payload) + '" on topic "' + message.topic + '" with QoS ' + str(message.qos))
    print(message.payload.decode("UTF-8"))
    client.message = message.payload.decode("UTF-8")
    

client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message
client.message = ""

#client.connect_async('mqtt.eclipse.org')
client.connect_async('broker.hivemq.com')

client.loop_start()


while True:
    #print(client.message)
    if (client.message == playerName + ",startPose"):
        clearFiles()
        clearCSV()
        callPose()
    pass
    
client.loop_stop()
client.disconnect()  
################### MQTT ###################
