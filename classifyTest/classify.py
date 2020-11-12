#!/usr/bin/python
#
#    This program  reads the angles from the acceleromteer, gyroscope
#    and mangnetometer on a BerryIMU connected to a Raspberry Pi.
#
#    This program includes two filters (low pass and median) to improve the
#    values returned from BerryIMU by reducing noise.
#
#    The BerryIMUv1, BerryIMUv2 and BerryIMUv3 are supported
#
#    This script is python 2.7 and 3 compatible
#
#    Feel free to do whatever you like with this code.
#    Distributed as-is; no warranty is given.
#
#    http://ozzmaker.com/



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


IMU.detectIMU()     #Detect if BerryIMU is connected.
if(IMU.BerryIMUversion == 99):
    print(" No BerryIMU found... exiting ")
    sys.exit()
IMU.initIMU()       #Initialise the accelerometer, gyroscope and compass


while i<200:

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

    ##################### END Tilt Compensation ########################


######JON
    if(i==25):
        restingPlace = myACC
        print("RESTING VALUE: " + str(restingPlace))

    myACC = IMU.readACCx()
    
    if(i>=25):
        if(ACCx<restingPlace+350 and ACCx>restingPlace-350):   # variable threshold value, good for different stances
            print("No Motion Detected")
            noMotion = True
            ctr+=1
            if(ctr>=4):
                backSwing = False
                downSwing = False
                completeSwing = False
                if(len(powerList) != 0):
                    print(powerList)
                    print("SWING POWER: " + str(max(powerList)))
                    powerList.clear()
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

        if(downSwing == True and backSwing == True):
            print("     COMPLETE SWING!!!!!!!!!!!!!")
            completeSwing = True
            downSwing = False
            backSwing = False
            powerList = List[i-5:]
            #print(myList)

        if(completeSwing == True):
            powerList.append(round(ACCx,3))

    #slow program down a bit, makes the output more readable
    time.sleep(0.03)

######JON
    
    List.append(round(ACCx,3))
    i+=1

######JON
