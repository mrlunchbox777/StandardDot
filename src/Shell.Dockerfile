FROM microsoft/dotnet:2.2-sdk-alpine

RUN apk update && apk upgrade && apk add bash

ARG NUGET_PACKAGE_SOURCE

ENV NUGET_PACKAGE_SOURCE="${NUGET_PACKAGE_SOURCE}"

WORKDIR /app
COPY . .
RUN ./src/.buildscripts/restore.sh
CMD cd src && dotnet restore && cd .. && tail -f /dev/null