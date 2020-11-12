import sys
import time
import math
import IMU
import datetime
import os

######JON
import paho.mqtt.client as mqtt
import numpy as np
######JON


######JON
i=0
ctr=0
completeSwingI=0
noMotion = True
downSwing = False
backswing = False
completeSwing = False

x=list()
powerList = []
List =[]
List1 = []
List2 = []
List3 = []
List4 = []
List5 = []
List6 = []
List7 = []
######JON


################### MQTT ###################

def on_connect(client, userdata, flags, rc):
    print("Connection returned result: "+str(rc))
    # Subscribing in on_connect() means that if we lose the connection and
    # reconnect then subscriptions will be renewed.
    # client.subscribe("ece180d/test")
    
def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

def on_message(client, userdata, message):
    print('Received message: "' + str(message.payload) + '" on topic "' + message.topic + '" with QoS ' + str(message.qos))

# 1. create a client instance.
client = mqtt.Client()
# add additional client options (security, certifications, etc.)
# many default options should be good to start off.
# add callbacks to client.
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message
# 2. connect to a broker using one of the connect*() functions.
client.connect_async('mqtt.eclipse.org')

client.loop_start()
#client.publish('ece180d/test', float(np.random.random(1)), qos=1)

# 6. use disconnect() to disconnect from the broker.
#client.loop_stop()
#client.disconnect()
################### MQTT ###################


################### CLASSIFIER ###################
IMU.detectIMU()     #Detect if BerryIMU is connected.
if(IMU.BerryIMUversion == 99):
    print(" No BerryIMU found... exiting ")
    sys.exit()
IMU.initIMU()       #Initialise the accelerometer, gyroscope and compass


while i<150:
    #Read the accelerometer,gyroscope and magnetometer values
    ACCx = IMU.readACCx()
    ACCy = IMU.readACCy()
    ACCz = IMU.readACCz()
    GYRx = IMU.readGYRx()
    GYRy = IMU.readGYRy()
    GYRz = IMU.readGYRz()
    MAGx = IMU.readMAGx()
    MAGy = IMU.readMAGy()
    MAGz = IMU.readMAGz()


######JON
    if(i==25):
        restingPlace = myACC
        print("RESTING VALUE: " + str(restingPlace))

    myACC = IMU.readACCx()
    
    if(i>=25):
        if(completeSwing==True):
            if(completeSwingI+7 == i):
                print(powerList)
                print("SWING POWER: " + str(max(powerList)))

                client.publish('ece180d/Team5/IMU', max(powerList)), qos=1)

                powerList.clear()
                backSwing = False
                downSwing = False
                completeSwing = False

        if(ACCx<restingPlace+350 and ACCx>restingPlace-350):   # variable threshold value, good for different stances
            print("No Motion Detected")
            noMotion = True
            ctr+=1
            if(ctr>=4):
                backSwing = False
                downSwing = False
                completeSwing = False
                
        else:
            print("Motion Detected: " + str(myACC))
            if(myACC < restingPlace-500):
                print("BACK SWING DETECTED!")
                backSwing = True
                downSwing = False
                noMotion = False
                ctr=0
            elif(myACC > restingPlace+450):
                print("DOWN SWING DETECTED!")
                downSwing = True
                noMotion = False
                ctr=0

        if(completeSwing == True):
            downSwing =False
            backSwing =False

        if(downSwing == True and backSwing == True):
            print("     COMPLETE SWING!!!!!!!!!!!!!")
            completeSwing = True
            downSwing = False
            backSwing = False
            powerList = List[i-5:]
            completeSwingI=i

        if(completeSwing == True):
            powerList.append(round(ACCx,3))

    #slow program down a bit, makes the output more readable
    time.sleep(0.03)
    
    List.append(round(ACCx,3))
    i+=1
################### CLASSIFIER ###################


################### MQTTEND ###################
client.loop_stop()
client.disconnect()
################### MQTTEND ###################
