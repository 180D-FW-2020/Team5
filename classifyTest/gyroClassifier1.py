import time
import IMU

import paho.mqtt.client as mqtt
import numpy as np

i = 0
ctr = 0
noMotion = True
backSwing = False
downSwing = False

x=list()
Listx = []
Listy = []
Listz = []

IMU.detectIMU()
if (IMU.BerryIMUversion == 99):
	print("No BerryIMU found... exiting ")
	sys.exit()
IMU.initIMU()

print("Data reads in 3 seconds...")
time.sleep(1)
print("...2...")
time.sleep(1)
print("...1...")
time.sleep(1)
print("Data Reading Begins:\n")

while i<2000:
	#GRYx = IMU.readGYRx()
	#GYRy = IMU.readGYRy()
	GYRz = IMU.readGYRz()

	#Listx.append(round(GYRx))
	#Listy.append(round(GYRy))
	Listz.append(round(GYRz))

	time.sleep(0.003)
	i+=1

print("Data Reading Completed: ")
#print("\nX Gyro:: ")
#print(Listx)
#print("\nY Gyro:: ")
#print(Listy)
print("\nZ Gyro:: ")
print(Listz)
