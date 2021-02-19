import sys
import time
import math
import IMU
import datetime
import os
import paho.mqtt.client as mqtt
import numpy as np
import gpiozero

i=0

print('Enter The Lobby Code:')
topicName = input()
print('Enter Your Nickname:')
playerName = input()
#topicName = str(sys.argv[1])
#playerName = str(sys.argv[2])
            
            

################### CLASSIFIER ###################
def callClassifier():    
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
    
    IMU.detectIMU()     #Detect if BerryIMU is connected.
    if(IMU.BerryIMUversion == 99):
        print(" No BerryIMU found... exiting ")
        sys.exit()
    IMU.initIMU()       #Initialise the accelerometer, gyroscope and compass


    while i<150:
        try:
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

            if(i==2):
                print("CALIBRATING!!! You have 5 seconds until Classification")
                
            if(i<40 and i>15):
                print("CALIBRATING: " + str(myACC))
            
            if(i==40):
                restingPlace = myACC
                print("RESTING VALUE: " + str(restingPlace))

            myACC = IMU.readACCx()

            if(i>=40):
                if(completeSwing==True):
                    if(completeSwingI+7 == i):
                        print(powerList)
                        print("SWING POWER: " + str(max(powerList)))

                        client.publish(topicName, playerName + ",classifierData," + str(max(powerList)), qos=1)
                        powerList.clear()
                        backSwing = False
                        downSwing = False
                        completeSwing = False
                        return

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
        except:
            print("Something Went Wrong, RETRY YOUR SWING")
            pass
    print("COMPLETE SWING NOT DETECTED")
    client.publish(topicName, playerName + ",classifierData,0", qos=1)
    print("Your Turn Was Skipped Due To Inactivity")
################### CLASSIFIER ###################



################### MQTT ###################
def on_connect(client, userdata, flags, rc):
    print("\nConnection returned result: "+str(rc))
    print("Connected To Topic: " + topicName + " As " + playerName + "\n")
    client.subscribe(topicName, qos=1)
    client.publish(topicName, "playerName," + playerName, qos=1)

def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

def on_message(client, userdata, message):
    print('Received message: "' + str(message.payload) + '" on topic "' + message.topic + '" with QoS ' + str(message.qos))
    print(message.payload.decode("UTF-8") + "\n")
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
    if (client.message == playerName + ",startClassifier"):
        callClassifier()
        client.message=""
    pass
    
client.loop_stop()
client.disconnect()  
################### MQTT ###################
