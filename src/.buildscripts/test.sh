cd src
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

dotnet build-server shutdown

# run sonarqube
if [ "${SONAR_LOGIN}" != "" ]
then
    ignorables=" \
        *Tests/ , .buildscripts/ , .sonarqube/ , .vscode/ , **/README.md \
        , **/*.csproj , *.sh , *.yml , *.Dockerfile , *.sln , .env , .env.example \
        "

    dotnet sonarscanner begin \
        -k:"${SONAR_PROJECTKEY}" \
        -o:"${SONAR_ORGANIZATION}" \
        -d:sonar.host.url="${SONAR_HOST}" \
        -d:sonar.login="${SONAR_LOGIN}" \
        -d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
        -d:sonar.coverage.exclusions="${ignorables}" \
        -d:sonar.branch.name="${GIT_BRANCH_NAME}" \
        -d:sonar.branch.target="${GIT_BRANCH_TARGET}"

    dotnet build ./StandardDot.sln

    dotnet sonarscanner end \
        -d:sonar.login="${SONAR_LOGIN}"
else
    echo "Skipping Sonar Scanner"
fi
cd ..