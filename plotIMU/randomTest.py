#import matplotlib
#matplotlib.use('Agg')

import matplotlib.pyplot as plt
import numpy as np
plt.ion() ## Note this correction
fig=plt.figure()
plt.axis([0,100,0,1])

i=0
x=list()
y=list()

while i <100:
    temp_y=np.random.random()
    x.append(i)
    y.append(temp_y)
    i+=1

plt.scatter(x,y)

plt.show()
#plt.pause(0.0001) #Note this correction
