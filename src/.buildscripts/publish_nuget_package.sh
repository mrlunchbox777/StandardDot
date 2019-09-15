cd src

if [ "$NUGET_API_KEY" == "" ]
then
    echo "----------------"
    echo "unable to publish without the environment variable NUGET_API_KEY set"
    echo "----------------"
fi

# deployableDirectories="$(ls -d */ | grep -v Tests)"
for i in $(ls -d */ | grep -v Tests)
do
    echo "----------------"
    echo "publishing ${i}"
    echo "----------------"
    cd "${i}"

    dotnet build -c Release --force --no-incremental
    dotnet pack --no-build --no-restore -o ./

    nugetPackages="$(find . -regex '.*\.nupkg' -print)"

    if [ "$nugetPackages" == "" ]
    then
        echo "----------------"
        echo "failed to publish ${i}"
        echo "----------------"
    else
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