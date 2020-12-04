# To recognize gpiozero commands, install gpiozero
# with the command 'pip3 install gpiozero'
import gpiozero

left_flag = False
right_flag = False
run_sequence = True

left_button = gpiozero.DigitalInputDevice(17)
right_button = gpiozero.DigitalInputDevice(27)

while run_sequence == True:

    if left_button.value == 1 and left_flag == True and right_button.value == 1 and right_flag == True:
        print("Direction Control Period Completed")
        run_sequence = False

    elif left_button.value  == 1 and  left_flag == False:
        print("Left Button Pressed\n")
        left_flag = True
    elif left_button.value == 0 and left_flag == True:
        print("Left Button Released\n")
        left_flag = False

    elif right_button.value == 1 and right_flag == False:
        print("Right Button Pressed\n")
        right_flag = True
    elif right_button.value == 0 and right_flag == True:
        print("Right Button Released\n")
        right_flag = False


