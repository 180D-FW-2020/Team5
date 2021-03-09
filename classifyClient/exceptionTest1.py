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


    while i<600:
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

                    #client.publish(topicName, playerName + ",classifierData," + str(max(powerList)), qos=1)
                    powerList.clear()
                    backSwing = False
                    downSwing = False
                    completeSwing = False
                    #return

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

    print("COMPLETE SWING NOT DETECTED")
    #client.publish(topicName, playerName + ",classifierData,0", qos=1)
    print("Your Turn Was Skipped Due To Inactivity")
################### CLASSIFIER ###################


callClassifier()
