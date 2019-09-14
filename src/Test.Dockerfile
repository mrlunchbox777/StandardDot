FROM microsoft/dotnet:2.2-sdk-alpine

RUN apk add openjdk8-jre

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

WORKDIR /tempApp
COPY ./.buildscripts ./.buildscripts
RUN if [ "${INSTALL_SONAR_SCANNER}" == "true" ]; then ./.buildscripts/install_sonar_scanner.sh; else echo "Skipping Sonar Scanner"; fi

WORKDIR /tempApp
COPY . .
RUN ./.buildscripts/restore.sh

WORKDIR /app
CMD ./.buildscripts/test.sh