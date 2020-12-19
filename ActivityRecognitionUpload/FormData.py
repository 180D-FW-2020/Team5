import pandas as pd
import json
import csv
import os
import sklearn
import numpy as np


def makeData(data1, dataset, datasetFinal,csvFile,count=0):
    for filename in data1:
        framePath = ('C:/users/stink/180da/ActivityRecognition/RealTimeOutput/' + filename)
        with open(framePath,'r') as f:
            frame = json.load(f)
        dataset[count] = frame
        count+=1

    for i in range(len(dataset)):
        if dataset[i]['people']:
            keypoints = dataset[i]['people'][0]['pose_keypoints_2d'] # extract keypoints for a given frame
            datasetFinal.append(keypoints)

    file = open(csvFile,'w+', newline='')
    with file: 
        write = csv.writer(file)
        write.writerows(datasetFinal)
    return 
