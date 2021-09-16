#!/bin/bash

cleanup()
{
    docker-compose down
}

pushd "$(dirname ${BASH_SOURCE[0]})/../"
docker-compose up -d
trap cleanup EXIT
dotnet watch msbuild &
docker-compose logs --follow -- adapter
