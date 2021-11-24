#!/bin/bash

set -eo pipefail
image=$1

if [ "$CODEBUILD_GIT_BRANCH" = "master" ]; then
    docker push $image;
fi