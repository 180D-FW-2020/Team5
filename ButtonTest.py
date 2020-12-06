# To recognize gpiozero commands, install gpiozero
# with the command 'pip3 install gpiozero'

#pip install RPi.GPIO
#pip3 install RPIO
#pip3 install pigpio


import gpiozero
import time

left_press = False
right_press = False
left_max = False
right_max = False
run_sequence = True

t_Left_start = 0
t_Left_end = 0
t_Left = 0
t_Right_start = 0
t_Right_end = 0
t_Right = 0
t_Max = 1000        #Maximum Time between ticks in milliseconds

left_button = gpiozero.DigitalInputDevice(17)
right_button = gpiozero.DigitalInputDevice(27)

print("Choose Your Direction! Press both buttons simultaneously to finish\n")

while run_sequence == True:

    if left_press == True and (int(round(time.time()*1000)) - t_Left_start) > t_Max:
        left_max = True
    if right_press == True and (int(round(time.time()*1000)) - t_Right_start) > t_Max:
        right_max = True

    if left_button.value == 1 and left_press == True and right_button.value == 1 and right_press == True:
        print("Direction Control Completed\n")
        run_sequence = False        

    elif left_button.value  == 1 and  left_press == False:
        #print("Left Button Pressed\n")
        left_press = True
        t_Left_start = int(round(time.time()*1000))
    elif left_max == True or (left_button.value == 0 and left_press == True):
        left_press = False
        left_max = False
        t_Left_end = int(round(time.time()*1000))
        t_Left = t_Left_end - t_Left_start
        #print("Left Button Released, Time: {}\n".format(t_Left))
        print("turn,left,{}\n".format(t_Left))

    elif right_button.value == 1 and right_press == False:
        #print("Right Button Pressed\n")
        right_press = True
        t_Right_start = int(round(time.time()*1000))
    elif right_max == True or (right_button.value == 0 and right_press == True):
        right_press = False
        right_max = False
        t_Right_end = int(round(time.time()*1000))
        t_Right = t_Right_end - t_Right_start
        #print("Right Button Released, Time: {}\n".format(t_Right))
        print("turn,right,{}\n".format(t_Right))

