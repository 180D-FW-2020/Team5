import tensorflow as tf
from tensorflow import keras
import pandas as pd
from keras.models import load_model
import cv2
from FormData import makeData
import numpy as np
import os
import time

data1 = os.listdir('C:/users/stink/180da/ActivityRecognition/RealTimeOutput')
csvFile = 'C:/users/stink/180da/activityrecognition/TestData.csv'
dataset = [[] for i in range(len(data1))]
datasetFinal = []

model = load_model('ActRecognition.h5')
cap = cv2.VideoCapture(0,cv2.CAP_DSHOW)
iter=0
while True:
    ret, img = cap.read()
    makeData(data1,dataset,datasetFinal,csvFile)
    testData = pd.read_csv(r'C:\Users\stink\180DA\ActivityRecognition\TestData.csv')
    testDataArray = testData.to_numpy()
    testDataFinal = np.reshape(testDataArray,(len(testDataArray),75,1))
    prediction = model.predict(testDataFinal)
    # Decide to transmit over MQTT 
    print(prediction[0][1])
    if prediction[0][1] > .01: 
        ###MQTT CODE HERE 
        print('OK to Putt!')
    iter+=1
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()

