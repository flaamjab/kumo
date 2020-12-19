#!/bin/bash

if [ $# -lt 2 ]
then
    echo "Please provide an input file path and an output folder path."
else
    unzip -o $1 -d $2
    chmod -R 754 $2
fi