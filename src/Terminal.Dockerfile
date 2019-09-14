FROM microsoft/dotnet:2.2-sdk-alpine

RUN apk update && apk upgrade && apk add bash

WORKDIR /tempApp
COPY . .
RUN ./.buildscripts/restore.sh

WORKDIR /app
CMD dotnet restore && tail -f /dev/null