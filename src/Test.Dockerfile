FROM microsoft/dotnet:2.2-sdk-alpine

RUN apk update && apk upgrade && apk add openjdk8-jre git

ARG SONAR_SCANNER_HOME="${HOME}/.sonar/sonar-scanner-${SONAR_SCANNER_VERSION}-linux"
ARG SONAR_SCANNER_OPTS="-server"
ARG SONAR_SCANNER_VERSION="4.0.0.1744"
ARG INSTALL_SONAR_SCANNER="false"
ARG NUGET_PACKAGE_SOURCE

ENV SONAR_SCANNER_HOME="${SONAR_SCANNER_HOME}"
ENV SONAR_SCANNER_OPTS="${SONAR_SCANNER_OPTS}"
ENV SONAR_SCANNER_VERSION="${SONAR_SCANNER_VERSION}"
ENV NUGET_PACKAGE_SOURCE="${NUGET_PACKAGE_SOURCE}"

ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR /app
COPY ./src/.buildscripts ./src/.buildscripts
RUN if [ "${INSTALL_SONAR_SCANNER}" == "true" ]; then ./src/.buildscripts/install_sonar_scanner.sh; else echo "Skipping Sonar Scanner"; fi
COPY . .
RUN ./src/.buildscripts/restore.sh
CMD ./src/.buildscripts/test.sh