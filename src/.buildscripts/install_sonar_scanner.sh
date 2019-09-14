echo "installing sonar scanner"

rm -rf $SONAR_SCANNER_HOME
mkdir -p $SONAR_SCANNER_HOME

rm -rf $HOME/.sonar
mkdir -p $HOME/.sonar

wget -O $HOME/.sonar/sonar-scanner.zip https://binaries.sonarsource.com/Distribution/sonar-scanner-cli/sonar-scanner-cli-$SONAR_SCANNER_VERSION-linux.zip

unzip $HOME/.sonar/sonar-scanner.zip -d $HOME/.sonar/
rm $HOME/.sonar/sonar-scanner.zip

dotnet tool install --global dotnet-sonarscanner

echo "finished installing sonar scanner"