# Adapted and modified from:
# https://stackoverflow.com/questions/51851198/opencv-set-camera-resolution-windows-vrs-linux
# https://opencv-python-tutroals.readthedocs.io/en/latest/py_tutorials/py_gui/py_video_display/py_video_display.html
# https://stackoverflow.com/questions/33311153/python-extracting-and-saving-video-frames

import cv2
from imagePrep import process_image_file
import numpy as np
import time

input = 0  # webcam input being used
vidcap = cv2.VideoCapture(input + cv2.CAP_DSHOW)
success,image = vidcap.read()
count = 0
pathOutFrames = 'C:\\Users\\Maria Medina\\Desktop\\Cam_images\\' # folder where output frames will be stored
pathOutModFrames = 'C:\\Users\\Maria Medina\\Desktop\\Mod_Cam_images\\' # folder where output modified frames will be stored
timeWait = 0  # time to wait before capturing a new frame

while success:

    # generic storage location for the output frames
    frame_location = pathOutFrames + "frame%d.jpg" % count
    mod_frame_location = pathOutModFrames + "frame%d.jpg" % count

    #cv2.imwrite(pathOutFrames + "frame%d.jpg" % count, image)     # save frame as JPEG file
    cv2.imwrite(frame_location, image)  # save frame as JPEG file

    # Process the frame as it is being read in
    label_class = process_image_file(frame_location)

    # writing on an image before saving it
    mod_image = cv2.imread(frame_location, cv2.IMREAD_COLOR)
    height, width, channels = mod_image.shape
    font = cv2.FONT_HERSHEY_SIMPLEX
    cv2.putText(mod_image, label_class, (0,450), font, 4, (255,255,155), 13, cv2.LINE_AA)

    # save a modified version of the image in another folder with text on it
    cv2.imwrite(mod_frame_location, mod_image) # save modified image in it's own folder

    # Display the original resulting frame
    cv2.imshow('frame', image)

    # Moved above: Process the frame as it is being read in
    # process_image_file(frame_location)

    time.sleep(timeWait)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

    success,image = vidcap.read()
    print('Read a new frame: ', success)
    count += 1

# When everything done, release the capture
vidcap.release()
cv2.destroyAllWindows()