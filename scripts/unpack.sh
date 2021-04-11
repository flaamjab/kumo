#!/bin/bash

if [ $# -ne 2 ]; then
    echo "Usage: unpack [input_file.docx] [output_dir]"
else
    temp_dir=$(mktemp -d)

    unzip -q -o $1 -d $temp_dir
    chmod -R u+r+w $temp_dir

    input_file=$1
    input_file_name=$(basename $input_file)
    input_file_name=${input_file_name%.*}
    output_dir=$2/$input_file_name

    rm -rf $output_dir
    mkdir $output_dir
    cp -rf $temp_dir/* $output_dir

    echo "Unpacked document into $output_dir"
fi
