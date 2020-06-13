import numpy as np
import argparse
import matplotlib.pyplot as plt
import cv2
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, Flatten
from tensorflow.keras.layers import Conv2D
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.layers import MaxPooling2D
from tensorflow.keras.preprocessing.image import ImageDataGenerator
import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'
os.environ['TF_FORCE_GPU_ALLOW_GROWTH'] = 'true'

import SpoutSDK

import argparse
# import pygame
# from pygame.locals import *
from OpenGL.GL import *
from OpenGL.GLU import *
import glfw

from pythonosc import dispatcher
from pythonosc import osc_server
from pythonosc.udp_client import SimpleUDPClient
import asyncio

# command line argument
ap = argparse.ArgumentParser()
ap.add_argument("--mode",help="train/display")
mode = ap.parse_args().mode

# plots accuracy and loss curves
def plot_model_history(model_history):
    """
    Plot Accuracy and Loss curves given the model_history
    """
    fig, axs = plt.subplots(1,2,figsize=(15,5))
    # summarize history for accuracy
    axs[0].plot(range(1,len(model_history.history['accuracy'])+1),model_history.history['accuracy'])
    axs[0].plot(range(1,len(model_history.history['val_accuracy'])+1),model_history.history['val_accuracy'])
    axs[0].set_title('Model Accuracy')
    axs[0].set_ylabel('Accuracy')
    axs[0].set_xlabel('Epoch')
    axs[0].set_xticks(np.arange(1,len(model_history.history['accuracy'])+1),len(model_history.history['accuracy'])/10)
    axs[0].legend(['train', 'val'], loc='best')
    # summarize history for loss
    axs[1].plot(range(1,len(model_history.history['loss'])+1),model_history.history['loss'])
    axs[1].plot(range(1,len(model_history.history['val_loss'])+1),model_history.history['val_loss'])
    axs[1].set_title('Model Loss')
    axs[1].set_ylabel('Loss')
    axs[1].set_xlabel('Epoch')
    axs[1].set_xticks(np.arange(1,len(model_history.history['loss'])+1),len(model_history.history['loss'])/10)
    axs[1].legend(['train', 'val'], loc='best')
    fig.savefig('plot.png')
    plt.show()

"""parsing and configuration"""
def parse_args():
    desc = "Spout for Python texture receiving example"
    parser = argparse.ArgumentParser(description=desc)

    parser.add_argument('--spout_size', nargs = 2, type=int, default=[640, 480], help='Width and height of the spout receiver')   

    parser.add_argument('--spout_name', type=str, default='Composition - Resolume Arena', help='Spout receiving name - the name of the sender you want to receive')  

    parser.add_argument('--window_size', nargs = 2, type=int, default=[640, 480], help='Width and height of the window')    

    return parser.parse_args()


"""main"""
async def show_spout():

    # parse arguments
    # args = parse_args()
    
    # window details
    # width = args.window_size[0] 
    # height = args.window_size[1] 
    width = 256
    height = 256
    display = (width,height)
    
    # window setup
    # pygame.init() 
    # pygame.display.set_caption('Spout Receiver')
    # pygame.display.set_mode(display, DOUBLEBUF|OPENGL)
    glfw.init()
    window = glfw.create_window(width, height, "spout receiver", None, None)
    if not window:
      print("couldnt open window")
      glfw.terminate()
    glfw.make_context_current(window)
    glEnable(GL_DEPTH_TEST)


    # OpenGL init
    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    glOrtho(0,width,height,0,1,-1)
    glMatrixMode(GL_MODELVIEW)
    glDisable(GL_DEPTH_TEST)
    glClearColor(0.0,0.0,0.0,0.0)
    glEnable(GL_TEXTURE_2D)

    # init spout receiver
    receiverName = "Unity"
    spoutReceiverWidth = 256
    spoutReceiverHeight = 256
    # create spout receiver
    spoutReceiver = SpoutSDK.SpoutReceiver()

	# Its signature in c++ looks like this: bool pyCreateReceiver(const char* theName, unsigned int theWidth, unsigned int theHeight, bool bUseActive);
    spoutReceiver.pyCreateReceiver(receiverName,spoutReceiverWidth,spoutReceiverHeight, False)

    # create texture for spout receiver
    textureReceiveID = glGenTextures(1)    
    
    # initalise receiver texture
    glBindTexture(GL_TEXTURE_2D, textureReceiveID)
    glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE)
    glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE)
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST)
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST)

    # copy data into texture
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, spoutReceiverWidth, spoutReceiverHeight, 0, GL_RED, GL_UNSIGNED_BYTE, None ) 
    glBindTexture(GL_TEXTURE_2D, 0)

    model.load_weights('model.h5')

    # prevents openCL usage and unnecessary logging messages
    cv2.ocl.setUseOpenCL(False)

    # dictionary which assigns each label an emotion (alphabetical order)
    emotion_dict = {0: "Angry", 1: "Disgusted", 2: "Fearful", 3: "Happy", 4: "Neutral", 5: "Sad", 6: "Surprised"}

    # cap.release()
    # cv2.destroyAllWindows()

    # dispatch = dispatcher.Dispatcher()
    # server = osc_server.AsyncIOOSCUDPServer(("127.0.0.1", 6969), dispatch, asyncio.get_event_loop())
    # transport, protocol = await server.create_serve_endpoint()
    oscclient = SimpleUDPClient("255.255.255.255", 6969, True)

    await loop(spoutReceiver, receiverName, spoutReceiverHeight, spoutReceiverWidth, textureReceiveID, emotion_dict, oscclient, window)

    # transport.close()
    # pygame.display.flip()        
    glfw.terminate()
    spoutReceiver.ReleaseReceiver()
    cv2.destroyAllWindows()

