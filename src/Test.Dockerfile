FROM microsoft/dotnet:2.2-sdk-alpine

ARG SONAR_SCANNER_HOME="${HOME}/.sonar/sonar-scanner-${SONAR_SCANNER_VERSION}-linux"
ARG SONAR_SCANNER_OPTS="-server"
ARG SONAR_SCANNER_VERSION="4.0.0.1744"
ARG INSTALL_SONAR_SCANNER="false"

ENV SONAR_SCANNER_HOME="${SONAR_SCANNER_HOME}"
ENV SONAR_SCANNER_OPTS="${SONAR_SCANNER_OPTS}"
ENV SONAR_SCANNER_VERSION="${SONAR_SCANNER_VERSION}"

ENV PATH="${SONAR_SCANNER_HOME}/bin:${PATH}"

WORKDIR /tempApp
COPY ./.buildscripts ./.buildscripts
RUN    echo "SONAR_SCANNER_VERSION - ${SONAR_SCANNER_VERSION}" \
    && echo "SONAR_SCANNER_HOME - ${SONAR_SCANNER_HOME}" \
    && echo "SONAR_SCANNER_OPTS - ${SONAR_SCANNER_OPTS}" \
    && echo "DSONAR_LOGIN - ${DSONAR_LOGIN}" \
    && echo "INSTALL_SONAR_SCANNER - ${INSTALL_SONAR_SCANNER}" \
    && echo "PATH - ${PATH}"
RUN if [ "${INSTALL_SONAR_SCANNER}" == "true" ]; then ./.buildscripts/install_sonar_scanner.sh; else echo "Skipping Sonar Scanner"; fi
# RUN ./.buildscripts/install_sonar_scanner.sh

WORKDIR /tempApp
COPY . .
RUN ./.buildscripts/restore.sh

WORKDIR /app
CMD ./.buildscripts/test.sh