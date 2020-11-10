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

RAD_TO_DEG = 57.29578
M_PI = 3.14159265358979323846
G_GAIN = 0.070          # [deg/s/LSB]  If you change the dps for gyro, you need to update this value accordingly
AA =  0.40              # Complementary filter constant
MAG_LPF_FACTOR = 0.4    # Low pass filter constant magnetometer
ACC_LPF_FACTOR = 0.4    # Low pass filter constant for accelerometer
ACC_MEDIANTABLESIZE = 9         # Median filter table size for accelerometer. Higher = smoother but a longer delay
MAG_MEDIANTABLESIZE = 9         # Median filter table size for magnetometer. Higher = smoother but a longer delay

######JON

i=0
downSwing = False
backswing = False

x=list()
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


while i<500:

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
        else:
            print("Motion Detected: " + str(myACC))
            if(myACC < restingPlace-500):
                print("BACK SWING DETECTED!")
                backSwing = True
            elif(myACC > restingPlace+1000):
                print("DOWN SWING DETECTED!")
                downSwing = True

    #slow program down a bit, makes the output more readable
    time.sleep(0.03)

######JON

    List.append(round(AccXangle, 3))
    List1.append(round(AccYangle, 3))
    List2.append(round(gyroXangle, 3))
    List3.append(round(gyroYangle, 3))
    List4.append(round(gyroZangle, 3))
    List5.append(round(ACCx, 3))
    List6.append(round(ACCy, 3))
    List7.append(round(ACCz, 3))

    i+=1

print("X ACC:: ")
print(List5)
######JON
