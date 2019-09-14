FROM microsoft/dotnet:2.2-sdk-alpine

WORKDIR /tempApp
COPY . .
RUN ./.buildscripts/restore.sh

WORKDIR /app
CMD ./.buildscripts/test.sh