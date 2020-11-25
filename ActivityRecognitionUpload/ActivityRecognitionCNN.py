import tensorflow as tf
import imageio
import keras
from keras.models import Sequential
from keras import layers, Model, optimizers, callbacks
from keras.layers import Dense, Dropout, Flatten
from keras.layers import Conv2D, MaxPooling2D
from tensorflow.keras.preprocessing.image import ImageDataGenerator
import numpy as np
import cv2 
import pandas as pd
import os

# Define Model Architecture
model = Sequential()
model.add(Conv1D(16, kernel_size=(3,3), activation='relu',input_shape=(18,2))
model.add(BatchNormalization())
model.add(Dropout(.25))
model.add(Flatten())
model.add(Dense(1,activation='sigmoid',name='output'))

