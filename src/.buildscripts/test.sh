dotnet build ./StandardDot.sln

for i in $(ls -d1 *Tests/)
do
    # still need to put in sonarqube
    echo "-----------------------------------------------"
    echo "running $i"
    cd "$i"
    dotnet test
    cd ..
    echo "completed running $i"
    echo "-----------------------------------------------"
done

# tail -f /dev/null