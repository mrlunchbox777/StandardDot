FROM microsoft/dotnet:2.2-sdk-alpine

ENV TEST_DIRS=""

WORKDIR /tempApp
COPY . .
RUN ./.buildscripts/restore.sh

WORKDIR /app
CMD ./.buildscripts/test.sh