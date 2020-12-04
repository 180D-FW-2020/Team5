import sys
import time
import math
import IMU
import datetime
import os
import paho.mqtt.client as mqtt
import numpy as np
import gpiozero


def callButtons():
    # To recognize gpiozero commands, install gpiozero
    # with the command 'pip3 install gpiozero'
    left_flag = False
    right_flag = False
    run_sequence = True

    left_button = gpiozero.DigitalInputDevice(17)
    right_button = gpiozero.DigitalInputDevice(27)

    while run_sequence == True:

        if left_button.value == 1 and left_flag == True and right_button.value == 1 and right_flag == True:
            print("Direction Control Period Completed")
            run_sequence = False
            return

        elif left_button.value  == 1 and  left_flag == False:
            print("Left Button Pressed\n")
            left_flag = True
            
        elif left_button.value == 0 and left_flag == True:
            print("Left Button Released\n")
            left_flag = False

        elif right_button.value == 1 and right_flag == False:
            print("Right Button Pressed\n")
            right_flag = True
            
        elif right_button.value == 0 and right_flag == True:
            print("Right Button Released\n")
            right_flag = False


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
        if(i==40):
            restingPlace = myACC
            print("RESTING VALUE: " + str(restingPlace))

        myACC = IMU.readACCx()

        if(i>=40):
            if(completeSwing==True):
                if(completeSwingI+7 == i):
                    print(powerList)
                    print("SWING POWER: " + str(max(powerList)))

                    client.publish('ece180da_team5', "classifierData," + str(max(powerList)), qos=1)
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
################### CLASSIFIER ###################


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
    if(message.payload=="startClassifier"):
        callClassifier()
    if(message.payload=="startButtons"):
        callButtons()
    

client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message

client.connect_async('mqtt.eclipse.org')

client.loop_start()


while True:
    pass
    
client.loop_stop()
client.disconnect()  
################### MQTT ###################

