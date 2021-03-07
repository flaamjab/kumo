#!/bin/bash

if [ $# -ne 2 ]; then
    echo "Usage: pack [input_dir] [output_dir]"
else
    input_dir=$1
    output_dir=$2

    pushd $1

    filename=$(basename $input_dir)
    zip -r $filename.docx *

    popd
    mv $1/$filename.docx $2
fi