async def loop(spoutReceiver, receiverName, spoutReceiverHeight, spoutReceiverWidth, textureReceiveID, emotion_dict, oscclient, window):
    while(True):
        glfw.poll_events()
        glfw.swap_buffers(window)
        # for event in pygame.event.get():
        #     if event.type == pygame.QUIT:
        #         spoutReceiver.ReleaseReceiver()
        #         pygame.quit()
        #         quit()
        # Find haar cascade to draw bounding box around face
        
        # receive texture
        # Its signature in c++ looks like this: bool pyReceiveTexture(const char* theName, unsigned int theWidth, unsigned int theHeight, GLuint TextureID, GLuint TextureTarget, bool bInvert, GLuint HostFBO);
        spoutReceiver.pyReceiveTexture(receiverName, spoutReceiverWidth, spoutReceiverHeight, int(textureReceiveID), GL_TEXTURE_2D, False, 0)
        
        glBindTexture(GL_TEXTURE_2D, textureReceiveID)

        # copy pixel byte array from received texture - this example doesn't use it, but may be useful for those who do want pixel info      
        frame = glGetTexImage(GL_TEXTURE_2D, 0, GL_RED, GL_UNSIGNED_BYTE, outputType=None)  #Using GL_RGB can use GL_RGBA 
        
        # swap width and height data around due to oddness with glGetTextImage. http://permalink.gmane.org/gmane.comp.python.opengl.user/2423
        # data.shape = (data.shape[1], data.shape[0], data.shape[2])
        
        # setup window to draw to screen
        glActiveTexture(GL_TEXTURE0)

        # clean start
        glClear(GL_COLOR_BUFFER_BIT  | GL_DEPTH_BUFFER_BIT )
        # reset drawing perspective
        glLoadIdentity()
        
        # draw texture on screen
        # glPushMatrix() use these lines if you want to scale your received texture
        # glScale(0.3, 0.3, 0.3)
        glBegin(GL_QUADS)

        glTexCoord(0,0)        
        glVertex2f(0,0)

        glTexCoord(1,0)
        glVertex2f(spoutReceiverWidth,0)

        glTexCoord(1,1)
        glVertex2f(spoutReceiverWidth,spoutReceiverHeight)

        glTexCoord(0,1)
        glVertex2f(0,spoutReceiverHeight)
        # frame = glReadPixels(0, 0, 256, 256, GL_BGR, GL_UNSIGNED_BYTE, None)
        
        glEnd()

        # glPixelStorei(GL_PACK_ALIGNMENT, (frame.step & 3) ? 1 : 4)
        # glPixelStorei(GL_PACK_ROW_LENGTH, frame.step/frame.elemSize())
        cv2.flip(frame, True, 0)

        facecasc = cv2.CascadeClassifier('haarcascade_frontalface_default.xml')
        gray = frame #cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        faces = facecasc.detectMultiScale(gray,scaleFactor=1.3, minNeighbors=5)

        for (x, y, w, h) in faces:
            cv2.rectangle(frame, (x, y-50), (x+w, y+h+10), (255, 0, 0), 2)
            roi_gray = gray[y:y + h, x:x + w]
            cropped_img = np.expand_dims(np.expand_dims(cv2.resize(roi_gray, (48, 48)), -1), 0)
            prediction = model.predict(cropped_img)
            maxindex = int(np.argmax(prediction))
            cv2.putText(frame, emotion_dict[maxindex], (x+20, y-60), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2, cv2.LINE_AA)
            oscclient.send_message("/angry", float(prediction[0][0]))
            oscclient.send_message("/disgusted", float(prediction[0][1]))
            oscclient.send_message("/fearful", float(prediction[0][2]))
            oscclient.send_message("/happy", float(prediction[0][3]))
            oscclient.send_message("/neutral", float(prediction[0][4]))
            oscclient.send_message("/sad", float(prediction[0][5]))
            oscclient.send_message("/surprised", float(prediction[0][6]))

        cv2.imshow('Video', cv2.resize(frame,(1600,960),interpolation = cv2.INTER_CUBIC))
        # glPopMatrix() make sure to pop your matrix if you're doing a scale        
        # update window

