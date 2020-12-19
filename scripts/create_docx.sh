#!/bin/bash

if [ $# -lt 2 ]
then
    echo "Please provide the input folder path and output file path."

else
    START=$(pwd)

    cd $1

    zip -r _file.docx *

    cd $START
    mv $1/_file.docx $2
fi
