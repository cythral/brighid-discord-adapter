#!/bin/bash

set -eo pipefail

if [ "$CODEBUILD_GIT_BRANCH" = "master" ]; then
    docker push $GATEWAY_ADAPTER_IMAGE_TAG;
fi