# Define data generators
train_dir = 'data/train'
val_dir = 'data/test'

num_train = 28709
num_val = 7178
batch_size = 64
num_epoch = 50

train_datagen = ImageDataGenerator(rescale=1./255)
val_datagen = ImageDataGenerator(rescale=1./255)

train_generator = train_datagen.flow_from_directory(
        train_dir,
        target_size=(48,48),
        batch_size=batch_size,
        color_mode="grayscale",
        class_mode='categorical')

validation_generator = val_datagen.flow_from_directory(
        val_dir,
        target_size=(48,48),
        batch_size=batch_size,
        color_mode="grayscale",
        class_mode='categorical')

# Create the model
model = Sequential()

model.add(Conv2D(32, kernel_size=(3, 3), activation='relu', input_shape=(48,48,1)))
model.add(Conv2D(64, kernel_size=(3, 3), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Dropout(0.25))

model.add(Conv2D(128, kernel_size=(3, 3), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Conv2D(128, kernel_size=(3, 3), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Dropout(0.25))

model.add(Flatten())
model.add(Dense(1024, activation='relu'))
model.add(Dropout(0.5))
model.add(Dense(7, activation='softmax'))

# If you want to train the same model or try other models, go for this
if mode == "train":
    model.compile(loss='categorical_crossentropy',optimizer=Adam(lr=0.0001, decay=1e-6),metrics=['accuracy'])
    model_info = model.fit_generator(
            train_generator,
            steps_per_epoch=num_train // batch_size,
            epochs=num_epoch,
            validation_data=validation_generator,
            validation_steps=num_val // batch_size)
    plot_model_history(model_info)
    model.save_weights('model.h5')

# emotions will be displayed on your face from the webcam feed
elif mode == "display":
    # model.load_weights('model.h5')

    # # prevents openCL usage and unnecessary logging messages
    # cv2.ocl.setUseOpenCL(False)

    # # dictionary which assigns each label an emotion (alphabetical order)
    # emotion_dict = {0: "Angry", 1: "Disgusted", 2: "Fearful", 3: "Happy", 4: "Neutral", 5: "Sad", 6: "Surprised"}

    # # start the webcam feed
    # cap = cv2.VideoCapture(0)
    # while True:
    #     # Find haar cascade to draw bounding box around face
    #     ret, frame = cap.read()
    #     if not ret:
    #         break
    #     facecasc = cv2.CascadeClassifier('haarcascade_frontalface_default.xml')
    #     gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    #     faces = facecasc.detectMultiScale(gray,scaleFactor=1.3, minNeighbors=5)

    #     for (x, y, w, h) in faces:
    #         cv2.rectangle(frame, (x, y-50), (x+w, y+h+10), (255, 0, 0), 2)
    #         roi_gray = gray[y:y + h, x:x + w]
    #         cropped_img = np.expand_dims(np.expand_dims(cv2.resize(roi_gray, (48, 48)), -1), 0)
    #         prediction = model.predict(cropped_img)
    #         maxindex = int(np.argmax(prediction))
    #         cv2.putText(frame, emotion_dict[maxindex], (x+20, y-60), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2, cv2.LINE_AA)

    #     cv2.imshow('Video', cv2.resize(frame,(1600,960),interpolation = cv2.INTER_CUBIC))
    #     if cv2.waitKey(1) & 0xFF == ord('q'):
    #         break

    # cap.release()
    # cv2.destroyAllWindows()
    asyncio.run(show_spout())
