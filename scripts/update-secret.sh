#!/bin/bash

CWD=$(dirname ${BASH_SOURCE[0]:-$0})
POSITIONAL=()
while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    --env)
    ENVIRONMENT="$2"
    shift # past argument
    shift # past value
    ;;
    --secret-name)
    SECRET_NAME="$2"
    shift
    shift
    ;;
    *)
    POSITIONAL+=("$1")
    shift
    ;;
esac
done

cythral-sso-aws --env $ENVIRONMENT
ACCOUNT_NUMBER=$(aws sts get-caller-identity --output text --query Account)

cythral-sso-aws --env shared
KEY_ID=arn:aws:kms:us-east-1:$ACCOUNT_NUMBER:alias/SecretsKey
CIPHERTEXT=$(aws kms encrypt --plaintext "$(echo ${POSITIONAL[@]} | base64)" --key-id $KEY_ID --output text --query CiphertextBlob)
PARAMS_FILE=$CWD/../deploy/params/$ENVIRONMENT.json
PARAMS_FILE_UPDATED_CONTENT=$(cat $PARAMS_FILE | jq --arg name "$SECRET_NAME" --arg value "$CIPHERTEXT" '.[$name]=$value')


echo $PARAMS_FILE_UPDATED_CONTENT | jq . > $PARAMS_FILE