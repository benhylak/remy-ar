import tensorflow as tf
import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'  # disable CPU AVX2 warning

graph_def = tf.GraphDef()
labels = []
filename = 'C:\\Users\\Maria Medina\\Desktop\\SecondModel\\model.pb'
labels_filename = 'C:\\Users\\Maria Medina\\Desktop\\SecondModel\\labels.txt'

#Import the TF graph
with tf.gfile.GFile(filename, 'rb') as f:
    graph_def.ParseFromString(f.read())
    tf.import_graph_def(graph_def, name='')

#Create a list of labels
with open (labels_filename, 'rt') as lf:
    for l in lf:
        labels. append(l.strip())