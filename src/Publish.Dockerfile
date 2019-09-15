FROM microsoft/dotnet:2.2-sdk-alpine

RUN apk update && apk upgrade && apk add jq

ARG NUGET_PACKAGE_SOURCE

ENV NUGET_PACKAGE_SOURCE="${NUGET_PACKAGE_SOURCE}"

WORKDIR /app
COPY . .
RUN ./src/.buildscripts/restore.sh
CMD ./src/.buildscripts/publish_nuget_package.sh