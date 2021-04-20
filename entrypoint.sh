#!/bin/bash

decrypt() {
    ciphertext=$1
    tempfile=$(mktemp)

    echo $ciphertext | base64 --decode > $tempfile
    echo $(aws kms decrypt --ciphertext-blob fileb://$tempfile --query Plaintext --output text | base64 --decode)
    rm $tempfile;
}

export Gateway__Token=$(decrypt ${Encrypted__Gateway__Token})
export Identity__ClientSecret=$(decrypt ${Encrypted__Identity__ClientSecret})

dotnet /app/Core.dll