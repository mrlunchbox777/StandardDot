# pulled from https://stackoverflow.com/questions/296536/how-to-urlencode-data-for-curl-command
rawurlencode() {
  local string="${1}"
  local strlen=${#string}
  local encoded=""
  local pos c o

  for (( pos=0 ; pos<strlen ; pos++ )); do
     c=${string:$pos:1}
     case "$c" in
        [-_.~a-zA-Z0-9] ) o="${c}" ;;
        * )               printf -v o '%%%02x' "'$c"
     esac
     encoded+="${o}"
  done
  echo "${encoded}"    # You can either set a return variable (FASTER) 
  REPLY="${encoded}"   #+or echo the result (EASIER)... or both... :p
}

cd src

# need an API Key to publish
if [ "$NUGET_API_KEY" == "" ]
then
    echo "----------------"
    echo "unable to publish without the environment variable NUGET_API_KEY set"
    echo "----------------"
    exit 1
fi

urlEncodedBranchName="$(rawurlencode ${GIT_BRANCH_NAME})"
qualityGate="$(wget -qO- https://sonarcloud.io/api/qualitygates/project_status?projectKey=${SONAR_PROJECTKEY}&branch=${urlEncodedBranchName})"
qualityGateStatus="$(echo ${qualityGate} | jq -r '.projectStatus.status')"

# need quality gate passing
if [ "${qualityGateStatus}" != "OK" ]
then
    echo "----------------"
    echo "quality gate failed with status ${qualityGateStatus}"
    echo "----------------"
    exit 1
else
    echo "----------------"
    echo "quality has status ${qualityGateStatus}"
    echo "----------------"
fi

# Attempt to publish everything except Test Projects
for i in $(ls -d */ | grep -v Test)
do
    echo "----------------"
    echo "publishing ${i}"
    echo "----------------"
    cd "${i}"

    dotnet build -c Release --force --no-incremental
    dotnet pack -o ./

    # Make sure we have a nuget package to publish
    nugetPackages="$(find . -regex '.*\.nupkg' -print)"

    if [ "$nugetPackages" == "" ]
    then
        echo "----------------"
        echo "failed to publish ${i}"
        echo "----------------"
    else
        # publish the first package found
        nugetPackage="$(echo $nugetPackages | cut -d$'\n' -f1)"
        echo "pushing $nugetPackage to $NUGET_PUSH_SOURCE"
        dotnet nuget push "$nugetPackage" -k "$NUGET_API_KEY" -s "$NUGET_PUSH_SOURCE"
        echo "----------------"
        echo "published ${i}"
        echo "----------------"
    fi

    cd ..
done

cd ..