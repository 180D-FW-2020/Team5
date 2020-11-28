import pandas as pd
import json
import csv
import os

data1 = os.listdir('C:/users/stink/180da/ReadyStance')
csvFile = 'C:/users/stink/180da/activityrecognition/Dataset2.csv'
dataset = [[] for i in range(len(data1))]
datasetFinal = []
count = 0
for filename in data1:
    framePath = ('C:/users/stink/180da/ReadyStance/' + filename)
    with open(framePath,'r') as f:
        frame = json.load(f)
    dataset[count] = frame
    count+=1
#for k in range(len(dataset)):
    #dataset[k] = dataset[k]['people']
for i in range(len(dataset)):
    keypoints = dataset[i]['people'][0]['pose_keypoints_2d'] # extract keypoints for a given frame
    datasetFinal.append(keypoints)
file = open(csvFile,'w+', newline='')
with file: 
    write = csv.writer(file)
    write.writerows(datasetFinal)


#dataset.write('C:/users/stink/180da/ActivityRecognition/dataset.txt')
#filedf.to_csv('C:/users/stink/180da/ActivityRecognition/Dataset.csv')


