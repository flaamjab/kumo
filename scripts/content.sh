#!/bin/bash

if [ $# -ne 1 ]; then
    echo "Usage content [input_file.docx]"
    exit 0
fi

input_file=$1
temp_dir=$(mktemp -d)

unzip -q -o -d $temp_dir $input_file

content_file=$temp_dir/word/document.xml
chmod u+r $content_file
xmllint --format $content_file

rm -rf $temp_dir
