# build the project to get everything cached
dotnet build ./StandardDot.sln

# run the tests
for i in $(ls -d1 *Tests/)
do
    echo "-----------------------------------------------"
    echo "running $i"
    cd "$i"
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
    cd ..
    echo "completed running $i"
    echo "-----------------------------------------------"
done

# run sonarqube
if [ "${DSONAR_LOGIN}" != "" ]
then
    # sonar-scanner \
    #     -Dsonar.projectKey=${DSONAR_PROJECTKEY} \
    #     -Dsonar.organization=${DSONAR_ORGANIZATION} \
    #     -Dsonar.sources=. \
    #     -Dsonar.host.url=${DSONAR_HOST} \
    #     -Dsonar.login=${DSONAR_LOGIN}
    dotnet sonarscanner begin \
        -d:sonar.projectKey=${DSONAR_PROJECTKEY} \
        -d:sonar.organization=${DSONAR_ORGANIZATION} \
        -d:sonar.sources=. \
        -d:sonar.host.url=${DSONAR_HOST} \
        -d:sonar.login=${DSONAR_LOGIN} \
        -d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"
        -d:sonar.coverage.exclusions="**Tests*.cs"
    dotnet build ./StandardDot.sln
    dotnet sonarscanner end
else
    echo "Skipping Sonar Scanner"
fi
