import tensorflow as tf
import imageio
from tensorflow import keras
from keras.models import Sequential
from keras import layers, Model, optimizers, callbacks
from keras.layers import Dense, Dropout, Flatten, Input
from keras.layers import Conv1D, MaxPooling1D, BatchNormalization, Embedding
from keras.preprocessing import sequence
from sklearn.model_selection import train_test_split
import numpy as np
import cv2 
import pandas as pd
import os
from keras.models import load_model
import h5py
#from formDataset.py import df_to_dataset, demo

# Load Data
data = pd.read_csv(r'C:\Users\stink\180DA\ActivityRecognition\TrainingData.csv')
data['Label'] = pd.Categorical(data['Label'])
data['Label'] = data.Label.cat.codes
#train, val = train_test_split(data,test_size=.2)
xtrain = data.loc[:,'1x':'25conf']
ytrain = data.loc[:,'Label']

#Convert to numpy array 
trainarraydata = xtrain.to_numpy()
trainarraylabels = ytrain.to_numpy()
trainarraydatanew = np.reshape(trainarraydata,(5069,75,1))
#print(trainarraydatanew[0])

# Define Model Architecture
model = Sequential()
model.add(Conv1D(16, 5, activation='relu',input_shape=(75,1)))
#model.add(MaxPooling1D(5))
model.add(BatchNormalization())
model.add(Dropout(.25))
model.add(Flatten())
model.add(Dense(1,activation='sigmoid'))

print(model.summary())

opt = optimizers.Adam(learning_rate=.0001)
model.compile(loss=tf.keras.losses.BinaryCrossentropy(from_logits=True),
              optimizer=opt,
              metrics=['acc'])

model.fit(trainarraydatanew, trainarraylabels,validation_split=.2, shuffle=True, batch_size=32, 
    epochs=50)
model.save('ActRecognition.h5')
