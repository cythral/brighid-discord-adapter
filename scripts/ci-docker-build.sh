#!/bin/bash

set -eo pipefail

cwd=$(dirname ${BASH_SOURCE[0]:-$0})
image=$1
dotnet_sdk_version=$(cat $cwd/../global.json | jq -r '.sdk.version')
docker build \
    --tag $image \
    --build-arg DOTNET_SDK_VERSION=$dotnet_sdk_version .

if [ "$CODEBUILD_GIT_BRANCH" = "master" ]; then
    docker push $image;
fi