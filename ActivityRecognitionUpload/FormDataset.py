import pandas as pd
import json
import csv
import os
import sklearn
from sklearn.model_selection import train_test_split
import numpy as np

data1 = os.listdir('C:/users/stink/180da/OtherStance')
csvFile = 'C:/users/stink/180da/activityrecognition/OtherStanceDataset.csv'
dataset = [[] for i in range(len(data1))]
datasetFinal = []
count = 0

for filename in data1:
    framePath = ('C:/users/stink/180da/OtherStance/' + filename)
    with open(framePath,'r') as f:
        frame = json.load(f)
    dataset[count] = frame
    count+=1
#for k in range(len(dataset)):
    #dataset[k] = dataset[k]['people']

for i in range(len(dataset)):
    if dataset[i]['people']:
        keypoints = dataset[i]['people'][0]['pose_keypoints_2d'] # extract keypoints for a given frame
        datasetFinal.append(keypoints)
file = open(csvFile,'w+', newline='')
with file: 
    write = csv.writer(file)
    write.writerows(datasetFinal)

dataset = pd.read_csv(r'C:\Users\stink\180DA\ActivityRecognition\TrainingData.csv')
train, val = train_test_split(dataset, test_size=.2)
xtrain = train.loc[:,'1x':'25conf']
ytrain = train.loc[:,'Label']
xval = val.loc[:,'1x':'25conf']
yval = val.loc[:,'Label']

trainarraydata = xtrain.to_numpy()
trainarraylabels = ytrain.to_numpy()
valarraydata = xval.to_numpy()
valarraylabels = yval.to_numpy()
print(trainarraydata.shape)
print(trainarraylabels.shape)


def df_to_dataset(dataframe, shuffle=True, batch_size=32):
  dataframe = dataset.copy()
  labels = dataframe.pop('target')
  #ds = tf.data.Dataset.from_tensor_slices((dict(dataframe), labels))
  if shuffle:
    ds = ds.shuffle(buffer_size=len(dataframe))
  ds = ds.batch(batch_size)
  return ds

#train_ds = df_to_dataset(train)
#val_ds = df_to_dataset(val, shuffle=False)

#example_batch = next(iter(train_ds))[0]
def demo(feature_column):
  example_batch = next(iter(train_ds))[0]
  feature_layer = layers.DenseFeatures(feature_column)
  print(feature_layer(example_batch).numpy())




#dataset.set_option('display.max_rows',none,'display.max_columns',none)
#print(dataset.loc[5000,'1x':'25conf'])
#dataset.write('C:/users/stink/180da/ActivityRecognition/dataset.txt')
#filedf.to_csv('C:/users/stink/180da/ActivityRecognition/Dataset.csv')