FROM microsoft/dotnet:2.2-sdk-alpine

ARG NUGET_PACKAGE_SOURCE

ENV NUGET_PACKAGE_SOURCE="${NUGET_PACKAGE_SOURCE}"

WORKDIR /tempApp
COPY . .
RUN ./.buildscripts/restore.sh

RUN apk update && apk upgrade && apk add bash

WORKDIR /app
CMD dotnet restore && tail -f /dev/null