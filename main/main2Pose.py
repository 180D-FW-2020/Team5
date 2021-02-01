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
topicName = str(sys.argv[1])
playerName = str(sys.argv[2])


################### BUTTONS ###################
def callButtons():
    left_press = False
    right_press = False
    left_max = False
    right_max = False
    run_sequence = True

    t_Left_start = 0
    t_Left_end = 0
    t_Left = 0
    t_Right_start = 0
    t_Right_end = 0
    t_Right = 0
    t_Max = 1000        #Maximum Time between ticks in milliseconds

    left_button = gpiozero.DigitalInputDevice(17)
    right_button = gpiozero.DigitalInputDevice(27)

    print("Choose Your Direction! Press both buttons simultaneously to finish\n")

    while run_sequence == True:

        if left_press == True and (int(round(time.time()*1000)) - t_Left_start) > t_Max:
            left_max = True
            
        if right_press == True and (int(round(time.time()*1000)) - t_Right_start) > t_Max:
            right_max = True

        if left_button.value == 1 and left_press == True and right_button.value == 1 and right_press == True:
            client.publish(topicName, playerName + ",endButtons", qos=1)
            print("Direction Control Completed\n")
            run_sequence = False        
            return

        elif left_button.value  == 1 and  left_press == False:
            #print("Left Button Pressed\n")
            left_press = True
            t_Left_start = int(round(time.time()*1000))
            
        elif left_max == True or (left_button.value == 0 and left_press == True):
            left_press = False
            left_max = False
            t_Left_end = int(round(time.time()*1000))
            t_Left = t_Left_end - t_Left_start
            #print("Left Button Released, Time: {}\n".format(t_Left))
            client.publish(topicName, playerName + ",turn,left," + str(t_Left), qos=1)
            print("turn,left,{}\n".format(t_Left))

        elif right_button.value == 1 and right_press == False:
            #print("Right Button Pressed\n")
            right_press = True
            t_Right_start = int(round(time.time()*1000))
            
        elif right_max == True or (right_button.value == 0 and right_press == True):
            right_press = False
            right_max = False
            t_Right_end = int(round(time.time()*1000))
            t_Right = t_Right_end - t_Right_start
            #print("Right Button Released, Time: {}\n".format(t_Right))
            client.publish(topicName, playerName + ",turn,right," + str(t_Right), qos=1)
            print("turn,right,{}\n".format(t_Right))
################### BUTTONS ###################
            
            

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
    print("COMPLETE SWING NOT DETECTED")
    client.publish(topicName, playerName + ",classifierData,0", qos=1)
    print("Your Turn Was Skipped Due To Inactivity")
################### CLASSIFIER ###################


###################### POSE ######################
def callPose():
    print("\nWaiting for Valid Pose...")
    time.sleep(3)
    client.publish(topicName, playerName + ",poseOK", qos=1)
    return
###################### POSE ######################


################### MQTT ###################
def on_connect(client, userdata, flags, rc):
    print("Connection returned result: "+str(rc))
    print("Connected To Topic: " + topicName + " As " + playerName)
    print("\n")
    client.subscribe(topicName, qos=1)
    client.publish(topicName, "playerName," + playerName, qos=1)

def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

def on_message(client, userdata, message):
    print('Received message: "' + str(message.payload) + '" on topic "' + message.topic + '" with QoS ' + str(message.qos))
    print(message.payload.decode("UTF-8"))
    client.message = message.payload.decode("UTF-8")
    
    if (client.message == playerName + ",startClassifier"):
        callClassifier()
    if (client.message == playerName + ",startButtons"):
        callButtons()
    if (client.message == playerName + ",startPose"):
        callPose()
    

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
    #client.message=""
    pass
    
client.loop_stop()
client.disconnect()  
################### MQTT ###################
