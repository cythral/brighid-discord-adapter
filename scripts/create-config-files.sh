#!/bin/bash

cwd=$(dirname "${BASH_SOURCE[0]}")

image=$1
version=$CODEBUILD_BUILD_NUMBER

get_lambdajection_version()
{
    pushd $cwd/../ >/dev/null
    dotnet list package | grep Lambdajection | head -n 1 | awk '{ print $4 }'
    popd >/dev/null
}

get_dotnet_version()
{
    pushd $cwd/../ >/dev/null
    dotnet --version
    popd >/dev/null
}

for file in $(ls ${cwd}/../deploy/params/*.json); do
    envName=$(echo $file | xargs basename | sed "s/\.json//")
    params=$(cat $file)
    params=$(echo $params | jq ".Image=\"$image\"")
    params=$(echo $params | jq ".LambdajectionVersion=\"$(get_lambdajection_version)\"")
    params=$(echo $params | jq ".DotnetVersion=\"$(get_dotnet_version)\"")
    
    config=$(cat $cwd/../deploy/config.json)
    config=$(echo $config | jq --argjson params "$params" '.Parameters=$params')
    echo $config > brighid-discord-adapter.${envName}.config.json
done