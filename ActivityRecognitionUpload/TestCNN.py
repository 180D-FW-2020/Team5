import tensorflow as tf
from tensorflow import keras
import pandas as pd
from keras.models import load_model
import cv2
from FormData import makeData
import numpy as np
import os
import time

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
    # Decide to transmit over MQTT 
    #print(prediction)
    for i in range(iter+10,len(prediction)):
        #print(prediction[i][1])
        spredict = prediction[i][1]
        if spredict > 1e-12 and spredict < 1e-2 and spredict != 0: 
            ###MQTT CODE HERE 
            #client.publish('ece180da_team5', "poseOK", qos=1)
            #print(i)
            print(spredict)
            print('Detected Pose')
    iter+=1
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
    
cap.release()
cv2.destroyAllWindows()

