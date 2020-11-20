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
#from scipy import ndimage


### Data Preprocessing
orgs = pd.read_csv(r'C:\Users\stink\180DA\RaspberryPutt.csv')
#Images = []
#Labels = []
#ThumbsUpImagePaths = '/content/drive/My Drive/RaspberryPutt/Thumbs Up/'
#FistImagePaths = '/content/drive/My Drive/RaspberryPutt/Fist/'

# Get Thumbs Up Images
# count = 1
# outpath = '/content/drive/My Drive/RaspberryPutt/New Thumbs Up/'
# for image_path in os.listdir(ThumbsUpImagePaths):

#         # create the full input path and read the file
#         input_path = os.path.join(ThumbsUpImagePaths, image_path)
#         image_to_rotate = imageio.imread(input_path)

#         # rotate the image
#         rotated = scipy.ndimage.interpolation.rotate(image_to_rotate, 90)

#         # create full output path, 'example.jpg' 
#         # becomes 'rotate_example.jpg', save the file to disk
#         fullpath = os.path.join(outpath, 'rotated_'+image_path)
#         imageio.imwrite(fullpath, rotated)
  #img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) # Converts into the correct colorspace (GRAY)
  #img = cv2.resize(img, (320, 120)) # Reduce image size so training can be faster
  #Images.append(img)

# # Get Fist Images
# for k in FistImagePaths:
#   img = cv2.imread(k)
#   img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) # Converts into the correct colorspace (GRAY)
#   img = cv2.resize(img, (320, 120)) # Reduce image size so training can be faster
#   Images.append(img)

# train_datagenerator = ImageDataGenerator(
#     rescale=1./255,
#     rotation_range=20,
#     width_shift_range=0.2,
#     height_shift_range=0.2,
#     shear_range=0.2,
#     zoom_range=0.2,
#     fill_mode='nearest',
#     validation_split = .2)

gen = ImageDataGenerator(
    rescale=1./255, 
    shear_range=.4,
    zoom_range=.2,
    validation_split=.25
)

test_datagenerator = ImageDataGenerator(
    rescale=1./255
)

training_data_new = gen.flow_from_dataframe(orgs,directory=r'C:\Users\stink\180DA\Images', x_col='Image',
                                                      y_col='Label',target_size=(120,120), class_mode='raw',subset='training')
validation_data_new = gen.flow_from_dataframe(orgs,directory=r'C:\Users\stink\180DA\Images', x_col='Image',
                                                      y_col='Label', target_size=(120,120),class_mode='raw',subset='validation')
###Construction of network architecture
img_input = layers.Input(shape=(120, 120, 3))

# First convolution extracts 16 filters that are 3x3
# Convolution is followed by max-pooling layer with a 2x2 window
x = layers.Conv2D(32, 5, activation='relu')(img_input)
x = layers.MaxPooling2D(2)(x)

# Second convolution extracts 32 filters that are 3x3
# Convolution is followed by max-pooling layer with a 2x2 window
x = layers.Conv2D(32, 3, activation='relu')(x)
x = layers.MaxPooling2D(2)(x)

# Third convolution extracts 64 filters that are 3x3
# Convolution is followed by max-pooling layer with a 2x2 window
x = layers.Convolution2D(64, 3, activation='relu')(x)
x = layers.MaxPooling2D(2)(x)

x = layers.Dropout(.25)(x)

# Flatten feature map to a 1-dim tensor
x = layers.Flatten()(x)

# Create a fully connected layer with ReLU activation and 128 hidden units
x = layers.Dense(128, activation='relu')(x)

# Add a dropout rate of 0.5
x = layers.Dropout(0.5)(x)

# Create output layer with a single node and sigmoid activation
output = layers.Dense(1, activation='sigmoid')(x)

# Configure model
model = Model(img_input, output)

### Compile model
model.compile(loss='binary_crossentropy',
              optimizer=optimizers.RMSprop(lr=.001),
              metrics=['acc'])

### Train model
model.fit(training_data_new,epochs=20,validation_data=validation_data_new)

### Testing 
