import tensorflow as tf
import imageio
from tensorflow import keras
from keras.models import Sequential
from keras import layers, Model, optimizers, callbacks
from keras.layers import Dense, Dropout, Flatten, Input
from keras.layers import Conv1D, MaxPooling1D, BatchNormalization, Embedding
from keras.utils import to_categorical
from keras.preprocessing import sequence
from sklearn.model_selection import train_test_split
import numpy as np
import cv2 
import pandas as pd
import os
import matplotlib.pyplot as plt
import h5py
#from formDataset.py import df_to_dataset, demo

# Load Data
data = pd.read_csv(r'C:\Users\stink\180DA\ActivityRecognition\TrainingData.csv')
#data['Label'] = pd.to_categorical(data['Label'])
#data['Label'] = data.Label.cat.codes
#train, val = train_test_split(data,test_size=.2)
xtrain = data.loc[:,'1x':'25conf']
ytrain = data.loc[:,'Label']
print(ytrain)

#Convert to numpy array 
trainarraydata = xtrain.to_numpy()
trainarraylabels = pd.get_dummies(ytrain).to_numpy()
trainarraydatanew = np.reshape(trainarraydata,(5069,75,1))
#print(trainarraydatanew[0])

# Define Model Architecture
model = Sequential()
model.add(Conv1D(16, kernel_size=5, activation='relu',input_shape=(75,1),padding='same'))
model.add(MaxPooling1D(5))
#model.add(BatchNormalization())
#model.add(Dropout(.25))
model.add(Flatten())
model.add(Dense(32, activation='relu'))
model.add(Dropout(.5))
model.add(Dense(2,activation='softmax'))

print(model.summary())

opt = optimizers.Adam(learning_rate=.0001)
model.compile(loss='categorical_crossentropy',
              optimizer=opt,
              metrics=['acc'])

history = model.fit(trainarraydatanew, trainarraylabels,validation_split=.2, shuffle=True, batch_size=32, 
    epochs=50)

# Training Insights
print(history.history.keys())
model.save('ActRecognition.h5')

plt.plot(history.history['acc'])
plt.plot(history.history['val_acc'])
plt.title('Model Training Overview')
plt.ylabel('Accuracy')
plt.xlabel('epoch')
plt.legend(['train','test'], loc='upper left')
plt.show()
