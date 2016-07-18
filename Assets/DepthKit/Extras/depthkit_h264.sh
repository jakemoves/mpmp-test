#!/bin/bash

#use this script to turn DepthKit exports into DepthKitJS compatible WebM files
#	if you don't have FFMPEG, install homebrew
# 	$ brew install ffmpeg --with-libvorbis --with-libvpx --with-fdk-aac --with-theora


if [ "$#" -ne 5 ]; then 
    echo "Wrong number of parameters!"
    echo "$ ./depthkit_h264.sh <image_sequence_path> <start_frame> <end_frame> <framerate> <output_movie_name>"
    exit 1
fi

#user defined variables
SEQUENCE_PATH=$1
START_FRAME=$2
END_FRAME=$3
FRAME_RATE=$4
OUTPUT_MOVIE_PATH=$5

START_SECOND=$(bc <<< "scale=4;$START_FRAME / $FRAME_RATE")
END_SECOND=$(bc <<< "scale=4;$END_FRAME / $FRAME_RATE")
DURATION_IN_SECONDS=$(bc <<< "scale=4;$END_SECOND - $START_SECOND")

echo "Exporting DepthKit Video" 
echo "    Input Sequence    $SEQUENCE_PATH"
echo "    Start Frame       $START_FRAME"
echo "    End Frame         $END_FRAME"
echo "    Frame Rate        $FRAME_RATE"
echo "    Duration (sec)    $DURATION_IN_SECONDS"
echo "    Output movie      $OUTPUT_MOVIE_PATH"

echo "ENCODING VIDEO...."
ffmpeg -y -start_number $START_FRAME -f image2 -r ${FRAME_RATE} -i "${SEQUENCE_PATH}/save.%05d.png" -codec:v libx264 -pix_fmt yuv420p -b 8000k -r $FRAME_RATE "${OUTPUT_MOVIE_PATH}.mp4"

echo "EXTRACTING POSTER FRAME...."
ffmpeg -y -i "${OUTPUT_MOVIE_PATH}.mp4" -frames:v 1 "${OUTPUT_MOVIE_PATH}.png"

echo "COPYING META FILE"
cp "${SEQUENCE_PATH}/_meta.txt" "${OUTPUT_MOVIE_PATH}.txt"

